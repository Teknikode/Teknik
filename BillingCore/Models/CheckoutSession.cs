using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class CheckoutSession
    {
        public string PaymentIntentId { get; set; }
        public string CustomerId { get; set; }
        public string SubscriptionId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string Url { get; set; }
    }
}
