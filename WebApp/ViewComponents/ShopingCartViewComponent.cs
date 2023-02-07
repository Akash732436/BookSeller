using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApp.ViewComponents
{
    public class ShopingCartViewComponent:ViewComponent
    {
        IUnitOfWork _unitOfWork;

        public ShopingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claims != null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart)!=null)
                return View(HttpContext.Session.GetInt32(SD.SessionCart));

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
                return View(HttpContext.Session.GetInt32(SD.SessionCart));

            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }


    }
}
