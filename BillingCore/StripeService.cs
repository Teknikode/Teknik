using Microsoft.AspNetCore.Http;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

        public override List<Models.Customer> GetCustomers()
        {
            var customers = new List<Models.Customer>();
            var service = new CustomerService();
            var foundCustomers = service.List();
            if (foundCustomers != null)
            {
                foreach (var customer in foundCustomers)
                {
                    customers.Add(ConvertCustomer(customer));
                }
            }
            return customers;
        }

        public override Models.Customer GetCustomer(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var service = new CustomerService();
                var foundCustomer = service.Get(email);
                if (foundCustomer != null)
                    return ConvertCustomer(foundCustomer);
            }

            return null;
        }

        public override string CreateCustomer(string username, string email)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            var options = new CustomerCreateOptions
            {
                Name = username,
                Email = email,
                Description = $"Customer for account {username}"
            };
            var service = new CustomerService();
            var customer = service.Create(options);
            return customer.Id;
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
            if (!string.IsNullOrEmpty(productId))
            {
                var productService = new ProductService();
                Stripe.Product product = productService.Get(productId);
                if (product != null)
                    return ConvertProduct(product);
            }

            return null;
        }

        public override List<Models.Price> GetPriceList(string productId)
        {
            var foundPrices = new List<Models.Price>();
            if (!string.IsNullOrEmpty(productId))
            {
                var options = new PriceListOptions
                {
                    Active = true,
                    Product = productId
                };
                options.AddExpand("data.product");

                var priceService = new PriceService();
                var priceList = priceService.List(options);
                if (priceList != null)
                {
                    foreach (var price in priceList)
                    {
                        foundPrices.Add(ConvertPrice(price));
                    }
                }
            }

            return foundPrices;
        }

        public override Models.Price GetPrice(string priceId)
        {
            if (!string.IsNullOrEmpty(priceId))
            {
                var options = new PriceGetOptions();
                var priceService = new PriceService();
                var price = priceService.Get(priceId, options);
                if (price != null)
                    return ConvertPrice(price);
            }

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
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var subService = new SubscriptionService();
                var sub = subService.Get(subscriptionId);
                if (sub != null)
                    return ConvertSubscription(sub);
            }

            return null;
        }

        public override Models.Subscription CreateSubscription(string customerId, string priceId)
        {
            if (!string.IsNullOrEmpty(customerId) &&
                !string.IsNullOrEmpty(priceId))
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
                    CancelAtPeriodEnd = false
                };
                subscriptionOptions.AddExpand("latest_invoice.payment_intent");
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Create(subscriptionOptions);

                return ConvertSubscription(subscription);
            }
            return null;
        }

        public override Models.Subscription EditSubscriptionPrice(string subscriptionId, string priceId)
        {
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Get(subscriptionId);
                if (subscription != null)
                {
                    var subscriptionOptions = new SubscriptionUpdateOptions()
                    {
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Id = subscription.Items.Data[0].Id,
                                Price = priceId,
                            },
                        },
                        CancelAtPeriodEnd = false,
                        ProrationBehavior = "create_prorations"
                    };
                    subscriptionOptions.AddExpand("latest_invoice.payment_intent");
                    var result = subscriptionService.Update(subscriptionId, subscriptionOptions);
                    if (result != null)
                        return ConvertSubscription(result);
                }
            }
            return null;
        }

        public override bool CancelSubscription(string subscriptionId)
        {
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var cancelOptions = new SubscriptionCancelOptions()
                {
                    InvoiceNow = true
                };
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Cancel(subscriptionId, cancelOptions);
                return subscription.Status == "canceled";
            }
            return false;
        }

        public override CheckoutSession CreateCheckoutSession(string customerId, string priceId, string successUrl, string cancelUrl)
        {
            // Modify Success URL to include session ID variable
            var uriBuilder = new UriBuilder(successUrl);
            var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);
            paramValues.Add("session_id", "{CHECKOUT_SESSION_ID}");
            uriBuilder.Query = paramValues.ToString();
            successUrl = uriBuilder.Uri.ToString();

            var checkoutService = new Stripe.Checkout.SessionService();
            var sessionOptions = new Stripe.Checkout.SessionCreateOptions()
            {
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>()
                {
                    new Stripe.Checkout.SessionLineItemOptions()
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                PaymentMethodTypes = new List<string>()
                {
                    "card"
                },                
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Customer = customerId
            };
            sessionOptions.AddExpand("customer");
            var session = checkoutService.Create(sessionOptions);
            return ConvertCheckoutSession(session);
        }

        public override CheckoutSession GetCheckoutSession(string sessionId)
        {
            var checkoutService = new Stripe.Checkout.SessionService();
            var sessionOptions = new Stripe.Checkout.SessionGetOptions();
            sessionOptions.AddExpand("customer");
            var session = checkoutService.Get(sessionId, sessionOptions);

            return ConvertCheckoutSession(session);
        }

        public override async Task<Models.Event> ParseEvent(HttpRequest request, string apiKey)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  request.Headers["Stripe-Signature"],
                  apiKey
                );

                return ConvertEvent(stripeEvent);
            }
            catch (StripeException)
            {
            }
            return null;
        }

        public override CheckoutSession ProcessCheckoutCompletedEvent(Models.Event ev)
        {
            // Handle the checkout.session.completed event
            var session = ev.Data as Stripe.Checkout.Session;

            return ConvertCheckoutSession(session);
        }

        public override Models.Subscription ProcessSubscriptionEvent(Models.Event ev)
        {
            // Handle the checkout.session.completed event
            var subscription = ev.Data as Stripe.Subscription;

            return ConvertSubscription(subscription);
        }

        public override Models.Customer ProcessCustomerEvent(Models.Event ev)
        {
            // Handle the checkout.session.completed event
            var customer = ev.Data as Stripe.Customer;

            return ConvertCustomer(customer);
        }

        public override PortalSession CreatePortalSession(string customerId, string returnUrl)
        {
            var portalService = new Stripe.BillingPortal.SessionService();
            var sessionOptions = new Stripe.BillingPortal.SessionCreateOptions()
            {
                Customer = customerId,
                ReturnUrl = returnUrl
            };
            var session = portalService.Create(sessionOptions);
            return ConvertPortalSession(session);
        }

        private Models.Product ConvertProduct(Stripe.Product product)
        {
            if (product == null)
                return null;
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
            if (price == null)
                return null;
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
            if (price.Metadata.ContainsKey("fileSize"))
                convPrice.FileSize = long.Parse(price.Metadata["fileSize"]);
            return convPrice;
        }

        private Models.Subscription ConvertSubscription(Stripe.Subscription subscription)
        {
            if (subscription == null)
                return null;
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
                Prices = prices,
                ClientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret
            };
        }

        private CheckoutSession ConvertCheckoutSession(Stripe.Checkout.Session session)
        {
            if (session == null)
                return null;

            var paymentStatus = PaymentStatus.Unpaid;
            switch (session.PaymentStatus)
            {
                case "paid":
                    paymentStatus = PaymentStatus.Paid;
                    break;
                case "unpaid":
                    paymentStatus = PaymentStatus.Unpaid;
                    break;
                case "no_payment_required":
                    paymentStatus = PaymentStatus.NoPaymentRequired;
                    break;
            }

            return new CheckoutSession()
            {
                PaymentIntentId = session.PaymentIntentId,
                CustomerId = session.Customer?.Id ?? session.CustomerId,
                SubscriptionId = session.SubscriptionId,
                PaymentStatus = paymentStatus,
                Url = session.Url
            };
        }

        private PortalSession ConvertPortalSession(Stripe.BillingPortal.Session session)
        {
            if (session == null)
                return null;

            return new PortalSession()
            {
                Url = session.Url
            };
        }

        private Models.Customer ConvertCustomer(Stripe.Customer customer)
        {
            var returnCust = new Models.Customer()
            {
                CustomerId = customer.Id
            };

            if (customer.Subscriptions != null && 
                customer.Subscriptions.Any())
                returnCust.Subscriptions = customer.Subscriptions.Select(s => ConvertSubscription(s)).ToList();

            return returnCust;
        }

        private Models.Event ConvertEvent(Stripe.Event ev)
        {
            if (ev == null)
                return null;

            var eventType = EventType.Unknown;
            switch (ev.Type)
            {
                case Events.CheckoutSessionCompleted:
                    eventType = EventType.CheckoutComplete;
                    break;
                case Events.CustomerSubscriptionDeleted:
                    eventType = EventType.SubscriptionDeleted;
                    break;
                case Events.CustomerSubscriptionUpdated:
                    eventType = EventType.SubscriptionUpdated;
                    break;
            }

            return new Models.Event()
            {
                EventType = eventType,
                Data = ev.Data.Object
            };
        }
    }
}
