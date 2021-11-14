using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class CheckoutResult
    {
        public string CustomerId { get; set; }

        public string SubscriptionId { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
    }
}
