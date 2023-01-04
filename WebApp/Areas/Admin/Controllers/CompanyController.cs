using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CompanyController : Controller
	{
		IUnitOfWork _unitOfWOrk;

		public CompanyController(IUnitOfWork unitOfWOrk)
		{
			_unitOfWOrk = unitOfWOrk;
		}

		public IActionResult Index()
		{
			IEnumerable<Company> company=_unitOfWOrk.Company.GetAll();
			return View(company);
		}

		public IActionResult Upsert(int? id)
		{
			Company company = new();

			if(id==0 || id == null)
			{
				//create
				//ViewBag.CategoryList=CategoryList;
				//ViewData["CoverList"] = CoverList;
				return View(company);
			}
			else
			{
				company=_unitOfWOrk.Company.GetFirstOrDefault(u => u.Id == id);
				return View(company);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]

		public IActionResult Upsert(Company obj)
		{
			if(ModelState.IsValid)
			{
				
				
				if (obj.Id == 0)
				{
					_unitOfWOrk.Company.Add(obj);
				}
				else
				{
					_unitOfWOrk.Company.Update(obj);
				}
				_unitOfWOrk.Save();
				TempData["success"] = "Cmpany created successfully.";
				return RedirectToAction("Index");
			}
			return View(obj);
		}

		#region API calls

		[HttpGet]

		public IActionResult GetAll()
		{
			var companyList = _unitOfWOrk.Company.GetAll();
			return Json(new { data = companyList });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var obj = _unitOfWOrk.Company.GetFirstOrDefault(u=>u.Id==id);

			if (obj == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			

			_unitOfWOrk.Company.Remove(obj);
			_unitOfWOrk.Save();
			return Json(new { success = true, message = "Delete Successful" });

		}

		#endregion

	}
}
