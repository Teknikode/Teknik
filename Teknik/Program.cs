using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Teknik.Configuration;
using Teknik.Logging;

namespace Teknik
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Microsoft.AspNetCore.Routing.UseCorrectCatchAllBehavior",
                                  true);
            BuildWebHost(args).Build().Run();
        }

        public static IHostBuilder BuildWebHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    string baseDir = hostingContext.HostingEnvironment.ContentRootPath;
                    string dataDir = Path.Combine(baseDir, "App_Data");
                    logging.AddProvider(new LoggerProvider(Config.Load(dataDir)));
                    logging.AddFilter<ConsoleLoggerProvider>("Microsoft.AspNetCore.Routing", LogLevel.Trace);
                });

            /*
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    string baseDir = hostingContext.HostingEnvironment.ContentRootPath;
                    string dataDir = Path.Combine(baseDir, "App_Data");
                    logging.AddProvider(new LoggerProvider(Config.Load(dataDir)));
                });
            */
        }
    }
}
