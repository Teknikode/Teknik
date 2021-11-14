using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            var billingEvent = await billingService.ParseEvent(Request);

            if (billingEvent == null)
                return BadRequest();

            var session = billingService.ProcessCheckoutCompletedEvent(billingEvent);
            if (session.PaymentStatus == PaymentStatus.Paid)
            {
                var subscription = billingService.GetSubscription(session.SubscriptionId);

                ProcessSubscription(session.CustomerId, subscription);
            }

            return Ok();
        }

        public async Task<IActionResult> HandleSubscriptionChange()
        {
            var billingService = BillingFactory.GetBillingService(_config.BillingConfig);

            var billingEvent = await billingService.ParseEvent(Request);

            if (billingEvent == null)
                return BadRequest();

            var customerEvent = billingService.ProcessCustomerEvent(billingEvent);
            
            foreach (var subscription in customerEvent.Subscriptions)
            {
                ProcessSubscription(customerEvent.CustomerId, subscription);
            }

            return Ok();
        }

        private void ProcessSubscription(string customerId, Subscription subscription)
        {
            // They have paid, so let's get their subscription and update their user settings
            var user = _dbContext.Users.FirstOrDefault(u => u.BillingCustomer != null &&
                                                            u.BillingCustomer.CustomerId == customerId);
            if (user != null)
            {
                var isActive = subscription.Status == SubscriptionStatus.Active;
                foreach (var price in subscription.Prices)
                {
                    ProcessPrice(user, price, isActive);
                }
            }
        }

        private void ProcessPrice(User user, Price price, bool active)
        {
            // What type of subscription is this
            if (_config.BillingConfig.UploadProductId == price.ProductId)
            {
                // Process Upload Settings
                user.UploadSettings.MaxUploadStorage = active ? price.Storage : _config.UploadConfig.MaxStorage;
                user.UploadSettings.MaxUploadFileSize = active ? price.FileSize : _config.UploadConfig.MaxUploadFileSize;
                _dbContext.Entry(user).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }
            else if (_config.BillingConfig.EmailProductId == price.ProductId)
            {
                // Process an email subscription
                string email = UserHelper.GetUserEmailAddress(_config, user.Username);
                if (active)
                {
                    UserHelper.EnableUserEmail(_config, email);
                    UserHelper.EditUserEmailMaxSize(_config, email, (int)price.Storage);
                }
                else
                {
                    UserHelper.DisableUserEmail(_config, email);
                }
            }
        }
    }
}
