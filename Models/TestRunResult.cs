using System;
using System.ComponentModel.DataAnnotations;

namespace ADOAnalyser.Models
{
    public class TestRunResult
    {
        [Key]
        public int RunId { get; set; }
        public DateTime RunDate { get; set; }
        public string ResultSummary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
