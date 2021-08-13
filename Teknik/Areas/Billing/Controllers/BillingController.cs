using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Billing.ViewModels;
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
        public IActionResult Subscriptions()
        {
            var subVM = new SubscriptionsViewModel();

            // Get Upload Subscriptions
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                CurrentPlan = true,
                SubscriptionId = "upload_free",
                SubscriptionName = "Basic Account",
                SubscribeText = "Free",
                SubscribeUrlMonthly = Url.SubRouteUrl("account", "User.Register"),
                BaseStorage = 5368709120
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                Recommended = true,
                SubscriptionId = "upload_10gb",
                SubscriptionName = "Standalone 10 GB",
                SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_10gb_monthly" }),
                SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_10gb_yearly" }),
                BaseStorage = 10737418240,
                BasePriceMonthly = 0.99,
                BasePriceYearly = 9.99
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                SubscriptionId = "upload_50gb",
                SubscriptionName = "Standalone 50 GB",
                SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_50gb_monthly" }),
                SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_50gb_yearly" }),
                BaseStorage = 53687091200,
                BasePriceMonthly = 3.99,
                BasePriceYearly = 39.99
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                SubscriptionId = "upload_100gb",
                SubscriptionName = "Standalone 100 GB",
                SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_100gb_monthly" }),
                SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_100gb_yearly" }),
                BaseStorage = 107374200000,
                BasePriceMonthly = 5.99,
                BasePriceYearly = 59.99
            });

            // Get Email Subscriptions
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                Recommended = true,
                SubscriptionId = "email_1gb",
                SubscriptionName = "Basic Email",
                SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_1gb_monthly" }),
                SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_1gb_yearly" }),
                BaseStorage = 1073741824,
                BasePriceMonthly = 1.99,
                BasePriceYearly = 19.99,
                PanelOffset = "3"
            });
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                SubscriptionId = "email_5gb",
                SubscriptionName = "Premium Email",
                SubscribeUrlMonthly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_5gb_monthly" }),
                SubscribeUrlYearly = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_5gb_yearly" }),
                BaseStorage = 5368709120,
                BasePriceMonthly = 3.99,
                BasePriceYearly = 39.99,
            });

            return View(subVM);
        }

        [AllowAnonymous]
        public IActionResult ViewPaymentInfo()
        {
            return View(new PaymentViewModel() { StripePublishKey = _config.BillingConfig.StripePublishApiKey });
        }
    }
}
