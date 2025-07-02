using ADOAnalyser.Models;

namespace ADOAnalyser.IRepository
{
    public interface ICommon
    {
        public string CreateCSV(List<TestRunDetail> details);
    }
}
