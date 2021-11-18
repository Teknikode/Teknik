﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Billing;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.BillingCore;
using Teknik.BillingCore.Models;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;

namespace Teknik.Areas.API.V1.Controllers
{
    public class BillingAPIv1Controller : APIv1Controller
    {
        public BillingAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        public async Task<IActionResult> HandleCheckoutCompleteEvent()
        {
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var billingEvent = await billingService.ParseEvent(Request, _config.BillingConfig.StripeCheckoutWebhookSecret);

            if (billingEvent == null)
                return BadRequest();

            var session = billingService.ProcessCheckoutCompletedEvent(billingEvent);
            if (session.PaymentStatus == PaymentStatus.Paid)
            {
                var subscription = billingService.GetSubscription(session.SubscriptionId);

                BillingHelper.ProcessSubscription(_config, _dbContext, session.CustomerId, subscription);
            }

            return Ok();
        }

        public async Task<IActionResult> HandleSubscriptionChange()
        {
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var billingEvent = await billingService.ParseEvent(Request, _config.BillingConfig.StripeCustomerWebhookSecret);

            if (billingEvent == null)
                return BadRequest();

            var subscriptionEvent = billingService.ProcessSubscriptionEvent(billingEvent);
            if (subscriptionEvent == null)
                return BadRequest();

            BillingHelper.ProcessSubscription(_config, _dbContext, subscriptionEvent.CustomerId, subscriptionEvent);

            return Ok();
        }
    }
}