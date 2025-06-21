using ADOAnalyser.Models;

namespace ADOAnalyser.IRepository
{
    public interface IRazorViewToStringRenderer
    {
        string RenderViewToString(string viewName, List<TestRunDetail> model);
    }

}
