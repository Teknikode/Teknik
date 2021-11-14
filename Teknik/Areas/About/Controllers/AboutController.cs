using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.About.ViewModels;
using Teknik.Areas.Billing.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.BillingCore;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.About.Controllers
{
    [Authorize]
    [Area("About")]
    public class AboutController : DefaultController
    {
        public AboutController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index([FromServices] Config config)
        {
            ViewBag.Title = "About";
            ViewBag.Description = "What is Teknik?";
            var vm = new AboutViewModel();

            // Get Biling Service
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var subVM = new SubscriptionsViewModel();

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

            vm.Subscriptions = subVM;

            return View(vm);
        }
    }
}