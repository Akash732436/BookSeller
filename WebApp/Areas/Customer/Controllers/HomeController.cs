using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;

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

        public IActionResult Details(int? id)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                Product = unitOfWork.Products.GetFirstOrDefault(x => x.Id == id, includeProperties:"Category,Cover")
            };

            return View(cartObj);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}