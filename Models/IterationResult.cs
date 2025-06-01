using static ADOAnalyser.Utility;

namespace ADOAnalyser.Models
{
    public class IterationResult
    {
        public List<IterationNodeWithPath> CurrentSprints { get; set; } = new();
        public List<IterationNodeWithPath> AllSprints { get; set; } = new();
    }
}
