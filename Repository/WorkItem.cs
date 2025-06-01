using ADOAnalyser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static ADOAnalyser.Utility;
namespace ADOAnalyser
{

    public class WorkItem : IWorkItem
    {
        private readonly IUtility _Utility;

        public WorkItem(IUtility Utility)
        {
            _Utility = Utility;
        }

        public IterationResult GetSprint(string projectName)
        {
            return _Utility.GetCurrentIterationAsync(projectName);
        }

        public string GetWorkItem(string projectName, string ids)
        {
            string Url = string.Format("{0}/_apis/wit/workitems?ids={1}&api-version=7.1-preview.2", projectName, ids);
            return _Utility.GetDataSync(Url);
            //return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public string GetProjects()
        {
            string Url = string.Format("_apis/projects?getDefaultTeamImageUrl=true");
            return _Utility.GetDataSync(Url);
            //return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public string GetAllWiql(string projectName)
        {
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);
            var query = new
            {
                query = @"
                SELECT [System.Id], [System.Title]
                FROM WorkItems
                WHERE [System.WorkItemType] = 'Task'
                  AND [System.AssignedTo] = ''"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            var result = _Utility.PostDataSync(Url, content);
            return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }
        public string GetAllWiqlByType(string projectName, string workItemType, string iterationPath)
        {
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);
            var iterationFilter = string.Join(" OR ", iterationPath.Select(i =>
       $"[System.IterationPath] = '{iterationPath.Replace("'", "''")}'"));

            var query = new
            {
                query = $@"Select [System.Id], [System.Title], [System.State] 
                     From WorkItems
                     Where [System.WorkItemType] = '{workItemType}'
                        AND [System.TeamProject] = '{projectName}'
                        AND ({iterationFilter})
                     order by [System.CreatedDate] desc"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            return _Utility.PostDataSync(Url, content);
        }

        public WorkItemModel GetAllWorkItemsByDateRange(DateTime fromDate, DateTime toDate)
         {
            var projectList = new List<string> { "CE", "ConnectALL", "VIEW-Portal" };
            var allValues = new List<Values>();
            string from = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");
            string to = toDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");

            foreach (var project in projectList)
            {
                // Step 1: WIQL Query for each project
                string wiqlUrl = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", project);

                string wiqlQuery = $@"
            SELECT [System.Id], [System.Title], [System.State] 
            FROM WorkItems
            WHERE 
             [System.TeamProject] = '{project}' AND
            [System.ChangedDate] > '{from}'
            AND [System.ChangedDate] < '{to}'
            AND [System.WorkItemType] IN ('Production Defect', 'Bug', 'user story')
            ORDER BY [System.ChangedDate] DESC";

                var queryPayload = new { query = wiqlQuery };
                var content = new StringContent(JsonConvert.SerializeObject(queryPayload), Encoding.UTF8, "application/json");

                var wiqlResponse = _Utility.PostDataSync(wiqlUrl, content);
                var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(wiqlResponse);

                var idList = wiqlData?.workItems?.Select(w => w.id).ToList();
                if (idList != null && idList.Count > 0)
                {
                    // Azure DevOps limits the batch to 200 work items per call
                    const int batchSize = 200;
                    for (int i = 0; i < idList.Count; i += batchSize)
                    {
                        var batchIds = idList.Skip(i).Take(batchSize);
                        string idListCsv = string.Join(",", batchIds);
                        var workItemJson = GetWorkItem(project, idListCsv);
                        var workItemData = JsonConvert.DeserializeObject<WorkItemModel>(workItemJson);

                        if (workItemData?.value != null)
                        {
                            allValues.AddRange(workItemData.value);
                        }
                    }
                }
            }

            return new WorkItemModel
            {
                value = allValues
            };
        }

    }
}
