using ADOAnalyser.Models;
using ADOAnalyser.TestModel;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static ADOAnalyser.Utility;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ADOAnalyser
{

    public class WorkItem : IWorkItem
    {
        private readonly IUtility _Utility;

        private string testRelation = "Microsoft.VSTS.Common.TestedBy-Forward";

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
            string Url = string.Format("{0}/_apis/wit/workitems?ids={1}&$expand=relations&api-version=7.1-preview.2", projectName, ids);
            return _Utility.GetDataSync(Url);
        }
        public string GetWorkItem(string ids)
        {
            string Url = string.Format("/_apis/wit/workitems?ids={0}&$expand=relations&api-version=7.1-preview.2", ids);
            return _Utility.GetDataSync(Url);
        }

        public async Task<string> GetWorkItemAsync(string projectName, string ids)
        {
            string Url = string.Format("{0}/_apis/wit/workitems?ids={1}&$expand=relations&api-version=7.1-preview.2", projectName, ids);
            return _Utility.GetDataSync(Url);
        }

        public async Task<string> GetWorkItemAsync(string ids)
        {
            string Url = string.Format("/_apis/wit/workitems?ids={0}&$expand=relations&api-version=7.1-preview.2", ids);
            return _Utility.GetDataSync(Url);
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
        }

        public string GetAllWiqlByType(string projectName, List<string> workItemType, string iterationPath, string filter = "")
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
                        {filter}
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
            int count = 0;
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
                        [System.WorkItemType] IN ('Production Defect', 'Bug', 'User Story') AND
                        [System.State] <> 'New'
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
                        parentItemsModel.value = parentItemsModel.value
                                                    .Where(a =>
                                                    (a.fields.CivicaAgileReproducible == null ||
                                                    a.fields.CivicaAgileReproducible.Equals("YES", StringComparison.CurrentCultureIgnoreCase))
                                                    )
                                                    .ToList();

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
                        count = count == 0 ? 0 : count;
                        AddTestRelationFilterData(model, count);
                        count = model.value.Count;
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

        private void AddTestRelationFilterData(WorkItemModel workData, int pickIndex)
        {
            //int batchSize = 200;
            int start = pickIndex;
            //count = start + batchSize;
            int count = workData.value.Count;
            for (int i = start; i < count; i++)
            {
                var relationIds = workData.value[i].relations?
                                  .Where(r => r.rel == testRelation)
                                  .Select(r =>
                                  {
                                      var segments = r.url.Split('/');
                                      return int.Parse(segments.Last());
                                  })
                                  .ToList();

                if (relationIds?.Any() == true)
                {
                    var getWorkItems = GetWorkItem(string.Join(", ", relationIds.Take(200)));
                    var testData = JsonConvert.DeserializeObject<TestedByModel>(getWorkItems);
                    if (testData?.value?.Any() == true)
                    {
                        workData.value[i].testByRelationField = testData.value
                                             .Select(v => v.fields)
                                             .Where(f => f != null)
                                             .Select(f => new TestByRelationField
                                             {
                                                 MicrosoftVSTSTCMAutomationStatus = f.MicrosoftVSTSTCMAutomationStatus,
                                                 CivicaAgileTestLevel = f.CivicaAgileTestLevel,
                                                 CivicaAgileTestPhase = f.CivicaAgileTestPhase,
                                                 CustomTestType = f.CustomTestType
                                             }).ToList();
                    }
                }
            }
        }

        public async Task<WorkItemModel> GetAllWorkItemsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var model = new WorkItemModel { value = new List<Values>() };
            var projectList = new List<string> { "CE", "ConnectALL", "VIEW-Portal" };
            string from = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");
            string to = toDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(new TimeSpan(24, 0, 9));
            var maxDegreeOfParallelism = 8;

            var tasks = projectList.ForEachAsyncConcurrent(async project =>
            {
                string wiqlQuery = $@"
                        SELECT [System.Id] 
                        FROM WorkItems
                        WHERE 
                            [System.TeamProject] = '{project}' AND
                            [System.ChangedDate] > '{from}' AND 
                            [System.ChangedDate] < '{to}' AND 
                            [System.WorkItemType] IN ('Production Defect', 'Bug', 'User Story') AND
                            [System.State] <> 'New' AND
                            [System.State] <> 'Approved'
                        ORDER BY [System.ChangedDate] DESC";

                var wiqlContent = new StringContent(JsonConvert.SerializeObject(new { query = wiqlQuery }), Encoding.UTF8, "application/json");
                string wiqlUrl = $"{project}/_apis/wit/wiql?api-version=7.1-preview.2";
                var wiqlResponse = await _Utility.PostDataAsync(wiqlUrl, wiqlContent);
                var wiqlResult = JsonConvert.DeserializeObject<WiqlModel>(wiqlResponse);

                if (wiqlResult?.workItems == null || !wiqlResult.workItems.Any())
                    return;

                var parentIds = wiqlResult.workItems.Select(w => w.id).ToList();
                var allChildIds = new ConcurrentBag<int>();
                var localValues = new ConcurrentBag<Values>();

                var parentTasks = parentIds.Chunk(200).Select(async batch =>
                {
                    var idListCsv = string.Join(",", batch);
                    string parentUrl = $"{project}/_apis/wit/workitems?ids={idListCsv}&$expand=relations&api-version=7.1-preview.2";
                    string parentJson = await _Utility.GetDataAsync(parentUrl);
                    var parentItemsModel = JsonConvert.DeserializeObject<WorkItemModel>(parentJson);

                    if (parentItemsModel?.value != null)
                    {
                        var filtered = parentItemsModel.value
                         .Where(a => string.IsNullOrEmpty(a.fields.CivicaAgileReproducible) ||
                         a.fields.CivicaAgileReproducible.Equals("YES", StringComparison.OrdinalIgnoreCase)).ToList();

                        Parallel.ForEach(filtered, item =>
                            {
                                localValues.Add(item);

                                if (item.relations != null)
                                {
                                    foreach (var relation in item.relations)
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
                            });
                    }
                });

                await Task.WhenAll(parentTasks);

                // Add test relation data
                var localList = localValues.ToList();

                await AddTestRelationFilterDataAsync(new WorkItemModel { value = localList });

                // Add to main model
                lock (model.value)
                {
                    model.value.AddRange(localList);
                }

                // Fetch child items
                var childTasks = allChildIds.Distinct().Chunk(200).Select(async batch =>
                {
                    string childUrl = $"{project}/_apis/wit/workitems?ids={string.Join(",", batch)}&$expand=relations,fields&api-version=7.1-preview.2";
                    string childJson = await _Utility.GetDataAsync(childUrl);
                    var childItems = JsonConvert.DeserializeObject<WorkItemModel>(childJson);


                    if (childItems?.value != null)
                    {
                        var tlReviewItems = childItems.value
                        .Where(wi => string.Equals(wi.fields?.SystemTitle, "TL PR Review", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                        foreach (var tlReviewItem in tlReviewItems)
                        {
                            var assignedTo = tlReviewItem.fields?.SystemAssignedTo;

                            var parentItem = model.value.FirstOrDefault(parent =>
                            parent.relations?.Any(r =>
                            r.rel == "System.LinkTypes.Hierarchy-Forward" &&
                            r.url.EndsWith($"/{tlReviewItem.id}")) == true);

                            if (parentItem != null)
                            {
                                parentItem.TlPrReviewAssignedTo = assignedTo;
                            }
                        }
                    }

                });

                await Task.WhenAll(childTasks);
            },
            cts.Token,
            maxDegreeOfParallelism);

            await Task.WhenAll(tasks);

            return new WorkItemModel
            {
                value = model.value,
                count = model.value.Count
            };
        }

        private async Task AddTestRelationFilterDataAsync(WorkItemModel workData)
        {
            var tasks = new List<Task>();

            foreach (var workItem in workData.value)
            {
                var relationIds = workItem.relations?
                .Where(r => r.rel == testRelation)
                .Select(r => int.Parse(r.url.Split('/').Last()))
                .ToList();

                if (relationIds?.Any() == true)
                {
                    var idsBatch = string.Join(",", relationIds.Take(200));

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var getWorkItems = await GetWorkItemAsync(idsBatch);
                            var testData = JsonConvert.DeserializeObject<TestedByModel>(getWorkItems);

                            if (testData?.value?.Any() == true)
                            {
                                workItem.testByRelationField = testData.value
                                    .Select(v => v.fields)
                                    .Where(f => f != null)
                                    .Select(f => new TestByRelationField
                                    {
                                        MicrosoftVSTSTCMAutomationStatus = f.MicrosoftVSTSTCMAutomationStatus,
                                        CivicaAgileTestLevel = f.CivicaAgileTestLevel,
                                        CivicaAgileTestPhase = f.CivicaAgileTestPhase,
                                        CustomTestType = f.CustomTestType
                                    }).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Optional: log or handle the error
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
