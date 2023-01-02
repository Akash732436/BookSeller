using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.Models
{
	public class Cover
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[DisplayName("Cover Name")]
		public string Name { get; set; }

	}
}
