using Microsoft.EntityFrameworkCore;
using System.Linq;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.BillingCore;
using Teknik.BillingCore.Models;
using Teknik.Configuration;
using Teknik.Data;

namespace Teknik.Areas.Billing
{
    public static class BillingHelper
    {
        public static void ProcessSubscription(Config config, TeknikEntities db, string customerId, Subscription subscription)
        {
            // They have paid, so let's get their subscription and update their user settings
            var user = db.Users.FirstOrDefault(u => u.BillingCustomer != null &&
                                                            u.BillingCustomer.CustomerId == customerId);
            if (user != null)
            {
                var isActive = subscription.Status == SubscriptionStatus.Active;
                foreach (var price in subscription.Prices)
                {
                    ProcessPrice(config, db, user, price, isActive);
                }
            }
        }

        public static void ProcessPrice(Config config, TeknikEntities db, User user, Price price, bool active)
        {
            // What type of subscription is this
            if (config.BillingConfig.UploadProductId == price.ProductId)
            {
                // Process Upload Settings
                user.UploadSettings.MaxUploadStorage = active ? price.Storage : config.UploadConfig.MaxStorage;
                user.UploadSettings.MaxUploadFileSize = active ? price.FileSize : config.UploadConfig.MaxUploadFileSize;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            else if (config.BillingConfig.EmailProductId == price.ProductId)
            {
                // Process an email subscription
                string email = UserHelper.GetUserEmailAddress(config, user.Username);
                if (active)
                {
                    UserHelper.EnableUserEmail(config, email);
                    UserHelper.EditUserEmailMaxSize(config, email, price.Storage);
                }
                else
                {
                    UserHelper.DisableUserEmail(config, email);
                }
            }
        }
    }
}
