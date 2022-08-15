using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.BillingService
{
    internal class Logger : ILogger
    {
        private readonly TextWriter _textWriter;

        public Logger(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _textWriter.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
    }
}
