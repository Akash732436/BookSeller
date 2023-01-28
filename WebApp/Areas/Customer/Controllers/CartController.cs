﻿using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Models.ViewModels;
using BookSeller.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace WebApp.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public ShopingCartVM ShopingCartVM { get; set; }
		public int OrderTotal { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimIdentity=(ClaimsIdentity)User.Identity;
			var id = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShopingCartVM = new ShopingCartVM()
			{
				shoppingCarts=_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==id.Value,includeProperties:"Product"),
				orderHeader=new()
			};
			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
				ShopingCartVM.orderHeader.OrderTotal += (item.Price * item.Count);
			}
			return View(ShopingCartVM);
		}

		public IActionResult Summary()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var id = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShopingCartVM = new ShopingCartVM()
			{
				shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == id.Value, includeProperties: "Product"),
				orderHeader = new()
			};

			ShopingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == id.Value);

			ShopingCartVM.orderHeader.Name = ShopingCartVM.orderHeader.ApplicationUser.Name;
			ShopingCartVM.orderHeader.City= ShopingCartVM.orderHeader.ApplicationUser.City;
			ShopingCartVM.orderHeader.State = ShopingCartVM.orderHeader.ApplicationUser.State;
			ShopingCartVM.orderHeader.StreetAddress = ShopingCartVM.orderHeader.ApplicationUser.StreetAddress;
			ShopingCartVM.orderHeader.PostalCode = ShopingCartVM.orderHeader.ApplicationUser.PostalCode;
			ShopingCartVM.orderHeader.PhoneNumber = ShopingCartVM.orderHeader.ApplicationUser.PhoneNumber;


			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
				ShopingCartVM.orderHeader.OrderTotal += (item.Price * item.Count);
			}
			return View(ShopingCartVM);
		}
		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var id = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShopingCartVM.shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == id.Value, includeProperties: "Product");

			ShopingCartVM.orderHeader.PaymentStatus = SD.StatusPending;
			ShopingCartVM.orderHeader.OrderStatus= SD.StatusPending;
			ShopingCartVM.orderHeader.OrderDate = DateTime.Now;
			ShopingCartVM.orderHeader.ApplicationUserId = id.Value;

			


			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
				ShopingCartVM.orderHeader.OrderTotal += (item.Price * item.Count);
			}

			_unitOfWork.OrderHeader.Add(ShopingCartVM.orderHeader);
			_unitOfWork.Save();
			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = item.ProductId,
					OrderId = ShopingCartVM.orderHeader.Id,
					Price = item.Price,
					Count = item.Count
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}

			//stripe settings
			var domain = "https://localhost:44325/";
			var options=new SessionCreateOptions
			{
				PaymentMethodTypes=new List<string>
				{
					"card"
				},
				LineItems=new List<SessionLineItemOptions>()
				,
				Mode = "payment",
				SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={ShopingCartVM.orderHeader.Id}",
				CancelUrl = domain+"customer/cart/index",
			};
			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				{
					var sessionLineItem=new SessionLineItemOptions
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
			ShopingCartVM.orderHeader.SessionId = session.Id;
			ShopingCartVM.orderHeader.PaymentIntentId = session.PaymentIntentId;
			_unitOfWork.OrderHeader.UpdateStripePaymentID(ShopingCartVM.orderHeader.Id,session.Id,session.PaymentIntentId);
			_unitOfWork.Save();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);



			//_unitOfWork.ShoppingCart.RemoveRange(ShopingCartVM.shoppingCarts);
			//_unitOfWork.Save();
			//return RedirectToAction("Index","Home");
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);
			//check the stripe status
			if (session.PaymentStatus.ToLower() == "paid")
			{
				_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
				_unitOfWork.Save();
			}


			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Save();
			return View(id);

		}

		public double GetPriceBasedOnQuantity(double quantity,double price,double price50,double price100)
		{
			if (quantity < 50) return price;
			if (quantity < 100) return price50;
			return price100;
		}

		public IActionResult Plus(int cartId)
		{
			ShoppingCart cart=_unitOfWork.ShoppingCart.GetFirstOrDefault(u=>u.Id==cartId);
			_unitOfWork.ShoppingCart.IncrementCount(cart, 1);

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			ShoppingCart cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			if (cart.Count == 1)
			{
				_unitOfWork.ShoppingCart.Remove(cart);
			}
			else
			{
				_unitOfWork.ShoppingCart.DecrementCount(cart, 1);
			}
			

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			ShoppingCart cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
	}
}