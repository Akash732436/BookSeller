using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository.IRepository
{
	public interface IUnitOfWork
	{
		ICategoryRepository Category { get; }

		ICoverRepository Covers { get; }

		IProductRepository Products { get; }

		ICompanyRepository Company { get; }

        IApplicationUserRepository ApplicationUser { get;}
        IShoppingCartRepository ShoppingCart { get; }

        void Save();
	}
}
