using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.BillingCore
{
    public abstract class BillingService
    {
        protected readonly BillingConfig Config;

        public BillingService(BillingConfig billingConfig)
        {
            Config = billingConfig;
        }

        public abstract bool CreateCustomer(string email);
        public abstract Tuple<bool, string, string> CreateSubscription(string customerId, string priceId);

        public abstract bool EditSubscription();

        public abstract Subscription GetSubscription(string subscriptionId);

        public abstract bool RemoveSubscription();

        public abstract void SyncSubscriptions();
    }
}
