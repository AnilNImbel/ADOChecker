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
        string GetWorkItem(string projectName, string ids);

        string GetWorkItem(string ids);

        Task<string> GetWorkItemAsync(string ids);

        Task<string> GetWorkItemAsync(string projectName, string ids);

        string GetProjects();

        IterationResult GetSprint(string projectName);

        string GetAllWiqlByType(string projectName, List<string> workItemType, string iterationPath, string filter = "");

        WorkItemModel GetAllWorkItemsByDateRange(DateTime fromDate, DateTime toDate);

        string GetWorkItemForReports(string projectName, string ids);

        Task<WorkItemModel> GetAllWorkItemsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
