using System.Net.Mail;
using System.Net;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;
        public Email() {

            smtpClient = new SmtpClient("smtp-mail.outlook.com")
            {
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential("anil.nimbel@civica.com", ""),
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
