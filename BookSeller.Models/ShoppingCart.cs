using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSeller.Models
{
    public class ShoppingCart
    {
        public Product Product { get; set; }
        [Required]
        [Range(1, 100, ErrorMessage = "Can not Order in this quantity")]
        public int Count { get; set; }
    }
}
