using System.Net.Mail;
using System.Net;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.Extensions.Options;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;

        private readonly EmailSetting _settings;

        private readonly string emailFrom;

        public Email(IOptions<EmailSetting> settings) 
        {
            _settings = settings.Value;
            emailFrom = _settings.EmailFrom;
            smtpClient = new SmtpClient(_settings.Server)
            {
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                Timeout = 30000
            };
        }

        public void EmailSend(string body, string ToEmail) {

            try
            {

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = "Work Item Verification",
                    Body = body + "<br> Missing Items",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(ToEmail);

                 smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw ;
            }

        }

        public class EmailSetting
        {
            public string Server { get; set; }
            public string EmailFrom { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }


    }
}
