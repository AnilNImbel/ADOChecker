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

        public WorkTypeController(IWorkItem workItem)
        {
            _workItem = workItem;
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
            var getWiql = _workItem.GetAllWiqlByType(project ,workType);
            var wiqlData = JsonConvert.DeserializeObject<WiqlModel>(getWiql);
            var idList = wiqlData.workItems.Select(a => a.id).ToList();
            var getWorkItems = _workItem.GetWorkItem(project, string.Join(", ", idList.Take(50)));
            var workData = JsonConvert.DeserializeObject<WorkItemModel>(getWorkItems);
            CheckImpactAssessment(workData);
            TempData["projectType"] = project.ToLower();
            return PartialView("_WorkItemGrid", workData);
        }

        private void CheckImpactAssessment(WorkItemModel workData)
        {
            for (int i = 0; i < workData.value.Count; i++)
            {
                string data = workData.value[i].fields.MicrosoftVSTSCMMIImpactAssessmentHtml;
                if (string.IsNullOrWhiteSpace(data))
                {
                    workData.value[i].fields.MicrosoftVSTSCMMIImpactAssessmentHtml = "Missing";
                }
                else
                {
                    workData.value[i].fields.MicrosoftVSTSCMMIImpactAssessmentHtml = ImpactAssessmentRegex(data) ? "Attached" : "Missing";
                }
            }
        }

        private bool ImpactAssessmentRegex(string data)
        {
            string pattern = @"https?://[^""']*Assessment[^""']*\.xlsx";
            return Regex.IsMatch(data, pattern, RegexOptions.IgnoreCase);
        }
    }
}
