using ADOAnalyser.Extensions;
using ADOAnalyser.Models;
using ADOAnalyser.Models.TestModel;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
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

        public string GetPipelines(string project, string folder)
        {
            string Url = string.Format("{0}/_apis/pipelines?folderPath={1}&api-version=7.0", project, folder);
            return _Utility.GetDataSync(Url);
        }

        public string GetBuilds(string project, int definitionId)
        {
            string Url = string.Format("{0}/_apis/build/builds?definitions={1}&statusFilter={2}&api-version=7.1-preview.7", project, definitionId, "all");
            return _Utility.GetDataSync(Url);
        }

        public async Task<string> GetPipelinesAsync(string project, string folder)
        {
            string Url = string.Format("{0}/_apis/pipelines?folderPath={1}&api-version=7.0", project, folder);
            return await _Utility.GetDataAsync(Url);
        }

        public async Task<string> GetBuildsAsync(string project, int definitionId)
        {
            string Url = string.Format("{0}/_apis/build/builds?definitions={1}&statusFilter={2}&api-version=7.1-preview.7", project, definitionId, "all");
            return await _Utility.GetDataAsync(Url);
        }

        private string BuildWorkItemUrl(string? projectName, string ids)
        {
            return string.IsNullOrEmpty(projectName)
            ? $"/_apis/wit/workitems?ids={ids}&$expand=relations&api-version=7.1-preview.2"
            : $"{projectName}/_apis/wit/workitems?ids={ids}&$expand=relations&api-version=7.1-preview.2";
        }

        public IterationResult GetSprint(string projectName)
        {
            return _Utility.GetCurrentIterationAsync(projectName);
        }

        public string GetWorkItem(string? projectName, string ids) => _Utility.GetDataSync(BuildWorkItemUrl(projectName, ids));
        
        public string GetWorkItem(string ids) => GetWorkItem(null, ids);
        
        public Task<string> GetWorkItemAsync(string projectName, string ids) => Task.FromResult(GetWorkItem(projectName, ids));
        
        public Task<string> GetWorkItemAsync(string ids) => Task.FromResult(GetWorkItem(null, ids));

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

        public async Task<WorkItemModel> GetAllWiqlSearchAsync(string projectName, List<string> workItemType, string filter = "")
        {
            var model = new WorkItemModel { value = new List<Values>() };
            string types = string.Join(",", workItemType.Select(t => $"'{t}'"));
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);

            var query = new
            {
                query = $@"SELECT [System.Id]
                         FROM WorkItems
                        Where 
                        [System.WorkItemType] IN ({types})
                        AND [System.TeamProject] = '{projectName}'
                        {filter}
                        ORDER BY [System.CreatedDate] desc"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            var wiqlJson = _Utility.PostDataSync(Url, content);
            var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(wiqlJson);

            var parentIds = wiqlData?.workItems?.Select(w => w.id).ToList();
            var localValues = new ConcurrentBag<Values>();

            if (parentIds?.Any() == true)
            {
                var tasks = parentIds.Chunk(200).Select(async batch =>
                {
                    var workItemsJson = await GetWorkItemAsync(projectName, string.Join(",", batch));
                    var parentItemsModel = JsonConvert.DeserializeObject<WorkItemModel>(workItemsJson);

                    if (parentItemsModel?.value != null)
                    {
                        var filtered = parentItemsModel.value
                         .Where(a => string.IsNullOrEmpty(a.fields.CivicaAgileReproducible) ||
                         a.fields.CivicaAgileReproducible.Equals("YES", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (parentItemsModel.value.Count > 0)
                        {
                            Parallel.ForEach(filtered, item =>
                            {
                                localValues.Add(item);                            
                            });
                        }
                    }
                    // Add test relation data
                    var localList = localValues.ToList();

                    await AddTestRelationFilterDataAsync(new WorkItemModel { value = localList });

                    // Add to main model
                    lock (model.value)
                    {
                        model.value.AddRange(localList);
                    }
                });
                await Task.WhenAll(tasks);
            }
            return new WorkItemModel
            {
                value = model.value,
                count = model.value.Count
            };
        }

        public async Task<WorkItemModel> GetAllWorkItemsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            DateTime toDateTime = toDate.Date.AddDays(1);

            var model = new WorkItemModel { value = new List<Values>() };
            var projectList = new List<string> { "CE", "ConnectALL", "VIEW-Portal" };
            string from = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000");
            string to = toDateTime.ToString("yyyy-MM-ddTHH:mm:ss.0000000");

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
                            [System.ChangedDate] >= '{from}' AND 
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
                            var testData = JsonConvert.DeserializeObject<TestCaseModel>(getWorkItems);

                            if (testData?.value?.Any() == true)
                            {
                                workItem.testByRelationField = testData.value
                                     .Where(v => v.fields != null)
                                     .Select(v => new TestByRelationField
                                     {
                                         TestId = v.id,
                                         SystemState = v.fields.SystemState,
                                         SystemAssignedTo = v.fields.SystemAssignedTo,
                                         CustomAutomation = v.fields.MicrosoftVSTSTCMAutomationStatus,
                                         CivicaAgileTestLevel = v.fields.CivicaAgileTestLevel,
                                         CivicaAgileTestPhase = v.fields.CivicaAgileTestPhase,
                                         CustomTestType = v.fields.CustomTestType
                                     }).ToList();
                            }
                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
