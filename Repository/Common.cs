using ADOAnalyser.IRepository;
using ADOAnalyser.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Text;

namespace ADOAnalyser.Repository
{
    public class Common : ICommon
    {
        public string CreateCSV(List<TestRunDetail> details)
        {
            var csvBuilder = new StringBuilder();

            // Write CSV header
            csvBuilder.AppendLine("Work Item Type,Id,Impact Assessment,Root Cause Analysis,Project Zero,PR Lifecycle,Status Discrepancy,Test Case,Technical Lead Name,Assigned To,State");

            // Write CSV rows
            foreach (var d in details)
            {
                // Escape commas and quotes if necessary
                string Escape(string s) => string.IsNullOrEmpty(s) ? "" : $"\"{s.Replace("\"", "\"\"")}\"";

                csvBuilder.AppendLine(string.Join(",",
                       Escape(d.WorkitemType ?? ""),
                       Escape(d.AdoItemId ?? ""),
                       //Escape(d.CallReference ?? ""),
                       Escape(d.ImpactAssessment ?? ""),
                       Escape(d.RootCauseAnalysis ?? ""),
                       Escape(d.ProjectZero ?? ""),
                       Escape(d.PRLifecycle ?? ""),
                       Escape(d.StatusDiscrepancy ?? ""),
                       Escape(d.TestCaseGap ?? ""),
                       Escape(d.TechnicalLeadName == null ? "" : d.TechnicalLeadName.Split('<')[0].Trim()),
                       Escape(d.AssignedTo ?? ""),
                       Escape(d.CurrentStatus ?? "")
                    ));
            }

            return csvBuilder.ToString();
        }
    }
}
