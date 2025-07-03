using ADOAnalyser.Models;
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
        string GetPipelines(string project, string folder);

        string GetBuilds(string project, int definitionId);

        Task<string> GetPipelinesAsync(string project, string folder);
        Task<string> GetBuildsAsync(string project, int definitionId);
        string GetWorkItem(string projectName, string ids);

        string GetWorkItem(string ids);

        Task<string> GetWorkItemAsync(string ids);

        Task<string> GetWorkItemAsync(string projectName, string ids);

        string GetProjects();

        IterationResult GetSprint(string projectName);

        string GetAllWiqlByType(string projectName, List<string> workItemType, string iterationPath, string filter = "");

        Task<WorkItemModel> GetAllWiqlSearchAsync(string projectName, List<string> workItemType, string filter = "");

        string GetWorkItemForReports(string projectName, string ids);

        Task<WorkItemModel> GetAllWorkItemsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
