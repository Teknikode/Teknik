using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.BillingCore;
using Teknik.BillingCore.Models;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Utilities;

namespace Teknik.BillingService
{
    public class Program
    {
        private static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string errorFile = Path.Combine(currentPath, "errorLogs.txt");
        private static string configPath = currentPath;

        private static readonly object dbLock = new object();
        private static readonly object scanStatsLock = new object();

        public static event Action<string> OutputEvent;

        public static int Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ArgumentOptions>(args).WithParsed(options =>
                {
                    if (!string.IsNullOrEmpty(options.Config))
                        configPath = options.Config;

                    if (Directory.Exists(configPath))
                    {
                        Config config = Config.Load(configPath);
                        Output(string.Format("[{0}] Started Billing Service Process.", DateTime.Now));

                        var optionsBuilder = new DbContextOptionsBuilder<TeknikEntities>();
                        optionsBuilder.UseSqlServer(config.DbConnection);

                        using (TeknikEntities db = new TeknikEntities(optionsBuilder.Options))
                        {
                            if (options.SyncSubscriptions)
                            {
                                // Sync subscription information
                                SyncSubscriptions(config, db);
                            }
                        }

                        Output(string.Format("[{0}] Finished Billing Service Process.", DateTime.Now));
                    }
                    else
                    {
                        string msg = string.Format("[{0}] Config File does not exist.", DateTime.Now);
                        File.AppendAllLines(errorFile, new List<string> { msg });
                        Output(msg);
                    }
                });
            }
            catch (Exception ex)
            {
                string msg = string.Format("[{0}] Exception: {1}", DateTime.Now, ex.GetFullMessage(true));
                File.AppendAllLines(errorFile, new List<string> { msg });
                Output(msg);
            }
            return -1;
        }

        public static void SyncSubscriptions(Config config, TeknikEntities db)
        {
            // Get Biling Service
            var billingService = BillingFactory.GetBillingService(config.BillingConfig);

            foreach (var user in db.Users)
            {
                string email = UserHelper.GetUserEmailAddress(config, user.Username);
                if (user.BillingCustomer != null)
                {
                    // get the subscriptions for this user
                    var subscriptions = billingService.GetSubscriptionList(user.BillingCustomer.CustomerId);
                    var uploadPrice = subscriptions.SelectMany(s => s.Prices).FirstOrDefault(p => p.ProductId == config.BillingConfig.UploadProductId);
                    if (uploadPrice != null)
                    {
                        // Process Upload Settings
                        user.UploadSettings.MaxUploadStorage = uploadPrice.Storage;
                        user.UploadSettings.MaxUploadFileSize = uploadPrice.FileSize;
                    }
                    var emailPrice = subscriptions.SelectMany(s => s.Prices).FirstOrDefault(p => p.ProductId == config.BillingConfig.EmailProductId);
                    if (emailPrice != null)
                    {
                        UserHelper.EnableUserEmail(config, email);
                        UserHelper.EditUserEmailMaxSize(config, email, emailPrice.Storage);
                    }
                }
                else
                {
                    // No customer, so let's reset their info
                    user.UploadSettings.MaxUploadStorage = config.UploadConfig.MaxStorage;
                    user.UploadSettings.MaxUploadFileSize = config.UploadConfig.MaxUploadFileSize;
                    UserHelper.DisableUserEmail(config, email);
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void Output(string message)
        {
            Console.WriteLine(message);
            if (OutputEvent != null)
            {
                OutputEvent(message);
            }
        }
    }
}
