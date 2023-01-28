using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		public ICategoryRepository Category { get; private set; }
		public ICoverRepository Covers { get; private set; }
		public IProductRepository Products { get; private set; }

		public ICompanyRepository Company { get; private set; }

		public IApplicationUserRepository ApplicationUser { get; private set; }
		public IShoppingCartRepository ShoppingCart { get; private set; }
		public IOrderDetailRepository OrderDetail { get; private set; }
		public IOrderHeaderRepository OrderHeader { get; private set; }

		private readonly ApplicationDbContext _db;

		public UnitOfWork(ApplicationDbContext db)
		{
			_db = db;
			Category = new CategoryRepository(_db);
			Covers=new CoverRepository(_db);
			Products=new ProductRepository(_db);
			Company= new CompanyRepository(_db);
			ApplicationUser= new ApplicationUserRepository(_db);
			ShoppingCart=new ShopingCartRepository(_db);
			OrderDetail=new OrderDetailRepository(_db);
			OrderHeader=new OrderHeaderRepository(_db);
		}

		public void Save()
		{
			_db.SaveChanges();
		}
	}
}
