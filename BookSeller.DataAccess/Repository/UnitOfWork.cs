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

		private readonly ApplicationDbContext _db;

		public UnitOfWork(ApplicationDbContext db)
		{
			_db = db;
			Category = new CategoryRepository(_db);
			Covers=new CoverRepository(_db);
		}

		public void Save()
		{
			_db.SaveChanges();
		}
	}
}
