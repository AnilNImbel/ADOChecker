using System.Net.Mail;
using System.Net;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;
        public Email() 
        {

            WebProxy proxy = new WebProxy("http://webproxy.civica.com:8080")
            {
                Credentials = CredentialCache.DefaultCredentials,
                BypassProxyOnLocal = true
            };

            // Set the default proxy for all web requests (including SMTP if applicable)
            WebRequest.DefaultWebProxy = proxy;

            smtpClient = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("anil.nimbel@civica.com", "@Annu90334M#1"),
                Timeout = 30000
            };
        }

        public void EmailSend(string body, string ToEmail) {

            try
            {

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("anil.nimbel@civica.com"),
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

    }
}
