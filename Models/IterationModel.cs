namespace ADOAnalyser.Models
{
    public class IterationRoot
    {
        public string name { get; set; }
        public List<IterationNode> children { get; set; }
    }

    public class IterationNode
    {
        public int id { get; set; }
        public string name { get; set; }
        public IterationAttributes Attributes { get; set; }
        public List<IterationNode> children { get; set; }
    }

    public class IterationAttributes
    {
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
    }

    public class IterationNodeWithPath
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public IterationAttributes Attributes { get; set; }
    }
}
