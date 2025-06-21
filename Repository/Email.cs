using System.Net.Mail;
using System.Net;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ADOAnalyser.Models;
using ADOAnalyser.IRepository;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;

        private readonly EmailSetting _settings;

        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        private readonly string emailFrom; 

        public Email(IOptions<EmailSetting> settings, IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _settings = settings.Value;
            emailFrom = _settings.EmailFrom;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
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

        public void EmailSend(List<TestRunDetail> workItem, string ToEmail) {

            try
            {
                string emailBody = _razorViewToStringRenderer.RenderViewToString("EmailTemplate/Index", workItem);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = "Work Item Verification",
                    Body = emailBody,
                    IsBodyHtml = true,
                };


                var recipients = ToEmail
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .ToList();

                foreach (var recipient in recipients)
                {
                    mailMessage.To.Add(recipient);
                }


                smtpClient.Send(mailMessage);
            }
            catch (System.Exception ex)
            {
                throw ;
            }

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
