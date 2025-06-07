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

        public string GetWorkItemForReports(string projectName, string ids)
        {
            string Url = string.Format("{0}/_apis/wit/workitems?ids={1}&$expand=relations&api-version=7.1-preview.2", projectName, ids);
            return _Utility.GetDataSync(Url);
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
                WHERE [System.WorkItemType] IN ('Production Defect', 'Bug', 'user story')
                  AND [System.AssignedTo] = ''"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            var result = _Utility.PostDataSync(Url, content);
            return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }
        public string GetAllWiqlByType(string projectName, List<string> workItemType, string iterationPath)
        {

            string types = string.Join(",", workItemType.Select(t => $"'{t}'"));
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);
            var iterationFilter = string.Join(" OR ", iterationPath.Select(i =>
       $"[System.IterationPath] = '{iterationPath.Replace("'", "''")}'"));

            var query = new
            {
                query = $@"Select [System.Id], [System.Title], [System.State] 
                     From WorkItems
                     Where [System.WorkItemType] IN ({types})
                        AND [System.TeamProject] = '{projectName}'
                        AND ({iterationFilter})
                     order by [System.CreatedDate] desc"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            return _Utility.PostDataSync(Url, content);
        }

        public WorkItemModel GetAllWorkItemsByDateRange(DateTime fromDate, DateTime toDate)
        {
            WorkItemModel model = new WorkItemModel();
            model.value = new List<Values>();
            var projectList = new List<string> { "CE", "ConnectALL", "VIEW-Portal" };

            string from = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");
            string to = toDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");

            foreach (var project in projectList)
            {
                // 1. WIQL query for parent work items (Production Defect, Bug, User Story)
                string wiqlQuery = $@"
            SELECT [System.Id] 
            FROM WorkItems
            WHERE 
                [System.TeamProject] = '{project}' AND
                [System.ChangedDate] > '{from}' AND 
                [System.ChangedDate] < '{to}' AND 
                [System.WorkItemType] IN ('Production Defect', 'Bug', 'User Story')
            ORDER BY [System.ChangedDate] DESC";

                var wiqlContent = new StringContent(JsonConvert.SerializeObject(new { query = wiqlQuery }), Encoding.UTF8, "application/json");
                string wiqlUrl = $"{project}/_apis/wit/wiql?api-version=7.1-preview.2";
                var wiqlResponse = _Utility.PostDataSync(wiqlUrl, wiqlContent);
                var wiqlResult = JsonConvert.DeserializeObject<WiqlModel>(wiqlResponse);

                if (wiqlResult?.workItems == null || !wiqlResult.workItems.Any())
                    return model;

                var parentIds = wiqlResult.workItems.Select(w => w.id).ToList();
                List<int> allChildIds = new List<int>();

                // 2. Fetch parent items in batches of 200
                const int batchSize = 200;
                for (int i = 0; i < parentIds.Count; i += batchSize)
                {
                    var batchIds = parentIds.Skip(i).Take(batchSize);
                    var idListCsv = string.Join(",", batchIds);

                    string parentUrl = $"{project}/_apis/wit/workitems?ids={idListCsv}&$expand=relations&api-version=7.1-preview.2";

                    string parentJson = _Utility.GetDataSync(parentUrl);
                    WorkItemModel parentItemsModel = JsonConvert.DeserializeObject<WorkItemModel>(parentJson);

                    if (parentItemsModel?.value != null)
                    {
                        model.value.AddRange(parentItemsModel.value);

                        // Extract child item IDs from relations
                        foreach (var parentItem in parentItemsModel.value)
                        {
                            if (parentItem.relations == null) continue;

                            foreach (var relation in parentItem.relations)
                            {
                                if (relation.rel == "System.LinkTypes.Hierarchy-Forward")
                                {
                                    var idPart = relation.url.Split('/').Last();
                                    if (int.TryParse(idPart, out int childId))
                                    {
                                        allChildIds.Add(childId);
                                    }
                                }
                            }
                        }
                    }
                }

                // 3. Fetch child items in batches and filter TL PR Review
                for (int i = 0; i < allChildIds.Count; i += batchSize)
                {
                    var childBatch = allChildIds.Skip(i).Take(batchSize);
                    string childUrl = $"{project}/_apis/wit/workitems?ids={string.Join(",", childBatch)}&$expand=relations,fields&api-version=7.1-preview.2";

                    string childJson = _Utility.GetDataSync(childUrl);
                    WorkItemModel childItems = JsonConvert.DeserializeObject<WorkItemModel>(childJson);

                    if (childItems?.value != null)
                    {
                        var tlReviewItems = childItems.value
                            .Where(wi => wi.fields?.SystemTitle?.Equals("TL PR Review", StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();

                        foreach (var tlReviewItem in tlReviewItems)
                        {
                            var assignedToObj = string.Empty;
                            // Extract Assigned To name
                            if (tlReviewItem.fields != null)
                            {
                                assignedToObj = tlReviewItem.fields?.SystemAssignedTo;
                            }

                            // Find the parent item for this TL PR Review child
                            var parentItem = model.value.FirstOrDefault(parent =>
                                parent.relations != null &&
                                parent.relations.Any(r =>
                                    r.rel == "System.LinkTypes.Hierarchy-Forward" &&
                                    r.url.EndsWith($"/{tlReviewItem.id}")));

                            if (parentItem != null)
                            {
                                parentItem.TlPrReviewAssignedTo = assignedToObj;
                            }
                        }

                        // Optionally, add TL PR Review items themselves to model.value if needed:
                        // model.value.AddRange(tlReviewItems);
                    }
                }
            }

            return new WorkItemModel
            {
                value = model.value,
                count = model.value.Count
            };
        }


        public WorkItemModel GetAllWorkItems(DateTime fromDate, DateTime toDate)
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
