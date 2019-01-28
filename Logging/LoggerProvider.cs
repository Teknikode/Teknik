using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Teknik.Configuration;

namespace Teknik.Logging
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly Config _config;
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();

        public LoggerProvider(Config config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new Logger(name, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
