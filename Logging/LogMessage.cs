using Microsoft.Extensions.Logging;
using System;
using Teknik.Utilities;

namespace Teknik.Logging
{
    public class LogMessage
    {
        public LogLevel Level { get; set; }
        public DateTime EntryDate { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public LogMessage()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Level = LogLevel.Information;
            EntryDate = DateTime.Now;
            Message = string.Empty;
            Exception = null;
        }

        public override string ToString()
        {
            // Create the full message we want to log
            string fullMessage = Message;
            if (Exception != null)
            {
                fullMessage += " | Exception: " + Exception.GetFullMessage(true, true);
            }

            // We have rotated if needed, so let's write the entry
            return string.Format("{0} | {1} | {2}", Level, EntryDate.ToString("yyyy-MM-dd HH:mm:ss.fff"), fullMessage);
        }
    }
}
