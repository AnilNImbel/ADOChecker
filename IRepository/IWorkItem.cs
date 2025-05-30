using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ADOAnalyser.Utility;
namespace ADOAnalyser
{
    public interface IWorkItem
    {
        string GetWorkItem(string projectName, string ids);

        string GetProjects();
        string GetAllWiql(string projectName);

        string GetAllWiqlByType(string projectName, string workItemType, List<IterationNodeWithPath> iterationPath);

        List<IterationNodeWithPath> GetSprint(string projectName);
    }
}
