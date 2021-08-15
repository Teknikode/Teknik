using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Billing.ViewModels;
using Teknik.BillingCore;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.Billing.Controllers
{
    [Area("Billing")]
    public class BillingController : DefaultController
    {
        public BillingController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(new BillingViewModel() { StripePublishKey = _config.BillingConfig.StripePublishApiKey });
        }

        [AllowAnonymous]
        public IActionResult ViewSubscriptions()
        {
            var subVM = new SubscriptionsViewModel();

            // Get Biling Service
            var billingService = BillingFactory.GetStorageService(_config.BillingConfig);

            // Get current subscriptions
            string curSubId = null;
            var curSubs = new Dictionary<string, List<string>>();

            if (User.Identity.IsAuthenticated)
            {
                var currentSubs = billingService.GetSubscriptionList(User.Identity.Name);
                foreach (var curSub in currentSubs)
                {
                    foreach (var price in curSub.Prices)
                    {
                        if (!curSubs.ContainsKey(price.ProductId))
                            curSubs[price.ProductId] = new List<string>();
                        curSubs[price.ProductId].Add(price.Id);
                    }
                }
            }

            // Show Free Subscription
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                CurrentPlan = curSubId == null,
                SubscribeText = "Free",
                SubscribeUrlMonthly = Url.SubRouteUrl("account", "User.Register"),
                BaseStorage = _config.UploadConfig.MaxUploadSizeBasic
            });

            // Get Upload Prices
            var curUploadSubs = new List<string>();
            if (curSubs.ContainsKey(_config.BillingConfig.UploadProductId))
                curUploadSubs = curSubs[_config.BillingConfig.UploadProductId];
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

                    var isCurrent = curUploadSubs.Exists(s => priceGrp.FirstOrDefault(p => p.ProductId == s) != null);
                    subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
                    {
                        Recommended = !handledFirst,
                        CurrentPlan = isCurrent,
                        SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { priceId = priceMonth?.Id }),
                        SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { priceId = priceYear?.Id }),
                        BaseStorage = priceMonth?.Storage,
                        BasePriceMonthly = priceMonth?.Amount,
                        BasePriceYearly = priceYear?.Amount
                    });
                    handledFirst = true;
                }
            }

            // Get Email Prices
            var curEmailSubs = new List<string>();
            if (curSubs.ContainsKey(_config.BillingConfig.EmailProductId))
                curEmailSubs = curSubs[_config.BillingConfig.EmailProductId];
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

                    var isCurrent = curUploadSubs.Exists(s => priceGrp.FirstOrDefault(p => p.ProductId == s) != null);
                    var emailSub = new SubscriptionViewModel()
                    {
                        Recommended = !handledFirst,
                        CurrentPlan = isCurrent,
                        SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { priceId = priceMonth?.Id }),
                        SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { priceId = priceYear?.Id }),
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
        public IActionResult Subscribe(string priceId)
        {
            // Get Subscription Info
            var billingService = BillingFactory.GetStorageService(_config.BillingConfig);
            var price = billingService.GetPrice(priceId);

            return View(new SubscriptionViewModel());
        }
    }
}
