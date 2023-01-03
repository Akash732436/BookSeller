using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CategoryController(IUnitOfWork _UnitOfWork)
        {
            this._UnitOfWork = _UnitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> CategoryList =_UnitOfWork.Category.GetAll();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("SameName", "Name and display order can not have same value");
            }

            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Add(obj);
                _UnitOfWork.Save();
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index");
            }


            return View(obj);

        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var category = _db.Categories.Find(id);
            var category = _UnitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Edit(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("SameName", "Name and display order can not have same value");
            }

            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Update(category);
                _UnitOfWork.Save();
                TempData["success"] = "Category Updated Successfully.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }
            var category = _UnitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            if (category == null) { return NotFound(); }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Delete(Category obj)
        {
            _UnitOfWork.Category.Remove(obj);
            _UnitOfWork.Save();
            TempData["success"] = "Category Deleted Successfully.";
            return RedirectToAction("Index");
        }

    }
}
