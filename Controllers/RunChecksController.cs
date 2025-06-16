using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using ADOAnalyser.Repository;
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
        public async Task<IActionResult> IndexAsync(DateTime fromDate, DateTime toDate)
        {
            //var workItemModel = _workItem.GetAllWorkItemsByDateRange(fromDate, toDate);
            var workItemModel = await Task.Run(() => _workItem.GetAllWorkItemsByDateRangeAsync(fromDate, toDate));

            if (workItemModel?.value != null)
            {
                await autoSpotCheck.CheckMissingDataAsync(workItemModel);
                workItemModel.passingCount = autoSpotCheck.TotalFullyPassedCount(workItemModel);
            }
            ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.ToString("yyyy-MM-dd");
            if (workItemModel?.value != null && workItemModel.value.Any())
            {
               await SaveTestRunResultAsync(fromDate, toDate, workItemModel);
            }
            return View(workItemModel);
        }

        private async Task SaveTestRunResultAsync(DateTime fromDate, DateTime toDate, WorkItemModel workItemModel)
        {
            int totalCount = workItemModel.value?.Count ?? 0;
            int missingCount = totalCount - workItemModel.passingCount;

            var runResult = new TestRunResult
            {
                RunDate = DateTime.Now,
                StartDate = fromDate,
                EndDate = toDate,
                ResultSummary = $"Run completed with {totalCount} work items.{workItemModel.passingCount} Passed, {missingCount} Failed."
            };

            await _dbContext.TestRunResults.AddAsync(runResult);
            await _dbContext.SaveChangesAsync(); // Generate RunId

            if (workItemModel?.value != null && workItemModel.value.Any() && runResult.RunId != 0)
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
                    TestCaseGap = string.Empty,
                    CurrentStatus = w.fields?.SystemState,
                    TechnicalLeadName = w.TlPrReviewAssignedTo,
                    DevName = string.Empty,
                    WorkitemType = w.fields?.SystemWorkItemType
                });

                await _dbContext.TestRunDetails.AddRangeAsync(detailRecords);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
