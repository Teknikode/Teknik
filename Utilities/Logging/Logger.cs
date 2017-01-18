using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.Logging
{
    public class Logger
    {
        private Config m_Config;

        public Logger(Config config)
        {
            m_Config = config;
        }

        public void WriteEntry(Exception ex)
        {
            // write an entry to the logs

        }

        public void WriteEntry(string message, LogLevel level)
        {
            if (m_Config.LoggingConfig.Enabled)
            {

            }
        }

        private void SendErrorEmail(string subject, string message)
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
