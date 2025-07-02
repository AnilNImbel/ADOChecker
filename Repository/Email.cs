using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Office.Interop.Outlook;
using ADOAnalyser.Models;
using ADOAnalyser.IRepository;
using System.Text;
using System;
namespace ADOAnalyser.Repository
{
    public class Email
    {
        private SmtpClient smtpClient;

        private readonly EmailSetting _settings;

        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        private readonly string emailFrom;

        private readonly ICommon _common;

        public Email(IOptions<EmailSetting> settings, IRazorViewToStringRenderer razorViewToStringRenderer, ICommon common)
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
            _common = common;
        }

        public void EmailSend(List<TestRunDetail> workItem, string ToEmail, int runId) 
        {
            try
            {
                string emailBody = _razorViewToStringRenderer.RenderViewToString("EmailTemplate/Index", workItem);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom),
                    Subject = "ADO Spot Check – Missing Information Identified",
                    Body = emailBody,
                    IsBodyHtml = true,
                };

                //CSV Creation 
                var csvContent = _common.CreateCSV(workItem);

                var fileName = $"TestRunDetails_{runId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                byte[] byteArray = Encoding.UTF8.GetBytes(csvContent.ToString());
                MemoryStream stream = new MemoryStream(byteArray);

                //Attachment
                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(stream, fileName, "text/csv");
                mailMessage.Attachments.Add(attachment);

                //recipients
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
            catch (System.Exception)
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
