using ADOAnalyser.Models;
using ADOAnalyser.Models.TestModel;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace ADOAnalyser.Controllers
{
    public class TestedByRelationGrid : Controller
    {
        private readonly IWorkItem _workItem;

        private readonly AutoSpotCheck autoSpotCheck;

        private string testRelation = "Microsoft.VSTS.Common.TestedBy-Forward";


        public TestedByRelationGrid(IWorkItem workItem, AutoSpotCheck autoSpotChecks)
        {
            _workItem = workItem;
            autoSpotCheck = autoSpotChecks;
        }

        public IActionResult Index(string workItemNumber)
        {
            var values = new List<Values>();
            if (!string.IsNullOrEmpty(workItemNumber))
            {
                var getWorkItems = _workItem.GetWorkItem(workItemNumber);
                var workItem = JsonConvert.DeserializeObject<WorkItemModel>(getWorkItems);
                if (workItem?.value?.Any() == true && workItem.value.Count > 0)
                {
                    AddTestRelationFilterData(workItem);
                    foreach (var items in workItem.value)
                    {
                        autoSpotCheck.CheckTestCaseGape(items);
                    }
                    values.AddRange(workItem.value);
                }
            }
            return View(values.FirstOrDefault());
        }

        private void AddTestRelationFilterData(WorkItemModel workData)
        {
            for (int i = 0; i < workData.value.Count; i++)
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
                    var getWorkItems = _workItem.GetWorkItem(string.Join(", ", relationIds.Take(200)));
                    var testData = JsonConvert.DeserializeObject<TestedByModel>(getWorkItems);
                    if (testData?.value?.Any() == true)
                    {
                        workData.value[i].testByRelationField = testData.value
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
