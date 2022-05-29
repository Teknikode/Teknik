using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.Services
{
    public class CacheCleanerService : BackgroundService
    {
        private readonly ILogger<Logger> _logger;
        public readonly ObjectCache _cache;

        public CacheCleanerService(ObjectCache cache, ILogger<Logger> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cache Cleaning Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _cache.CleanCache();

                    await Task.Delay(new TimeSpan(0, 5, 0), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred cleaning cache.");
                }
            }

            _logger.LogInformation("Cache Cleaning Service is stopping.");
        }
    }
}
