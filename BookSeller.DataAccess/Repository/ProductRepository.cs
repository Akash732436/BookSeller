using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	public class ProductRepository: Repository<Product>, IProductRepository
	{
		private readonly ApplicationDbContext _db;
		public ProductRepository(ApplicationDbContext db):base(db)
		{
			_db = db;
		}

		public void Update(Product product)
		{
			var obj=_db.Products.FirstOrDefault(u=>u.Id== product.Id);
			if (obj != null)
			{
				obj.ISBN = product.ISBN;
				obj.Author = product.Author;
				obj.Description = product.Description;
				obj.Price = product.Price;
				obj.Price50 = product.Price50;
				obj.Price100= product.Price100;
				obj.ListPrice = product.ListPrice;
				obj.Title= product.Title;
				obj.CategoryId = product.CategoryId;
				obj.CoverId= product.CoverId;
				if (obj.ImageUrl != null)
				{
					obj.ImageUrl = product.ImageUrl;
				}
			}
		}
	}
}
