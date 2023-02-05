using BookSeller.DataAccess.Repository;
using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Models.ViewModels;
using BookSeller.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderManagmentController : Controller
	{
		IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM OrderVM { get; set; }

		public OrderManagmentController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}


		public IActionResult Details(int orderId)
		{
			OrderVM =new OrderVM()
			{
				OrderHeader=_unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id==orderId,includeProperties:"ApplicationUser"),
				OrderDetails=_unitOfWork.OrderDetail.GetAll(u=>u.OrderId==orderId,includeProperties:"Product"),
			};

			return View(OrderVM);

		}
		[ActionName("Details")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Details_Pay_Now()
		{
			OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			OrderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");


			//stripe settings
			var domain = "https://localhost:44325/";
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string>
				{
					"card"
				},
				LineItems = new List<SessionLineItemOptions>()
				,
				Mode = "payment",
				SuccessUrl = domain + $"admin/orderManagment/PaymentConfirmation?orderId={OrderVM.OrderHeader.Id}",
				CancelUrl = domain + $"admin/orderManagment/details?orderId={OrderVM.OrderHeader.Id}",
			};
			foreach (var item in OrderVM.OrderDetails)
			{
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "inr",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title,
							},
						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);
				}

			}
			var service = new SessionService();
			Session session = service.Create(options);
	
			_unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.Save();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);


			//return View(OrderVM);

		}

		public IActionResult PaymentConfirmation(int orderId)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				//check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentID(orderId, orderHeader.SessionId, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(orderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}

			}

			_unitOfWork.Save();
			return View(orderId);
		}


			[HttpPost]
		[ValidateAntiForgeryToken]

		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,tracked:false);
			orderHeaderFromDB.Name=OrderVM.OrderHeader.Name;
			orderHeaderFromDB.PhoneNumber=OrderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDB.StreetAddress=OrderVM.OrderHeader.StreetAddress;
			orderHeaderFromDB.City=OrderVM.OrderHeader.City;
			orderHeaderFromDB.State=OrderVM.OrderHeader.State;
			orderHeaderFromDB.PostalCode=OrderVM.OrderHeader.PostalCode;

			if (OrderVM.OrderHeader.Carrier != null)
			{
				orderHeaderFromDB.Carrier=OrderVM.OrderHeader.Carrier;
			}
			if(OrderVM.OrderHeader.TrackingNumber!= null)
			{
				orderHeaderFromDB.TrackingNumber=OrderVM.OrderHeader.TrackingNumber;
			}

			_unitOfWork.OrderHeader.Update(orderHeaderFromDB);
			_unitOfWork.Save();

			TempData["Success"] = "Order Details Updated Successfully.";
			return RedirectToAction("Details", "OrderManagment", new { orderId = orderHeaderFromDB.Id });

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
			_unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();

			TempData["Success"] = "Order status Updated Successfully.";
			return RedirectToAction("Details", "OrderManagment", new { orderId = OrderVM.OrderHeader.Id });

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult ShipOrder()
		{
			var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
			orderHeaderFromDB.Carrier = OrderVM.OrderHeader.Carrier;
			orderHeaderFromDB.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			orderHeaderFromDB.OrderStatus = SD.StatusShipped;
			orderHeaderFromDB.ShippingDate=DateTime.Now;
			if (orderHeaderFromDB.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				orderHeaderFromDB.PaymentDueDate=DateTime.Now.AddDays(30);
			}

			_unitOfWork.OrderHeader.Update(orderHeaderFromDB);
			_unitOfWork.Save();

			TempData["Success"] = "Order Shipped Successfully.";
			return RedirectToAction("Details", "OrderManagment", new { orderId = orderHeaderFromDB.Id });

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
		public IActionResult CancelOrder()
		{
			var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
			if (orderHeaderFromDB.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeaderFromDB.PaymentIntentId
				};
				var service = new RefundService();
				Refund refund=service.Create(options);
				_unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			_unitOfWork.Save();

			TempData["Success"] = "Order Cancelled Successfully.";
			return RedirectToAction("Details", "OrderManagment", new { orderId = orderHeaderFromDB.Id });
		}
		#region API calls

		[HttpGet]

		public IActionResult GetAll(string status)
		{

			IEnumerable<OrderHeader> orderHeaders;
			

			if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
			}
			else
			{
				var claimIdentity =(ClaimsIdentity) User.Identity;
				var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
				orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "ApplicationUser");
			}

			switch (status)
			{
				case "pending":
					orderHeaders = orderHeaders.Where(u=>u.PaymentStatus==SD.PaymentStatusDelayedPayment);
					break;
				case "inprocess":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					
					break;
			}

			return Json(new { data = orderHeaders });
		}


		#endregion

	}
}
