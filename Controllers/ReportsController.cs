using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ADOAnalyser.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
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
            csvBuilder.AppendLine("AdoItemId,CallReference,ImpactAssessment,RootCauseAnalysis,ProjectZero,PRLifecycle,StatusDiscrepancy,TestCaseGap,CurrentStatus,TechnicalLeadName,DevName");

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
                       Escape(d.CurrentStatus ?? "")
                    ));
            }

            var fileName = $"TestRunDetails_{runId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            return File(fileBytes, "text/csv", fileName);
        }
    }
}
