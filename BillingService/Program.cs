using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommandLine;
using Microsoft.EntityFrameworkCore;
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
