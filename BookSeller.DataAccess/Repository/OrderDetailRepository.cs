using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	internal class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
	{
		private readonly ApplicationDbContext _db;


		public OrderDetailRepository(ApplicationDbContext db):base(db)
		{
			_db = db;

		}

		public void Update(OrderDetail obj)
		{
			_db.OrderDetail.Update(obj);
		}
	}
}
