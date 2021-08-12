using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingCore
{
    public class UserSubscription
    {
        public int UserId { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
