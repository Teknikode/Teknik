using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.BillingCore.Models;
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

        public abstract object GetCustomer(string id);
        public abstract bool CreateCustomer(string email);

        public abstract List<Product> GetProductList();
        public abstract Product GetProduct(string productId);

        public abstract List<Price> GetPriceList(string productId);
        public abstract Price GetPrice(string priceId);

        public abstract List<Subscription> GetSubscriptionList(string customerId);
        public abstract Subscription GetSubscription(string subscriptionId);
        public abstract Tuple<bool, string, string> CreateSubscription(string customerId, string priceId);
        public abstract bool EditSubscription(Subscription subscription);
        public abstract bool RemoveSubscription(string subscriptionId);

        public abstract void SyncSubscriptions();
    }
}
