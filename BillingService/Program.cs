using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Teknik.Areas.Billing;
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

            // Get all customers
            var customers = billingService.GetCustomers();
            if (customers != null)
            {
                var customerIds = customers.Select(c => c.CustomerId).ToList();
                // Find customers that aren't linked anymore
                var unlinkedCustomers = db.Users.Select(u => u.BillingCustomer).Where(b => b != null && !customerIds.Contains(b.CustomerId));
                foreach (var customer in unlinkedCustomers)
                {
                    BillingHelper.RemoveCustomer(db, customer.CustomerId);
                }
            }

            foreach (var user in db.Users.Include(u => u.BillingCustomer))
            {
                // Only set/reset their limits if they have a subscription or have subscribed at some point
                if (user.BillingCustomer != null)
                {
                    // get the subscriptions for this user
                    var subscriptions = billingService.GetSubscriptionList(user.BillingCustomer.CustomerId);
                    var uploadPrice = subscriptions.SelectMany(s => s.Prices).FirstOrDefault(p => p.ProductId == config.BillingConfig.UploadProductId);

                    // Process upload subscription sync
                    if (uploadPrice != null)
                    {
                        BillingHelper.SetUploadLimits(db, user, uploadPrice.Storage, uploadPrice.FileSize);
                    }
                    else
                    {
                        BillingHelper.SetUploadLimits(db, user, config.UploadConfig.MaxStorage, config.UploadConfig.MaxUploadFileSize);
                    }

                    // Process email subscription sync
                    var emailPrice = subscriptions.SelectMany(s => s.Prices).FirstOrDefault(p => p.ProductId == config.BillingConfig.EmailProductId);
                    if (emailPrice != null)
                    {
                        BillingHelper.SetEmailLimits(config, user, emailPrice.Storage, true);
                    }
                    else
                    {
                        BillingHelper.SetEmailLimits(config, user, config.EmailConfig.MaxSize, false);
                    }
                }
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
