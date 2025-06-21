using System.ComponentModel.DataAnnotations;

namespace ADOAnalyser.Models
{
    public class TestRunDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int RunId { get; set; }

        public string? AdoItemId { get; set; }
        public string? CallReference { get; set; }
        public string? ImpactAssessment { get; set; }
        public string? RootCauseAnalysis { get; set; }
        public string? ProjectZero { get; set; }
        public string? PRLifecycle { get; set; }
        public string? StatusDiscrepancy { get; set; }
        public string? TestCaseGap { get; set; }
        public string? TestCaseGapeHTML { get; set; }
        public string? CurrentStatus { get; set; }
        public string? TechnicalLeadName { get; set; }
        public string? DevName { get; set; }
        public string? WorkitemType { get; set; }
    }
}
