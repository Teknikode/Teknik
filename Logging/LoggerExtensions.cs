using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Teknik.Configuration;

namespace Teknik.Logging
{
    public static class LoggerExtensions
    {
        public static ILoggerFactory AddLogger(this ILoggerFactory loggerFactory, Config config)
        {
            loggerFactory.AddProvider(new LoggerProvider(config));
            return loggerFactory;
        }
    }
}
