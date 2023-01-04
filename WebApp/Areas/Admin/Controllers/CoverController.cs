using BookSeller.DataAccess.Repository;
using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CoverController : Controller
	{
		private readonly IUnitOfWork unitOfWork;

		public CoverController(IUnitOfWork unitOfWork)
		{
			this.unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var covers = unitOfWork.Covers.GetAll();
			return View(covers);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Cover obj)
		{
			if (obj.Name.Length > 20)
			{
				ModelState.AddModelError("name", "Length can not be more than 20.");
			}
			if (ModelState.IsValid)
			{
				unitOfWork.Covers.Add(obj);
				unitOfWork.Save();
				TempData["Success"] = "A Cover type has been added.";
				return RedirectToAction("Index");
			}
			return View(obj);
		}

		public IActionResult Edit(int? id)
		{
			if(id==0 || id == null)
			{
				return NotFound();
			}

			var obj=unitOfWork.Covers.GetFirstOrDefault(x => x.Id==id);
			if(obj==null)
			{
				return NotFound();
			}
			return View(obj);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Cover obj)
		{
			if (obj.Name.Length > 20)
			{
				ModelState.AddModelError("name", "Length can not be more than 20.");
			}
			if(ModelState.IsValid)
			{
				unitOfWork.Covers.Update(obj);
				unitOfWork.Save();
				TempData["Success"] = "A Cover type has been updated.";
				return RedirectToAction("Index");
			}
			return View(obj);
		}

		public IActionResult Delete(int? id)
		{
			if (id == 0 || id == null)
			{
				return NotFound();
			}

			var obj = unitOfWork.Covers.GetFirstOrDefault(x => x.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			return View(obj);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Delete")]
		public IActionResult PostDelete(int? id)
		{
			if(id==null || id == 0)
			{
				return NotFound();
			}
			var cov=unitOfWork.Covers.GetFirstOrDefault(x=>x.Id==id);
			if(cov==null) 
			{
				return NotFound();
			}
			unitOfWork.Covers.Remove(cov);
			unitOfWork.Save();
			TempData["Success"] = "A Cover type has been removed.";
			return RedirectToAction("Index");
		}
	}
}
