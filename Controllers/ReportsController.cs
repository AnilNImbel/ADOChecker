using ADOAnalyser.DBContext;
using ADOAnalyser.IRepository;
using ADOAnalyser.Models;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADOAnalyser.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Email _email;
        private readonly ICommon _common;

        public ReportsController(AppDbContext context, Email email,ICommon common)
        {
            _context = context;
            _email =   email;
            _common = common;
        }

        public IActionResult Index()
        {
            var reports = _context.TestRunResults
                .OrderByDescending(r => r.RunDate)
                .ToList();

            return View(reports);
        }

        [HttpGet]
        public IActionResult DownloadCsv(int runId)
        {
            var details = _context.TestRunDetails
                .Where(d => d.RunId == runId)
                .ToList();

            if (!details.Any())
                return NotFound();

            var csvBuilder = _common.CreateCSV(details);

            var fileName = $"TestRunDetails_{runId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            return File(fileBytes, "text/csv", fileName);
        }


        [HttpGet]
        public JsonResult EmailSend(int runId)
        {
            var approvedlist = ApprovedEmailList();
            var emailCollection =_context.TestRunDetails
                                     .Where(a => a.RunId == runId)
                                     .AsEnumerable()
                                     .GroupBy(a =>
                                     {
                                         // Extract email from TechnicalLeadName
                                         if (string.IsNullOrEmpty(a.TechnicalLeadName) || !a.TechnicalLeadName.Contains("<") || !a.TechnicalLeadName.Contains(">"))
                                             return "null";

                                         var extractedEmail = a.TechnicalLeadName.Substring(
                                         a.TechnicalLeadName.IndexOf('<') + 1,
                                         a.TechnicalLeadName.IndexOf('>') - a.TechnicalLeadName.IndexOf('<') - 1
                                         ).ToLower();

                                         return approvedlist.Contains(extractedEmail) ? extractedEmail : "null";
                                     })
                                     .Select(g => new EmailCollectionModel
                                     {
                                         Email = g.Key == "null" ? null : g.Key,
                                         workIds = string.Join(", ", g.Select(x => x.AdoItemId))
                                     })
                                     .ToList();



            var EmailConfig = _context.EmailConfig.Where(a => a.IsActive).ToList();

            if (!emailCollection.Any())
            {
                return Json(new { success = false, message = "No Data Found!" });
            }

            try
            {
                if (emailCollection.Any())
                {
                    foreach (var email in emailCollection)
                    {
                        if(email.Email != null) //&& ApprovedEmailList().Contains(email.Email))
                        {

                            var allEmails = email.Email +
                                                         (EmailConfig.Any()
                                                         ? ", " + string.Join(", ", EmailConfig.Select(a => a.EmailId))
                                                         : string.Empty);

                            _email.EmailSend(GridData(email.workIds, runId),  allEmails, runId);
                        }
                        else
                        {
                            if (EmailConfig.Any())
                            {
                                _email.EmailSend(GridData(email.workIds, runId), string.Join(", ", EmailConfig.Select(a => a.EmailId)), runId);
                            }
                            else
                            {
                                if(emailCollection.Count == 1)
                                {
                                    return Json(new { success = false, message = "No Email Found!" });
                                }
                            }
                        }
                    }
                }
                return Json(new { success = true, message = "Email sent successfully." });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<string> ApprovedEmailList()
        {
            var emailList = AppSettingsReader.GetValue("Others", "ApprovedTLEmail");

            List<string> approvedList = emailList
             .Split(',', StringSplitOptions.RemoveEmptyEntries)
             .Select(s => s.Trim())
             .ToList();

            return approvedList;
        }


        private List<TestRunDetail> GridData(string workIds, int runID)
        {
            var data = _context.TestRunDetails.Where(a => a.RunId == runID).ToList();

            return data;
        }
    }
}
