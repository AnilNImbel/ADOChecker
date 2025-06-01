namespace ADOAnalyser.Models
{
    public class SprintViewModel
    {
        public string? SelectedSprint { get; set; }
        public List<string> AllSprints { get; set; }
        public WorkItemModel WorkItemData { get; set; }
    }
}
