using BookSeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.DataAccess.Repository.IRepository
{
	public interface ICoverRepository:IRepository<Cover>
	{
		public void Update(Cover obj);
	}
}
