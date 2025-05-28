namespace ADOAnalyser.Models
{
    public class WiqlModel
    {
        public string queryType { get; set; }
        public DateTime asOf { get; set; }
        public List<Column> columns { get; set; }
        public List<SortColumn> sortColumns { get; set; }
        public List<WorkItem> workItems { get; set; }
    }

    public class Column
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Field
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class SortColumn
    {
        public Field field { get; set; }
        public bool descending { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public string url { get; set; }
    }
}
