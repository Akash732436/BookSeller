using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.Models.ViewModels
{
	public class ShopingCartVM
	{
		public IEnumerable<ShoppingCart> shoppingCarts;
		public double CartTotal { get; set; }
	}
}
