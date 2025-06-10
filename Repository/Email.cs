using System.Net.Mail;
using System.Net;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;
        public Email() {

            smtpClient = new SmtpClient("outlook.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("anil.nimbel@civica.com", ""),
                EnableSsl = true,
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
