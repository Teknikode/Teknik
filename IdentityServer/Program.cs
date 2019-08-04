using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Teknik.Configuration;
using Teknik.Logging;

namespace Teknik.IdentityServer
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
