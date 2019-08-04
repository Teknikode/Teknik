using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Logging;

namespace Teknik
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    string baseDir = hostingContext.HostingEnvironment.ContentRootPath;
                    string dataDir = Path.Combine(baseDir, "App_Data");
                    logging.AddProvider(new LoggerProvider(Config.Load(dataDir)));
                })
                .Build();
        }
    }
}
