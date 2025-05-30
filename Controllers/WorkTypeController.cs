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

        public IActionResult LoadPartial(string workType)
        {
            string project = TempData["projectType"].ToString();
            var iteration = _workItem.GetSprint(project);

            if (iteration != null && iteration.Count > 0)
            {
                var getWiql = _workItem.GetAllWiqlByType(project, workType, iteration);
                var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(getWiql);
                var idList = wiqlData.workItems.Select(a => a.id).ToList();
                if(idList != null && idList.Count > 0)
                {

                    var getWorkItems = _workItem.GetWorkItem(project, string.Join(", ", idList.Take(500)));
                    var workData = JsonConvert.DeserializeObject<WorkItemModel>(getWorkItems);
                    CheckMissingData(workData);
                    //SetCountForMissing(workData);
                    TempData["projectType"] = project.ToLower();
                    return PartialView("_WorkItemGrid", workData);
                }
                else
                {
                    return PartialView("_WorkItemGrid", new WorkItemModel());
                }
            }
            else
            {
                return PartialView("_WorkItemGrid", new WorkItemModel());
            }
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
