using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace ADOAnalyser.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Email _email;

        public ReportsController(AppDbContext context, Email email)
        {
            _context = context;
            _email =   email;
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

            var csvBuilder = new StringBuilder();

            // Write CSV header
            csvBuilder.AppendLine("Work Item Type,Ado Item Id,Call Reference,Impact Assessment,Root Cause Analysis,Project Zero,PR Lifecycle,Status Discrepancy,TestCase Gaps,Current Stage,Technical Lead Name");

            // Write CSV rows
            foreach (var d in details)
            {
                // Escape commas and quotes if necessary
                string Escape(string s) => string.IsNullOrEmpty(s) ? "" : $"\"{s.Replace("\"", "\"\"")}\"";

                csvBuilder.AppendLine(string.Join(",",
                       Escape(d.WorkitemType ?? ""),
                       Escape(d.AdoItemId ?? ""),
                       Escape(d.CallReference ?? ""),
                       Escape(d.ImpactAssessment ?? ""),
                       Escape(d.RootCauseAnalysis ?? ""),
                       Escape(d.ProjectZero ?? ""),
                       Escape(d.PRLifecycle ?? ""),
                       Escape(d.StatusDiscrepancy ?? ""),
                       Escape(d.TestCaseGap ?? ""),
                       Escape(d.CurrentStatus ?? ""),
                       Escape(d.TechnicalLeadName ?? "")
                    ));
            }

            var fileName = $"TestRunDetails_{runId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            return File(fileBytes, "text/csv", fileName);
        }

        [HttpGet]
        public IActionResult EmailSend(int runId)
        {
            var emailCollection = _context.TestRunDetails
             .Where(a => a.TechnicalLeadName != null)
             .AsEnumerable() // Forces in-memory evaluation
                .GroupBy(a => a.TechnicalLeadName)
             .Select(g => new EmailCollectionModel
             {
                 Email = g.Key.Substring(g.Key.IndexOf('<') + 1, g.Key.IndexOf('>') - g.Key.IndexOf('<') - 1),
                 Body = string.Join(", ", g.Select(x => x.AdoItemId))
             })
             .ToList();


            if (!emailCollection.Any())
            {
                TempData["AlertMessage"] = "No Email Found!";
            }
            try
            {
                if (emailCollection.Any())
                {
                    foreach (var email in emailCollection)
                    {
                        _email.EmailSend(email.Body, "anil.nimbel@civica.com");
                    }
                    TempData["AlertMessage"] = "Email Send Successfully.";
                }
            }
            catch(Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
            }
            return RedirectToAction("Index", "Reports");
        }
    }
}
