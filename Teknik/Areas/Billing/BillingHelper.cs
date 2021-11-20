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
        public static Models.Customer CreateCustomer(TeknikEntities db, User user, string customerId)
        {
            var customer = new Models.Customer()
            {
                CustomerId = customerId,
                User = user
            };
            db.Customers.Add(customer);
            user.BillingCustomer = customer;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return customer;
        }

        public static void RemoveCustomer(TeknikEntities db, string customerId)
        {
            // They have paid, so let's get their subscription and update their user settings
            var user = db.Users.FirstOrDefault(u => u.BillingCustomer != null &&
                                                    u.BillingCustomer.CustomerId == customerId);
            if (user != null &&
                user.BillingCustomer != null)
            {
                db.Customers.Remove(user.BillingCustomer);
                user.BillingCustomer = null;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

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
                SetUploadLimits(db, 
                                  user, 
                                  active ? price.Storage : config.UploadConfig.MaxStorage, 
                                  active ? price.FileSize : config.UploadConfig.MaxUploadFileSize);
            }
            else if (config.BillingConfig.EmailProductId == price.ProductId)
            {
                SetEmailLimits(config, user, price.Storage, active);
            }
        }

        public static void SetUploadLimits(TeknikEntities db, User user, long storage, long fileSize)
        {
            // Process Upload Settings
            user.UploadSettings.MaxUploadStorage = storage;
            user.UploadSettings.MaxUploadFileSize = fileSize;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void SetEmailLimits(Config config, User user, long storage, bool active)
        {
            // Process an email subscription
            string email = UserHelper.GetUserEmailAddress(config, user.Username);
            if (active)
            {
                UserHelper.EnableUserEmail(config, email);
                UserHelper.EditUserEmailMaxSize(config, email, storage);
            }
            else
            {
                UserHelper.DisableUserEmail(config, email);
            }
        }
    }
}
