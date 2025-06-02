namespace ADOAnalyser.Models
{
    public class TestRunReportViewModel
    {
            public int RunId { get; set; }
            public DateTime RunDate { get; set; }
            public string ResultSummary { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
    }
}
