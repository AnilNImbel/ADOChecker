using ADOAnalyser.Models;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ADOAnalyser.Controllers
{
    public class AdvanceSearchController : Controller
    {
        private readonly IWorkItem _workItem;
        private readonly ADORules _autoSpotCheck;
        private const string ProjectName = "CE";

        public AdvanceSearchController(IWorkItem workItem, ADORules autoSpotCheck)
        {
            _workItem = workItem;
            _autoSpotCheck = autoSpotCheck;
        }

        public IActionResult Index()
        {
            IterationResult iterationData = _workItem.GetSprint(ProjectName);
            var allSprints = iterationData.AllSprints;

            var viewModel = new SprintViewModel
            {
                AllSprints = allSprints.Select(s => new SprintDropdownItem
                {
                    FullPath = s.FullPath,
                    StartDate = s.Attributes.StartDate,
                    EndDate = s.Attributes.FinishDate
                }).ToList(),
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> SearchAsync(string workItemType, int? adoNumber, string assignedTo, string state, string sprint)
        {
            var workItemTypes = new List<string> { "Bug", "User Story", "Production Defect" };
            string systemStateFilter = " AND [System.State] <> 'New' AND [System.State] <> 'Approved'";
            string sprintFilter = string.Empty;
            string additionalFilter = string.Empty;

            // Sprint filter
            if (sprint == "All")
            {
                var iterationData = _workItem.GetSprint(ProjectName);
                var allSprints = iterationData.AllSprints;

                var sprintConditions = allSprints
                    .Select(i => $"[System.IterationPath] = '{i.FullPath.Replace("'", "''")}'");

                sprintFilter = " AND (" + string.Join(" OR ", sprintConditions) + ")";
            }
            else
            {
                sprintFilter = $" AND [System.IterationPath] = '{sprint}'";
            }

            // Work item type filter
            if (workItemType != "All")
            {
                workItemTypes = workItemTypes
                    .Where(type => type.Equals(workItemType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // State filter
            if (state != "All")
            {
                systemStateFilter += $" AND [System.State] = '{state}'";
            }

            // ADO number filter

            if (adoNumber.HasValue)
            {
                systemStateFilter += $" AND [System.Id] = {adoNumber.Value}";
            }


            // AssignedTo filter
            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                additionalFilter += $" AND [System.AssignedTo] CONTAINS '{assignedTo}'";
            }

            // Final WIQL filter query
            string filterQuery = additionalFilter + systemStateFilter + sprintFilter;

            // Fetch work items
            var workItemModel = await Task.Run(() =>
                _workItem.GetAllWiqlSearchAsync(ProjectName, workItemTypes, filterQuery));

            if (workItemModel?.value != null)
            {
                await _autoSpotCheck.CheckMissingDataAsync(workItemModel);
            }

            // Set view flags
            workItemModel.showCSV = AppSettingsReader.GetValue("AdvanceSearch", "ShowCsv") == string.Empty ? false : Convert.ToBoolean(AppSettingsReader.GetValue("AdvanceSearch", "ShowCsv"));
            workItemModel.showTotalCount = AppSettingsReader.GetValue("AdvanceSearch", "ShowCount") == string.Empty ? false : Convert.ToBoolean(AppSettingsReader.GetValue("AdvanceSearch", "ShowCount"));
            workItemModel.controllerName = "AdvanceSearch";

            return PartialView("_WorkItemGrid", workItemModel);

        }
    }
}
