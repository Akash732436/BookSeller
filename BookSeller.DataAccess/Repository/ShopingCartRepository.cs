using BookSeller.DataAccess.Repository.IRepository;
using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository
{
	public class ShopingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
		private readonly ApplicationDbContext _db;

		public ShopingCartRepository(ApplicationDbContext db):base(db)
		{
			_db = db;
		}

		public int DecrementCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count -= count;
			return count;
		}

		public int IncrementCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count += count;
			return count;
		}
	}
}
