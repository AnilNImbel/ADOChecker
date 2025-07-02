using ADOAnalyser.Enum;
using ADOAnalyser.Models.TestModel;
using ADOAnalyser.Models;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ADOAnalyser.Controllers
{
    public class AdvanceSearchController : Controller
    {
        private readonly IWorkItem _workItem;
        private readonly ADORules _autoSpotCheck;

        private const string TestRelation = "Microsoft.VSTS.Common.TestedBy-Forward";
        private const string ProjectName = "CE";

        public AdvanceSearchController(IWorkItem workItem, ADORules autoSpotCheck)
        {
            _workItem = workItem;
            _autoSpotCheck = autoSpotCheck;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Search(string workItemType, int? adoNumber, string assignedTo, string state)
        {
            var workItemTypes = new List<string> { "Bug", "User Story", "Production Defect" };
            //var systemState = new List<string> { "Closed", "Resolved", "Test", "Active"};
            string filter = string.Empty;
            string systemState = @" AND [System.State] <> 'New' AND [System.State] <> 'Approved'";

            bool isDefaultSearch =
            (string.IsNullOrEmpty(workItemType) || workItemType == "All") &&
            !adoNumber.HasValue &&
            string.IsNullOrEmpty(assignedTo) &&
            (string.IsNullOrEmpty(state) || state == "All");

            if (isDefaultSearch)
            {
                return Content("Please select at least one filter to perform a search.");
            }

            if (workItemType != "All")
            {
                workItemTypes = workItemTypes.Where(a => a.Equals(workItemType)).ToList();
            }

            if (state != "All")
            {
                systemState += $" AND [System.State] = '{state}'";
            }


            if(adoNumber != null)
            {
                filter = $" AND [System.Id] = '{adoNumber}'";
            }
            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                filter += $" AND [System.AssignedTo] CONTAINS '{assignedTo}'";
            }

            var workData = new WorkItemModel();

            var wiqlJson = _workItem.GetAllWiqlSearch(ProjectName, workItemTypes, filter += systemState);
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

            return PartialView("_WorkItemGrid", workData);
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
                    var testData = JsonConvert.DeserializeObject<TestCaseModel>(testItemsJson);

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
