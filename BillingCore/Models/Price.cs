using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class Price
    {
        public string ProductId { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public bool Recommended { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public long Storage { get; set; }
        public Interval Interval { get; set; }
    }
}
