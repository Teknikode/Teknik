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
    public static class Logging
    {
        private static Config m_Config
        {
            get
            {
                return Config.Load();
            }
        }

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

            WriteEntry(log);
        }

        public static void WriteEntry(LogMessage log)
        {
            if (m_Config.LoggingConfig.Enabled)
            {
                // Do we want to write a log for this level? (Default to Error)
                LogLevel minLogLevel = LogLevel.Error;
                Enum.TryParse(m_Config.LoggingConfig.EmailLevel, out minLogLevel);

                if (log.Level >= minLogLevel)
                {
                    if (!Directory.Exists(m_Config.LoggingConfig.OutputDirectory))
                    {
                        Directory.CreateDirectory(m_Config.LoggingConfig.OutputDirectory);
                    }
                    // Get current log file
                    string fileName = Constants.LOG_FILE_NAME_PREFIX + Constants.LOG_FILE_EXT;
                    string logFile = Path.Combine(m_Config.LoggingConfig.OutputDirectory, fileName);

                    if (File.Exists(logFile))
                    {
                        // File already exists, so lets see if we need to rotate it
                        if (m_Config.LoggingConfig.RotateLogs)
                        {
                            FileInfo info = new FileInfo(logFile);
                            if (m_Config.LoggingConfig.MaxSize < info.Length && m_Config.LoggingConfig.MaxSize > 0)
                            {
                                // File is too large, so let's create a new name for it based on todays date
                                string newFileName = Constants.LOG_FILE_NAME_PREFIX + "_" + DateTime.Now.ToString("yyyyMMdd") + Constants.LOG_FILE_EXT;
                                newFileName = FileHelper.MakeUniqueFilename(newFileName, m_Config.LoggingConfig.OutputDirectory);
                                string newLog = Path.Combine(m_Config.LoggingConfig.OutputDirectory, newFileName);

                                // Move the current file to the new file
                                File.Move(logFile, newLog);
                            }

                            // Make sure we have less than the max number of logs
                            List<string> totalFiles = Directory.GetFiles(m_Config.LoggingConfig.OutputDirectory, string.Format("{0}*{1}", Constants.LOG_FILE_NAME_PREFIX, Constants.LOG_FILE_EXT), SearchOption.TopDirectoryOnly).ToList();
                            if (totalFiles.Count + 1 > m_Config.LoggingConfig.MaxCount && m_Config.LoggingConfig.MaxCount > 0)
                            {
                                // We will have too many logs, so let's remove the last one
                                totalFiles.Sort();
                                string fileToRemove = totalFiles[totalFiles.Count - 1];
                                File.Delete(fileToRemove);
                            }
                        }
                    }

                    // We have rotated if needed, so let's write the entry
                    File.AppendAllText(logFile, log.ToString());
                }

                // Send Email Message if enabled
                if (m_Config.LoggingConfig.SendEmail)
                {
                    // Do we want to send an email for this level?  (Default to error)
                    LogLevel minEmailLevel = LogLevel.Error;
                    Enum.TryParse(m_Config.LoggingConfig.EmailLevel, out minEmailLevel);
                    if (log.Level >= minEmailLevel)
                    {
                        string subject = string.Format("{0} Log Message");
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

        private static void SendErrorEmail(string subject, string message)
        {
            try
            {
                // Let's also email the message to support
                SmtpClient client = new SmtpClient();
                client.Host = m_Config.LoggingConfig.SenderAccount.Host;
                client.Port = m_Config.LoggingConfig.SenderAccount.Port;
                client.EnableSsl = m_Config.LoggingConfig.SenderAccount.SSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential(m_Config.LoggingConfig.SenderAccount.Username, m_Config.LoggingConfig.SenderAccount.Password);
                client.Timeout = 5000;

                MailMessage mail = new MailMessage(m_Config.LoggingConfig.SenderAccount.EmailAddress, m_Config.LoggingConfig.RecipientEmailAddress);
                mail.Subject = subject;
                mail.Body = message;
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                client.Send(mail);
            }
            catch (Exception) { /* don't handle something in the handler */ }
        }
    }
}
