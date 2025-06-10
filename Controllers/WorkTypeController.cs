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
            List<string> workItemType = new List<string> { workType };
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
                    if (workData?.value?.Any() == true)
                    {
                        workData.value = workData.value
                                             .Where(a => string.IsNullOrEmpty(a.fields.CivicaAgileReproducible) ||
                                              a.fields.CivicaAgileReproducible.Equals("YES", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (workData.value.Count > 0)
                        {
                            autoSpotCheck.CheckMissingData(workData);
                            autoSpotCheck.SetCountForMissing(workData);
                        }
                    }
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
            return PartialView("_WorkItemGridApplication", viewModel);
        }
    }
}
