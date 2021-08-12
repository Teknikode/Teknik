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
                SubscriptionId = "upload_free",
                SubscriptionName = "Basic Account",
                SubscribeText = "Sign up for free",
                SubscribeUrl = Url.SubRouteUrl("account", "User.Register"),
                BaseStorage = 5368709120,
                MaxStorage = 107374200000
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "upload_10gb",
                SubscriptionName = "Standalone 10 GB",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_10gb" }),
                BaseStorage = 10737418240,
                BasePrice = 0.99,
                BaseUnit = "month",
                OverageAllowed = true,
                OveragePrice = 0.30,
                OverageUnit = "GB",
                MaxStorage = 107374200000
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "upload_50gb",
                SubscriptionName = "Standalone 50 GB",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_50gb" }),
                BaseStorage = 53687091200,
                BasePrice = 3.99,
                BaseUnit = "month",
                OverageAllowed = true,
                OveragePrice = 0.30,
                OverageUnit = "GB",
                MaxStorage = 107374200000
            });
            subVM.UploadSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "upload_usage",
                SubscriptionName = "Pay Per Unit",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "upload_usage" }),
                BaseStorage = null,
                BasePrice = 0.15,
                BaseUnit = "GB",
                OverageAllowed = true,
                OverageUnit = "monthly",
                MaxStorage = 107374200000
            });

            // Get Email Subscriptions
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "email_1gb_monthly",
                SubscriptionName = "Basic Email - Monthly",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_1gb_monthly" }),
                BaseStorage = 1073741824,
                BasePrice = 1.99,
                BaseUnit = "month"
            });
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "email_1gb_yearly",
                SubscriptionName = "Basic Email - Yearly",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_1gb_yearly" }),
                BaseStorage = 1073741824,
                BasePrice = 19.99,
                BaseUnit = "year"
            });
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "email_5gb_monthly",
                SubscriptionName = "Premium Email - Monthly",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_5gb_monthly" }),
                BaseStorage = 5368709120,
                BasePrice = 3.99,
                BaseUnit = "month"
            });
            subVM.EmailSubscriptions.Add(new SubscriptionViewModel()
            {
                Primary = true,
                SubscriptionId = "email_5gb_yearly",
                SubscriptionName = "Premium Email - Yearly",
                SubscribeText = "Subscribe",
                SubscribeUrl = Url.SubRouteUrl("billing", "Billing.Subscribe", new { subscription = "email_5gb_yearly" }),
                BaseStorage = 5368709120,
                BasePrice = 39.99,
                BaseUnit = "year"
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
