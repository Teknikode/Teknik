using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.BillingCore.Models;
using Teknik.Configuration;

namespace Teknik.BillingCore
{
    public class StripeService : BillingService
    {
        public StripeService(BillingConfig config) : base(config)
        {
            StripeConfiguration.ApiKey = config.StripeSecretApiKey;
        }

        public override object GetCustomer(string id)
        {
            var service = new CustomerService();
            return service.Get(id);
        }

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

        public override List<Models.Product> GetProductList()
        {
            var productList = new List<Models.Product>();
            var productService = new ProductService();

            var options = new ProductListOptions
            {
                Active = true
            };
            var products = productService.List(options);
            foreach (var product in products)
            {
                productList.Add(ConvertProduct(product));
            }
            return productList;
        }

        public override Models.Product GetProduct(string productId)
        {
            var productService = new ProductService();
            Stripe.Product product = productService.Get(productId);
            if (product != null)
                return ConvertProduct(product);

            return null;
        }

        public override List<Models.Price> GetPriceList(string productId)
        {
            var foundPrices = new List<Models.Price>();
            var options = new PriceListOptions
            {
                Active = true,
                Product = productId
            };

            var priceService = new PriceService();
            var priceList = priceService.List(options);
            if (priceList != null)
            {
                foreach (var price in priceList)
                {
                    foundPrices.Add(ConvertPrice(price));
                }
            }
            return foundPrices;
        }

        public override Models.Price GetPrice(string priceId)
        {

            var priceService = new PriceService();
            var price = priceService.Get(priceId);
            if (price != null)
                return ConvertPrice(price);

            return null;
        }

        public override List<Models.Subscription> GetSubscriptionList(string customerId)
        {
            var subscriptionList = new List<Models.Subscription>();

            if (!string.IsNullOrEmpty(customerId))
            {
                var options = new SubscriptionListOptions
                {
                    Customer = customerId                     
                };
                var subService = new SubscriptionService();
                var subs = subService.List(options);
                if (subs != null)
                {
                    foreach (var sub in subs)
                    {
                        subscriptionList.Add(ConvertSubscription(sub));
                    }
                }
            }

            return subscriptionList;
        }

        public override Models.Subscription GetSubscription(string subscriptionId)
        {
            var subService = new SubscriptionService();
            var sub = subService.Get(subscriptionId);
            if (sub != null)
                return ConvertSubscription(sub);

            return null;
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

        public override bool EditSubscription(Models.Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveSubscription(string subscriptionId)
        {
            throw new NotImplementedException();
        }

        public override void SyncSubscriptions()
        {
            throw new NotImplementedException();
        }

        private Models.Product ConvertProduct(Stripe.Product product)
        {
            return new Models.Product()
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Prices = GetPriceList(product.Id)
            };
        }

        private Models.Price ConvertPrice(Stripe.Price price)
        {
            var interval = Interval.Once;
            if (price.Type == "recurring")
            {
                switch (price.Recurring.Interval)
                {
                    case "day":
                        interval = Interval.Day;
                        break;
                    case "week":
                        interval = Interval.Week;
                        break;
                    case "month":
                        interval = Interval.Month;
                        break;
                    case "year":
                        interval = Interval.Year;
                        break;
                }
            }
            var convPrice = new Models.Price()
            {
                Id = price.Id,
                ProductId = price.ProductId,
                Name = price.Nickname,
                Interval = interval,
                Currency = price.Currency
            };
            if (price.UnitAmountDecimal != null)
                convPrice.Amount = price.UnitAmountDecimal / 100;
            if (price.Metadata.ContainsKey("storage"))
                convPrice.Storage = long.Parse(price.Metadata["storage"]);
            return convPrice;
        }

        private Models.Subscription ConvertSubscription(Stripe.Subscription subscription)
        {
            var status = SubscriptionStatus.Incomplete;
            switch (subscription.Status)
            {
                case "active":
                    status = SubscriptionStatus.Active;
                    break;
                case "past_due":
                    status = SubscriptionStatus.PastDue;
                    break;
                case "unpaid":
                    status = SubscriptionStatus.Unpaid;
                    break;
                case "canceled":
                    status = SubscriptionStatus.Canceled;
                    break;
                case "incomplete":
                    status = SubscriptionStatus.Incomplete;
                    break;
                case "incomplete_expired":
                    status = SubscriptionStatus.IncompleteExpired;
                    break;
                case "trialing":
                    status = SubscriptionStatus.Trialing;
                    break;
            }
            var prices = new List<Models.Price>();
            if (subscription.Items != null)
            {
                foreach (var item in subscription.Items)
                {
                    prices.Add(ConvertPrice(item.Price));
                }
            }
            return new Models.Subscription()
            {
                Id = subscription.Id,
                CustomerId = subscription.CustomerId,
                Status = status,
                Prices = prices
            };
        }
    }
}
