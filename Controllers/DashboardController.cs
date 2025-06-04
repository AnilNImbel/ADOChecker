using ADOAnalyser.Common;
using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ADOAnalyser.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IWorkItem _workItem;

        private readonly AutoSpotCheck autoSpotCheck;

        public DashboardController(IWorkItem workItem, AutoSpotCheck autoSpotChecks)
        {
            _workItem = workItem;
            autoSpotCheck = autoSpotChecks;
        }

        public IActionResult Index(string? selectedSprint = null)
        {
            List<string> workItemType = new List<string> { "Bug", "User Story", "Production Defect" };
            string project = "CE";
            TempData["workItemModel"] = null;
            // Get sprint info (current + all)
            var iterationData = _workItem.GetSprint(project); // your GetSprint returns IterationResult
            var allSprints = iterationData.AllSprints;
            var currentSprints = iterationData.CurrentSprints;

            // Choose sprint: dropdown value or fallback to current sprint
            string sprintToUse = selectedSprint ?? currentSprints.FirstOrDefault()?.FullPath;

            var workData = new WorkItemModel();

            if (!string.IsNullOrEmpty(sprintToUse))
            {
                var getWiql = _workItem.GetAllWiqlByType(project, workItemType, sprintToUse);
                var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(getWiql);
                var idList = wiqlData.workItems.Select(a => a.id).ToList();

                if (idList != null && idList.Count > 0)
                {
                    var getWorkItems = _workItem.GetWorkItem(project, string.Join(", ", idList.Take(500)));
                    workData = JsonConvert.DeserializeObject<WorkItemModel>(getWorkItems);
                    if (workData != null)
                    {
                        CheckMissingData(workData);
                        SetCountForMissing(workData);
                    }
                }
            }

            // Create a view model to send sprint info + work items to partial
            var viewModel = new SprintViewModel
            {
                SelectedSprint = sprintToUse,
                AllSprints = allSprints.Select(i => i.FullPath).ToList(),
                WorkItemData = workData
            };
            HttpContext.Session.SetString("workItemModel", JsonConvert.SerializeObject(workData));
            return View(viewModel);
        }

        public IActionResult GridLoad(string missingType)
        {
            //var filteredData = workItem.value.Where(a => a.fields.GetType().GetProperty(missingType).GetValue(a.fields).ToString().Equals(ResultEnum.Missing.ToString())).ToList();
            WorkItemModel data = new WorkItemModel();
            WorkItemModel workItem = JsonConvert.DeserializeObject<WorkItemModel>((string)HttpContext.Session.GetString("workItemModel"));
            if (workItem != null && workItem.value != null)
            {
                if (missingType.Equals("IAStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.IAStatus.Equals(ResultEnum.Missing.ToString())).ToList();
                }
                else if (missingType.Equals("RootCauseStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.RootCauseStatus.Equals(ResultEnum.Missing.ToString())).ToList();
                }
                if (missingType.Equals("ProjectZeroStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.ProjectZeroStatus.Equals(ResultEnum.Missing.ToString())).ToList();
                }
                if (missingType.Equals("PRLifeCycleStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.PRLifeCycleStatus.Equals(ResultEnum.Missing.ToString())).ToList();
                }
                if (missingType.Equals("StatusDiscrepancyStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.StatusDiscrepancyStatus.Equals(ResultEnum.Yes.ToString())).ToList();
                }
                if (missingType.Equals("TestCaseGapeStatus"))
                {
                    data.value = workItem.value.Where(a => a.fields.TestCaseGapeStatus.Equals(ResultEnum.Missing.ToString())).ToList();
                }
            }
            return PartialView("_WorkItemGrid", data);
            // Do something with the product
        }

        private void CheckMissingData(WorkItemModel workData)
        {
            for (int i = 0; i < workData.value.Count; i++)
            {
                autoSpotCheck.CheckImpactAssessment(workData.value[i].fields);
                autoSpotCheck.CheckRootCause(workData.value[i].fields);
                autoSpotCheck.CheckProjectZero(workData.value[i].fields);
                autoSpotCheck.CheckPRLifeCycle(workData.value[i].fields);
                autoSpotCheck.CheckStatusDiscre(workData.value[i].fields);
                autoSpotCheck.CheckTestCaseGape(workData.value[i].fields);
                autoSpotCheck.CheckVTDRequired(workData.value[i].fields);
                autoSpotCheck.CheckVLDBRequired(workData.value[i].fields);
            }
        }

        private void SetCountForMissing(WorkItemModel workData)
        {
            workData.missingIACount = autoSpotCheck.MissingImpactAssessmentCount(workData);
            workData.missingRootCauseCount = autoSpotCheck.MissingRootCauseCount(workData);
            workData.missingProjectZeroCount = autoSpotCheck.MissingProjectZeroCount(workData);
            workData.missingPRLifeCycleCount = autoSpotCheck.MissingPRLifeCycleCount(workData);
            workData.missingStatusDiscreCount = autoSpotCheck.MissingStatusDiscreCount(workData);
            workData.missingTestCaseCount = autoSpotCheck.MissingTestCaseGapeCount(workData);
            workData.missingVTDCount = autoSpotCheck.MissingVTDCount(workData);
            workData.missingVLDBCount = autoSpotCheck.MissingVLDBCount(workData);
        }
    }
}
