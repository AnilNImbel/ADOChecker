using ADOAnalyser.Common;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace ADOAnalyser.Controllers
{
    public class WorkTypeController : Controller
    {
        private readonly IWorkItem _workItem;

        private readonly AutoSpotCheck autoSpotCheck;
        public WorkTypeController(IWorkItem workItem, AutoSpotCheck autoSpotChecks)
        {
            _workItem = workItem;
            autoSpotCheck = autoSpotChecks;
        }

        public IActionResult Index(string project)
        {
            TempData["projectType"] = project.ToLower();
            List<WorkTypeModel> workTypeModel = new List<WorkTypeModel>
            {
                new WorkTypeModel { project = project, workType = "User Story" , icon = "fas fa-book-open"},
                new WorkTypeModel { project = project, workType = "Bug", icon = "fas fa-bug" },
                new WorkTypeModel { project = project, workType = "Production Defect" , icon= "fas fa-exclamation-triangle"}
            };
            return View(workTypeModel);
        }

        public IActionResult LoadPartial(string workType, string? selectedSprint = null)
        {
            string project = TempData["projectType"]?.ToString() ?? "DefaultProject";

            // Get sprint info (current + all)
            var iterationData = _workItem.GetSprint(project); // your GetSprint returns IterationResult
            var allSprints = iterationData.AllSprints;
            var currentSprints = iterationData.CurrentSprints;

            // Choose sprint: dropdown value or fallback to current sprint
            string sprintToUse = selectedSprint ?? currentSprints.FirstOrDefault()?.FullPath;

            var workData = new WorkItemModel();

            if (!string.IsNullOrEmpty(sprintToUse))
            {
                var getWiql = _workItem.GetAllWiqlByType(project, workType, sprintToUse);
                var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(getWiql);
                var idList = wiqlData.workItems.Select(a => a.id).ToList();

                if (idList != null && idList.Count > 0)
                {
                    var getWorkItems = _workItem.GetWorkItem(project, string.Join(", ", idList.Take(500)));
                    workData = JsonConvert.DeserializeObject<WorkItemModel>(getWorkItems);
                    CheckMissingData(workData);
                }
            }

            TempData["projectType"] = project.ToLower();

            // Create a view model to send sprint info + work items to partial
            var viewModel = new SprintViewModel
            {
                SelectedSprint = sprintToUse,
                AllSprints = allSprints.Select(i => i.FullPath).ToList(),
                WorkItemData = workData
            };

            TempData["worktype"] = workType;
            return PartialView("_WorkItemGrid", viewModel);
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
