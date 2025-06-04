using ADOAnalyser.Common;
using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ADOAnalyser.Controllers
{
    public class RunChecksController : Controller
    {
        private readonly IWorkItem _workItem;

        private readonly AutoSpotCheck autoSpotCheck;

        private readonly AppDbContext _dbContext;

        public RunChecksController(IWorkItem workItem, AutoSpotCheck autoSpotChecks, AppDbContext dbContext)
        {
            _workItem = workItem;
            autoSpotCheck = autoSpotChecks;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // This will look for Views/RunChecks/Index.cshtml
        }

        [HttpPost]
        public IActionResult Index(DateTime fromDate, DateTime toDate)
        {
            var workItemModel = _workItem.GetAllWorkItemsByDateRange(fromDate, toDate);

            CheckMissingData(workItemModel);
            ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.ToString("yyyy-MM-dd");
            if (workItemModel?.value != null && workItemModel.value.Any())
            {
                SaveTestRunResult(fromDate, toDate, workItemModel);
            }
            return View(workItemModel);
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

        private void SaveTestRunResult(DateTime fromDate, DateTime toDate, WorkItemModel workItemModel)
        {
            var runResult = new TestRunResult
            {
                RunDate = DateTime.Now,
                StartDate = fromDate,
                EndDate = toDate,
                ResultSummary = $"Run completed with {workItemModel.value?.Count ?? 0} work items."
            };

            _dbContext.TestRunResults.Add(runResult);
            _dbContext.SaveChanges(); // Generate RunId

            if (workItemModel?.value != null && workItemModel.value.Any())
            {
                var detailRecords = workItemModel.value.Select(w => new TestRunDetail
                {
                    RunId = runResult.RunId,
                    AdoItemId = w.id.ToString(),
                    CallReference = w.fields?.CivicaAgileCallReference,
                    ImpactAssessment = w.fields?.IAStatus,
                    RootCauseAnalysis = w.fields?.RootCauseStatus,
                    ProjectZero = w.fields?.ProjectZeroStatus,
                    PRLifecycle = w.fields?.PRLifeCycleStatus,
                    StatusDiscrepancy = w.fields?.StatusDiscrepancyStatus,
                    TestCaseGap = string.Empty, // or combine with VLDB if needed
                    CurrentStatus = w.fields?.SystemState,
                    TechnicalLeadName = string.Empty,
                    DevName = string.Empty,
                    WorkitemType = w.fields?.SystemWorkItemType
                });

                _dbContext.TestRunDetails.AddRange(detailRecords);
                _dbContext.SaveChanges();
            }
        }
    }
}
