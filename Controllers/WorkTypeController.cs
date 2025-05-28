using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

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
            TempData["projectType"] = project.ToLower();
            return PartialView("_WorkItemGrid", workData);
        }

    }
}
