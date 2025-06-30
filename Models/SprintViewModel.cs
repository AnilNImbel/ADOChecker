namespace ADOAnalyser.Models
{
    public class SprintViewModel
    {
        public string? SelectedSprint { get; set; }
        public List<SprintDropdownItem> AllSprints { get; set; }
        public WorkItemModel WorkItemData { get; set; }
    }

    public class SprintDropdownItem
    {
        public string FullPath { get; set; } // e.g., "CE\\The Mastermind\\Sprint-20"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DisplayName => $"{FullPath} (Date: {StartDate?.ToString("dd-MM-yyyy")} - {EndDate?.ToString("dd-MM-yyyy")})";
    }
}
