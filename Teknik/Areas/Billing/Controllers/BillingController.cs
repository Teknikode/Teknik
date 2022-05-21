using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Teknik.Areas.Billing.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.BillingCore;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.Billing.Controllers
{
    [Authorize]
    [Area("Billing")]
    public class BillingController : DefaultController
    {
        public BillingController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        [TrackPageView]
        public IActionResult ViewSubscriptions()
        {
            ViewBag.Title = "Subscriptions";

            var subVM = new SubscriptionsViewModel();

            // Get Biling Service
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            // Get current subscriptions
            var curPrices = new Dictionary<string, List<string>>();

            if (User.Identity.IsAuthenticated)
            {
                var user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user.BillingCustomer != null)
                {
                    var currentSubs = billingService.GetSubscriptionList(user.BillingCustomer.CustomerId);
                    foreach (var curSub in currentSubs)
                    {
                        foreach (var price in curSub.Prices)
                        {
                            if (!curPrices.ContainsKey(price.ProductId))
                                curPrices[price.ProductId] = new List<string>();
                            curPrices[price.ProductId].Add(price.Id);
                        }
                    }
                }
            }
            bool hasUploadProduct = curPrices.ContainsKey(_config.BillingConfig.UploadProductId);
            bool hasEmailProduct = curPrices.ContainsKey(_config.BillingConfig.EmailProductId);
            var curUploadPrice = string.Empty;
            if (curPrices.ContainsKey(_config.BillingConfig.UploadProductId))
                curUploadPrice = curPrices[_config.BillingConfig.UploadProductId].FirstOrDefault();
            var curEmailPrice = string.Empty;
            if (curPrices.ContainsKey(_config.BillingConfig.EmailProductId))
                curEmailPrice = curPrices[_config.BillingConfig.EmailProductId].FirstOrDefault();

            // Show Free Subscription
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                CurrentSubMonthly = !hasUploadProduct && User.Identity.IsAuthenticated,
                SubscribeText = "Free",
                SubscribeUrlMonthly = Url.SubRouteUrl("about", "About.Index"),
                BaseStorage = _config.UploadConfig.MaxStorage
            });

            // Get Upload Prices
            var uploadProduct = billingService.GetProduct(_config.BillingConfig.UploadProductId);
            if (uploadProduct != null)
            {
                bool handledFirst = false;
                foreach (var priceGrp in uploadProduct.Prices.GroupBy(p => p.Storage).OrderBy(p => p.Key))
                {
                    // Get Monthly prices
                    var priceMonth = priceGrp.FirstOrDefault(p => p.Interval == BillingCore.Models.Interval.Month);

                    // Get Yearly prices
                    var priceYear = priceGrp.FirstOrDefault(p => p.Interval == BillingCore.Models.Interval.Year);

                    var curPrice = priceGrp.FirstOrDefault(p => p.Id == curUploadPrice);
                    subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
                    {
                        Recommended = !handledFirst,
                        CurrentSubMonthly = curPrice?.Id == priceMonth?.Id,
                        CurrentSubYearly = curPrice?.Id == priceYear?.Id,
                        SubscribeUrlMonthly = Url.SubRouteUrl("billing", 
                                                              hasUploadProduct ? "Billing.EditSubscription" : "Billing.Checkout", 
                                                              new { priceId = priceMonth?.Id }),
                        SubscribeUrlYearly = Url.SubRouteUrl("billing", 
                                                             hasUploadProduct ? "Billing.EditSubscription" : "Billing.Checkout", 
                                                             new { priceId = priceYear?.Id }),
                        BaseStorage = priceMonth?.Storage,
                        BasePriceMonthly = priceMonth?.Amount,
                        BasePriceYearly = priceYear?.Amount
                    });
                    handledFirst = true;
                }
            }

            // Get Email Prices
            var emailProduct = billingService.GetProduct(_config.BillingConfig.EmailProductId);
            if (emailProduct != null)
            {
                bool handledFirst = false;
                foreach (var priceGrp in emailProduct.Prices.GroupBy(p => p.Storage).OrderBy(p => p.Key))
                {
                    // Get Monthly prices
                    var priceMonth = priceGrp.FirstOrDefault(p => p.Interval == BillingCore.Models.Interval.Month);

                    // Get Yearly prices
                    var priceYear = priceGrp.FirstOrDefault(p => p.Interval == BillingCore.Models.Interval.Year);

                    var curPrice = priceGrp.FirstOrDefault(p => p.Id == curEmailPrice);
                    var emailSub = new SubscriptionViewModel()
                    {
                        Recommended = !handledFirst,
                        CurrentSubMonthly = curPrice?.Id == priceMonth?.Id,
                        CurrentSubYearly = curPrice?.Id == priceYear?.Id,
                        SubscribeUrlMonthly = Url.SubRouteUrl("billing",
                                                              hasEmailProduct ? "Billing.EditSubscription" : "Billing.Checkout",
                                                              new { priceId = priceMonth?.Id }),
                        SubscribeUrlYearly = Url.SubRouteUrl("billing",
                                                             hasEmailProduct ? "Billing.EditSubscription" : "Billing.Checkout",
                                                             new { priceId = priceYear?.Id }),
                        BaseStorage = priceMonth?.Storage,
                        BasePriceMonthly = priceMonth?.Amount,
                        BasePriceYearly = priceYear?.Amount
                    };
                    if (!handledFirst)
                        emailSub.PanelOffset = "3";
                    subVM.EmailSubscriptions.Add(emailSub);
                    handledFirst = true;
                }
            }

            return View(subVM);
        }

        [AllowAnonymous]
        public IActionResult ViewPaymentInfo()
        {
            return View(new PaymentViewModel() { StripePublishKey = _config.BillingConfig.StripePublishApiKey });
        }

        [AllowAnonymous]
        public IActionResult Subscribe(string priceId, string subscriptionId)
        {
            // Get Subscription Info
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var vm = new SubscribeViewModel();
            vm.Price = billingService.GetPrice(priceId);
            vm.Subscription = billingService.GetSubscription(subscriptionId);

            return View(vm);
        }

        public IActionResult Checkout(string priceId)
        {
            if (!_config.BillingConfig.Enabled)
                throw new UnauthorizedAccessException();

            // Get Subscription Info
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var price = billingService.GetPrice(priceId);
            if (price == null)
                throw new ArgumentException("Invalid Price ID", "priceId");

            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user == null)
                throw new UnauthorizedAccessException();

            var session = billingService.CreateCheckoutSession(user.BillingCustomer?.CustomerId,
                                                               priceId, 
                                                               Url.SubRouteUrl("billing", "Billing.CheckoutComplete", new { productId = price.ProductId }), 
                                                               Url.SubRouteUrl("billing", "Billing.Subscriptions"));
            return Redirect(session.Url);
        }

        public IActionResult CheckoutComplete(string productId, string session_id)
        {
            if (!_config.BillingConfig.Enabled)
                throw new UnauthorizedAccessException();

            // Get Checkout Info
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);
            var checkoutSession = billingService.GetCheckoutSession(session_id);
            if (checkoutSession != null)
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user == null)
                    throw new UnauthorizedAccessException();

                if (user.BillingCustomer == null)
                {
                    BillingHelper.CreateCustomer(_dbContext, user, checkoutSession.CustomerId);
                }

                var subscription = billingService.GetSubscription(checkoutSession.SubscriptionId);
                if (subscription != null)
                {
                    foreach (var price in subscription.Prices)
                    {
                        if (price.ProductId == productId)
                        {
                            return Redirect(Url.SubRouteUrl("billing", "Billing.SubscriptionSuccess", new { priceId = price.Id }));
                        }
                    }
                }
            }

            return Redirect(Url.SubRouteUrl("billing", "Billing.ViewSubscriptions"));
        }

        public IActionResult EditSubscription(string priceId)
        {
            if (!_config.BillingConfig.Enabled)
                throw new UnauthorizedAccessException();

            // Get Subscription Info
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var price = billingService.GetPrice(priceId);
            if (price == null)
                throw new ArgumentException("Invalid Price ID", "priceId");

            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user == null)
                throw new UnauthorizedAccessException();
            
            if (user.BillingCustomer == null)
            {
                return Checkout(priceId);
            }
            else
            {
                var currentSubs = billingService.GetSubscriptionList(user.BillingCustomer.CustomerId);
                foreach (var curSub in currentSubs)
                {
                    if (curSub.Prices.Exists(p => p.ProductId == price.ProductId))
                    {
                        billingService.EditSubscriptionPrice(curSub.Id, price.Id);
                        return Redirect(Url.SubRouteUrl("billing", "Billing.SubscriptionSuccess", new { priceId = price.Id }));
                    }
                }
            }

            return Redirect(Url.SubRouteUrl("billing", "Billing.ViewSubscriptions"));
        }

        public IActionResult SubscriptionSuccess(string priceId)
        {
            var vm = new SubscriptionSuccessViewModel();

            // Get Subscription Info
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var price = billingService.GetPrice(priceId);
            if (price == null)
                throw new ArgumentException("Invalid Price ID", "priceId");

            var product = billingService.GetProduct(price.ProductId);
            vm.ProductName = product.Name;
            vm.Price = price.Amount;
            vm.Interval = price.Interval.ToString();
            vm.Storage = price.Storage;

            return View(vm);
        }
    }
}
