using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.BillingCore
{
    public class StripeService : BillingService
    {
        public StripeService(BillingConfig config) : base(config)
        { }

        public override bool CreateCustomer(string email)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
            };
            var service = new CustomerService();
            var customer = service.Create(options);
            return customer != null;
        }

        public override Subscription GetSubscription(string subscriptionId)
        {
            throw new NotImplementedException();
        }

        public override Tuple<bool, string, string> CreateSubscription(string customerId, string priceId)
        {
            // Create the subscription. Note we're expanding the Subscription's
            // latest invoice and that invoice's payment_intent
            // so we can pass it to the front end to confirm the payment
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
                PaymentBehavior = "default_incomplete",
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Stripe.Subscription subscription = subscriptionService.Create(subscriptionOptions);

                return new Tuple<bool, string, string>(true, subscription.Id, subscription.LatestInvoice.PaymentIntent.ClientSecret);
            }
            catch (StripeException e)
            {
                return new Tuple<bool, string, string>(false, $"Failed to create subscription. {e}", null);
            }
        }

        public override bool EditSubscription()
        {
            throw new NotImplementedException();
        }

        public override bool RemoveSubscription()
        {
            throw new NotImplementedException();
        }

        public override void SyncSubscriptions()
        {
            throw new NotImplementedException();
        }

        private Customer GetCustomer(string id)
        {
            var service = new CustomerService();
            return service.Get(id);
        }
    }
}
