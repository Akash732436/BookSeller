using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private ShopingCartVM ShopingCartVM;
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
				shoppingCarts=_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==id.Value,includeProperties:"Product")
			};
			foreach (var item in ShopingCartVM.shoppingCarts)
			{
				item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
				ShopingCartVM.CartTotal += (item.Price * item.Count);
			}
			return View(ShopingCartVM);
		}

		public IActionResult Summary()
		{
			//var claimIdentity = (ClaimsIdentity)User.Identity;
			//var id = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

			//ShopingCartVM = new ShopingCartVM()
			//{
			//	shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == id.Value, includeProperties: "Product")
			//};
			//foreach (var item in ShopingCartVM.shoppingCarts)
			//{
			//	item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
			//	ShopingCartVM.CartTotal += (item.Price * item.Count);
			//}
			return View();
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
