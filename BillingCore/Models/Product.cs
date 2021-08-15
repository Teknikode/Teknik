using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Price> Prices { get; set; }

        public Product()
        {
            Prices = new List<Price>();
        }
    }
}
