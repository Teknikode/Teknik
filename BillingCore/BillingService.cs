using Microsoft.AspNetCore.Http;
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

        public abstract List<Customer> GetCustomers();
        public abstract Customer GetCustomer(string id);
        public abstract string GetCustomerProfileUrl(string id);
        public abstract string CreateCustomer(string username, string email);

        public abstract List<Product> GetProductList();
        public abstract Product GetProduct(string productId);

        public abstract List<Price> GetPriceList(string productId);
        public abstract Price GetPrice(string priceId);

        public abstract List<Subscription> GetSubscriptionList(string customerId);
        public abstract Subscription GetSubscription(string subscriptionId);
        public abstract Subscription CreateSubscription(string customerId, string priceId);
        public abstract Subscription EditSubscriptionPrice(string subscriptionId, string priceId);
        public abstract Subscription RenewSubscription(string subscriptionId);
        public abstract bool CancelSubscription(string subscriptionId, bool atEndOfPeriod);

        public abstract CheckoutSession CreateCheckoutSession(string customerId, string priceId, string successUrl, string cancelUrl);
        public abstract CheckoutSession GetCheckoutSession(string sessionId);

        public abstract PortalSession CreatePortalSession(string customerId, string returnUrl);

        public abstract Task<Event> ParseEvent(HttpRequest request, string apiKey);
        public abstract CheckoutSession ProcessCheckoutCompletedEvent(Event e);
        public abstract Subscription ProcessSubscriptionEvent(Event e);
        public abstract Customer ProcessCustomerEvent(Event e);
    }
}
