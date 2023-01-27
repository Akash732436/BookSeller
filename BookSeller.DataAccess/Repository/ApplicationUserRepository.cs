using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
		private readonly ApplicationDbContext _db;

		public ApplicationUserRepository(ApplicationDbContext db):base(db)
		{
			_db = db;
		}

		
	}
}
