using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Logging
{
    public static class Logger
    {
        private static readonly object Locker = new object();

        public static void WriteEntry(string message)
        {
            WriteEntry(LogLevel.Info, message, null);
        }

        public static void WriteEntry(LogLevel level, string message)
        {
            WriteEntry(level, message, null);
        }

        public static void WriteEntry(Exception ex)
        {
            WriteEntry(LogLevel.Error, ex.Message, ex);
        }

        public static void WriteEntry(string message, Exception ex)
        {
            WriteEntry(LogLevel.Error, message, ex);
        }

        public static void WriteEntry(LogLevel level, string message, Exception exception)
        {
            // write an entry to the logs
            LogMessage log = new LogMessage();
            log.Level = level;
            log.Message = message;
            log.Exception = exception;

            WriteLogMessage(log);
        }

        private static void WriteLogMessage(LogMessage log)
        {
            try
            {
                Config config = Config.Load();
                if (config.LoggingConfig.Enabled)
                {
                    // Do we want to write a log for this level? (Default to Error)
                    LogLevel minLogLevel = LogLevel.Error;
                    Enum.TryParse(config.LoggingConfig.LogLevel, out minLogLevel);

                    try
                    {
                        if (log.Level >= minLogLevel)
                        {
                            // Lock the file processing so only 1 thread is working on the log file at a time
                            lock (Locker)
                            {
                                if (!Directory.Exists(config.LoggingConfig.OutputDirectory))
                                {
                                    Directory.CreateDirectory(config.LoggingConfig.OutputDirectory);
                                }
                                // Get current log file
                                string fileName = Constants.LOG_FILE_NAME_PREFIX + Constants.LOG_FILE_EXT;
                                string logFile = Path.Combine(config.LoggingConfig.OutputDirectory, fileName);

                                if (File.Exists(logFile))
                                {
                                    // File already exists, so lets see if we need to rotate it
                                    if (config.LoggingConfig.RotateLogs)
                                    {
                                        FileInfo info = new FileInfo(logFile);
                                        if (config.LoggingConfig.MaxSize < info.Length && config.LoggingConfig.MaxSize > 0)
                                        {
                                            // File is too large, so let's create a new name for it based on todays date
                                            string newFileName = Constants.LOG_FILE_NAME_PREFIX + "_" + DateTime.Now.ToString("yyyyMMdd") + Constants.LOG_FILE_EXT;
                                            newFileName = FileHelper.MakeUniqueFilename(newFileName, config.LoggingConfig.OutputDirectory);
                                            string newLog = Path.Combine(config.LoggingConfig.OutputDirectory, newFileName);

                                            // Move the current file to the new file
                                            File.Move(logFile, newLog);
                                        }

                                        // Make sure we have less than the max number of logs
                                        List<string> totalFiles = Directory.GetFiles(config.LoggingConfig.OutputDirectory, string.Format("{0}*{1}", Constants.LOG_FILE_NAME_PREFIX, Constants.LOG_FILE_EXT), SearchOption.TopDirectoryOnly).ToList();
                                        if (totalFiles.Count + 1 > config.LoggingConfig.MaxCount && config.LoggingConfig.MaxCount > 0)
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
                    }
                    catch (Exception) { } // If we throw when writing the log, still try to send the email if needed

                    // Send Email Message if enabled
                    if (config.LoggingConfig.SendEmail)
                    {
                        // Do we want to send an email for this level?  (Default to error)
                        LogLevel minEmailLevel = LogLevel.Error;
                        Enum.TryParse(config.LoggingConfig.EmailLevel, out minEmailLevel);
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
            }
            catch (Exception)
            {
                // Can't do anything about it. :/
            }
        }

        private static void SendErrorEmail(string subject, string message)
        {
            try
            {
                Config config = Config.Load();
                // Let's also email the message to support
                SmtpClient client = new SmtpClient();
                client.Host = config.LoggingConfig.SenderAccount.Host;
                client.Port = config.LoggingConfig.SenderAccount.Port;
                client.EnableSsl = config.LoggingConfig.SenderAccount.SSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential(config.LoggingConfig.SenderAccount.Username, config.LoggingConfig.SenderAccount.Password);
                client.Timeout = 5000;

                MailMessage mail = new MailMessage(config.LoggingConfig.SenderAccount.EmailAddress, config.LoggingConfig.RecipientEmailAddress);
                mail.Subject = subject;
                mail.Body = message;
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                client.Send(mail);
            }
            catch (Exception ex) { /* don't handle something in the handler */
            }
        }
    }
}
