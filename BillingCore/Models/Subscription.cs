using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore.Models
{
    public class Subscription
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime BillingPeriodEnd { get; set; }
        public bool CancelAtBillingEnd { get; set; }
        public List<Price> Prices { get; set; }
        public string ClientSecret { get; set; }
    }
}
