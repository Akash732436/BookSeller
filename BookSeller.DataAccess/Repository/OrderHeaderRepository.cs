using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	internal class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _db;


		public OrderHeaderRepository(ApplicationDbContext db):base(db)
		{
			_db = db;

		}

		public void Update(OrderHeader obj)
		{
			_db.OrderHeaders.Update(obj);
		}

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderHeader=_db.OrderHeaders.FirstOrDefault(x=> x.Id == id);
			if (orderHeader != null)
			{
				orderHeader.OrderStatus = orderStatus;
				if(paymentStatus != null)
				{
					orderHeader.PaymentStatus = paymentStatus;
				}
			}
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			var orderfromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
			orderfromDb.SessionId = sessionId;
			orderfromDb.PaymentIntentId= paymentIntentId;
		}
	}
}
