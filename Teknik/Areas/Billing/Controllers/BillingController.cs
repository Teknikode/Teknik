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
            return View(new BillingViewModel() { StripePublishKey = _config.BillingConfig.StripePublishApiKey });
        }

        [AllowAnonymous]
        public IActionResult ViewPaymentInfo()
        {
            return View(new PaymentViewModel() { StripePublishKey = _config.BillingConfig.StripePublishApiKey });
        }
    }
}
