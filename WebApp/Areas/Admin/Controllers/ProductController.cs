using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using BookSeller.Models.ViewModels;
using BookSeller.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
	{
		IUnitOfWork _unitOfWOrk;

		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWOrk, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWOrk = unitOfWOrk;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			IEnumerable<Product> product=_unitOfWOrk.Products.GetAll();
			return View(product);
		}

		public IActionResult Upsert(int? id)
		{
			ProductVM productVM = new()
			{
				Product = new(),
				CategoryList = _unitOfWOrk.Category.GetAll().Select(
					u => new SelectListItem
					{
						Text = u.Name,
						Value = u.Id.ToString()
					}
					),
				CoverList = _unitOfWOrk.Covers.GetAll().Select(
					u => new SelectListItem
					{
						Text = u.Name,
						Value = u.Id.ToString()
					}
					)
			};

			if(id==0 || id == null)
			{
				//create
				//ViewBag.CategoryList=CategoryList;
				//ViewData["CoverList"] = CoverList;
				return View(productVM);
			}
			else
			{
				productVM.Product=_unitOfWOrk.Products.GetFirstOrDefault(u => u.Id == id);
				return View(productVM);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]

		public IActionResult Upsert(ProductVM obj,IFormFile? file)
		{
			if(ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName=Guid.NewGuid().ToString();
					var uploads=Path.Combine(wwwRootPath, "Images/Products");
					var extension = Path.GetExtension(file.FileName);


					if (obj.Product.ImageUrl != null)
					{
						string Oldpath=Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));
						if(System.IO.File.Exists(Oldpath))
						{
							System.IO.File.Delete(Oldpath);
						}
					}

					using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
					{
						file.CopyTo(fileStreams);
					}

					obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
						

				}
				if (obj.Product.Id == 0)
				{
					_unitOfWOrk.Products.Add(obj.Product);
				}
				else
				{
					_unitOfWOrk.Products.Update(obj.Product);
				}
				_unitOfWOrk.Save();
				TempData["success"] = "Product created successfully.";
				return RedirectToAction("Index");
			}
			return View(obj);
		}

		#region API calls

		[HttpGet]

		public IActionResult GetAll()
		{
			var productList = _unitOfWOrk.Products.GetAll(includeProperties:"Category,Cover");
			return Json(new { data = productList });
		}
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var obj = _unitOfWOrk.Products.GetFirstOrDefault(u=>u.Id==id);

			if (obj == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
			if (System.IO.File.Exists(oldPath))
			{
				System.IO.File.Delete(oldPath);
			}

			_unitOfWOrk.Products.Remove(obj);
			_unitOfWOrk.Save();
			return Json(new { success = true, message = "Delete Successful" });

		}

		#endregion

	}
}
