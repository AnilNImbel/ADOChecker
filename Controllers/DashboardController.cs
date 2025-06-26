
using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using ADOAnalyser.Models.TestModel;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ADOAnalyser.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IWorkItem _workItem;
        private readonly AutoSpotCheck _autoSpotCheck;

        private const string TestRelation = "Microsoft.VSTS.Common.TestedBy-Forward";
        private const string ProjectName = "CE";

        public DashboardController(IWorkItem workItem, AutoSpotCheck autoSpotCheck)
        {
            _workItem = workItem;
            _autoSpotCheck = autoSpotCheck;
        }

        public IActionResult Index(string? selectedSprint = null)
        {
            var workItemTypes = new List<string> { "Bug", "User Story", "Production Defect" };
            const string filter = @"AND [System.State] <> 'New' AND [System.State] <> 'Approved'";

            var iterationData = _workItem.GetSprint(ProjectName);
            var allSprints = iterationData.AllSprints;
            var currentSprints = iterationData.CurrentSprints;

            var sprintToUse = selectedSprint ?? currentSprints.FirstOrDefault()?.FullPath;
            var workData = new WorkItemModel();

            if (!string.IsNullOrEmpty(sprintToUse))
            {
                var wiqlJson = _workItem.GetAllWiqlByType(ProjectName, workItemTypes, sprintToUse, filter);
                var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(wiqlJson);

                var idList = wiqlData?.workItems?.Select(w => w.id).ToList();
                if (idList?.Any() == true)
                {
                    var workItemsJson = _workItem.GetWorkItem(ProjectName, string.Join(", ", idList.Take(200)));
                    workData = JsonConvert.DeserializeObject<WorkItemModel>(workItemsJson);

                    if (workData?.value?.Any() == true)
                    {
                        workData.value = workData.value
                        .Where(w => string.IsNullOrEmpty(w.fields.CivicaAgileReproducible) ||
                        w.fields.CivicaAgileReproducible.Equals("YES", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                        if (workData.value.Count > 0)
                        {
                            AddTestRelationFilterData(workData);
                            _autoSpotCheck.CheckMissingData(workData);
                            _autoSpotCheck.SetCountForMissing(workData);
                        }
                    }
                }
            }

            var viewModel = new SprintViewModel
            {
                SelectedSprint = sprintToUse,
                AllSprints = allSprints.Select(s => s.FullPath).ToList(),
                WorkItemData = workData
            };

            HttpContext.Session.SetString("workItemModel", JsonConvert.SerializeObject(workData));
            return View(viewModel);
        }

        public IActionResult GridLoad(string missingType)
        {
            var workItemJson = HttpContext.Session.GetString("workItemModel");
            var workItem = JsonConvert.DeserializeObject<WorkItemModel>(workItemJson ?? string.Empty);
            var filteredData = new WorkItemModel();

            if (workItem?.value != null)
            {
                filteredData.value = missingType switch
                {
                    "IAStatus" => workItem.value.Where(w => w.fields.IAStatus == ResultEnum.Missing.ToString()).ToList(),
                    "RootCauseStatus" => workItem.value.Where(w => w.fields.RootCauseStatus == ResultEnum.Missing.ToString()).ToList(),
                    "ProjectZeroStatus" => workItem.value.Where(w => w.fields.ProjectZeroStatus == ResultEnum.Missing.ToString()).ToList(),
                    "PRLifeCycleStatus" => workItem.value.Where(w => w.fields.PRLifeCycleStatus == ResultEnum.Missing.ToString()).ToList(),
                    "StatusDiscrepancyStatus" => workItem.value.Where(w => w.fields.StatusDiscrepancyStatus == ResultEnum.Yes.ToString()).ToList(),
                    "TestCaseGapeStatus" => workItem.value.Where(w => w.fields.TestCaseGapeStatus == ResultEnum.Missing.ToString()).ToList(),
                    "All" => workItem.value.ToList(),
                    _ => new List<Values>()
                };
            }

            return PartialView("_WorkItemGrid", filteredData);
        }

        private void AddTestRelationFilterData(WorkItemModel workData)
        {
            foreach (var item in workData.value)
            {
                var relationIds = item.relations?
                .Where(r => r.rel == TestRelation)
                .Select(r => int.Parse(r.url.Split('/').Last()))
                .ToList();

                if (relationIds?.Any() == true)
                {
                    var testItemsJson = _workItem.GetWorkItem(string.Join(", ", relationIds.Take(200)));
                    var testData = JsonConvert.DeserializeObject<TestedByModel>(testItemsJson);

                    if (testData?.value?.Any() == true)
                    {
                        item.testByRelationField = testData.value
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
            }
        }
    }
}
