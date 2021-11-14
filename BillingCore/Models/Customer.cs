using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class Customer
    {
        public string CustomerId { get; set; }

        public List<Subscription> Subscriptions { get; set; }

        public Customer()
        {
            Subscriptions = new List<Subscription>();
        }
    }
}
