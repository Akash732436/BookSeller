using BookSeller.DataAccess.Repository;
using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products=unitOfWork.Products.GetAll(includeProperties:"Category,Cover");
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId= productId,
                Product = unitOfWork.Products.GetFirstOrDefault(x => x.Id == productId, includeProperties:"Category,Cover")
            };

            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]

        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claim =(ClaimsIdentity) User.Identity;
            var id = claim.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = id.Value;


            ShoppingCart cartFromDb = unitOfWork.ShoppingCart.GetFirstOrDefault(
                x=>x.ApplicationUserId==id.Value && x.ProductId==shoppingCart.ProductId
            );

            if (cartFromDb == null)
            {
				unitOfWork.ShoppingCart.Add(shoppingCart);
                unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                    unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==id.Value).ToList().Count());
			}
            else
            {
                unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                unitOfWork.Save();
            }
            
            //unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}