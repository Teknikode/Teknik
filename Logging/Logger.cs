using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Logging
{
    public class Logger : ILogger
    {
        private static readonly object Locker = new object();

        private readonly string _name;
        private readonly Config _config;

        public Logger(string name, Config config)
        {
            _name = name;
            _config = config;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // write an entry to the logs
            LogMessage log = new LogMessage();
            log.Level = logLevel;
            log.Message = formatter(state, exception);
            log.Exception = exception;

            WriteLogMessage(log);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_config.LoggingConfig.Enabled)
            {
                // Do we want to write a log for this level? (Default to Error)
                LogLevel minLogLevel = LogLevel.Error;
                Enum.TryParse(_config.LoggingConfig.LogLevel, out minLogLevel);
                if (logLevel >= minLogLevel)
                    return true;
            }
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        private void WriteLogMessage(LogMessage log)
        {
            try
            {
                // Lock the file processing so only 1 thread is working on the log file at a time
                lock (Locker)
                {
                    if (!Directory.Exists(_config.LoggingConfig.OutputDirectory))
                    {
                        Directory.CreateDirectory(_config.LoggingConfig.OutputDirectory);
                    }
                    // Get current log file
                    string fileName = Constants.LOG_FILE_NAME_PREFIX + Constants.LOG_FILE_EXT;
                    string logFile = Path.Combine(_config.LoggingConfig.OutputDirectory, fileName);

                    if (File.Exists(logFile))
                    {
                        // File already exists, so lets see if we need to rotate it
                        if (_config.LoggingConfig.RotateLogs)
                        {
                            FileInfo info = new FileInfo(logFile);
                            if (_config.LoggingConfig.MaxSize < info.Length && _config.LoggingConfig.MaxSize > 0)
                            {
                                // File is too large, so let's create a new name for it based on todays date
                                string newFileName = Constants.LOG_FILE_NAME_PREFIX + "_" + DateTime.Now.ToString("yyyyMMdd") + Constants.LOG_FILE_EXT;
                                newFileName = FileHelper.MakeUniqueFilename(newFileName, _config.LoggingConfig.OutputDirectory);
                                string newLog = Path.Combine(_config.LoggingConfig.OutputDirectory, newFileName);

                                // Move the current file to the new file
                                File.Move(logFile, newLog);
                            }

                            // Make sure we have less than the max number of logs
                            List<string> totalFiles = Directory.GetFiles(_config.LoggingConfig.OutputDirectory, string.Format("{0}*{1}", Constants.LOG_FILE_NAME_PREFIX, Constants.LOG_FILE_EXT), SearchOption.TopDirectoryOnly).ToList();
                            if (totalFiles.Count + 1 > _config.LoggingConfig.MaxCount && _config.LoggingConfig.MaxCount > 0)
                            {
                                // We will have too many logs, so let's remove the last one
                                totalFiles.Sort();
                                string fileToRemove = totalFiles[totalFiles.Count - 1];
                                File.Delete(fileToRemove);
                            }
                        }
                    }

                    // We have rotated if needed, so let's write the entry
                    File.AppendAllText(logFile, log.ToString() + Environment.NewLine);
                }
            }
            catch (Exception) { } // If we throw when writing the log, still try to send the email if needed

            try
            {
                // Send Email Message if enabled
                if (_config.LoggingConfig.SendEmail)
                {
                    // Do we want to send an email for this level?  (Default to error)
                    LogLevel minEmailLevel = LogLevel.Error;
                    Enum.TryParse(_config.LoggingConfig.EmailLevel, out minEmailLevel);
                    if (log.Level >= minEmailLevel)
                    {
                        string subject = string.Format("{0} Log Message", log.Level);
                        string message = "Message: " + log.Message;
                        if (log.Exception != null)
                        {
                            message += Environment.NewLine + Environment.NewLine + "Exception: " + log.Exception.GetFullMessage(true, true);
                        }
                        SendErrorEmail(subject, message);
                    }
                }
            }
            catch (Exception)
            {
                // Can't do anything about it. :/
            }
        }

        private void SendErrorEmail(string subject, string message)
        {
            try
            {
                // Let's also email the message to support
                SmtpClient client = new SmtpClient();
                client.Host = _config.LoggingConfig.SenderAccount.Host;
                client.Port = _config.LoggingConfig.SenderAccount.Port;
                client.EnableSsl = _config.LoggingConfig.SenderAccount.SSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new System.Net.NetworkCredential(_config.LoggingConfig.SenderAccount.Username, _config.LoggingConfig.SenderAccount.Password);
                client.Timeout = 5000;

                MailMessage mail = new MailMessage(_config.LoggingConfig.SenderAccount.EmailAddress, _config.LoggingConfig.RecipientEmailAddress);
                mail.Subject = subject;
                mail.Body = message;
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                client.Send(mail);
            }
            catch (Exception) { /* don't handle something in the handler */
            }
        }
    }
}
