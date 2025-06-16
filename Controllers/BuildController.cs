using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Outlook;
using System.Net.Http;
using ADOAnalyser.Models.BuildsModel;
using ADOAnalyser.Models.PipelineModel;

namespace ADOAnalyser.Controllers
{
    public class BuildController : Controller
    {
        private readonly IWorkItem _workItem;
        private const string FolderPath = "\\CE\\release";
        private const string ProjectName = "CE";

        public BuildController(IWorkItem workItem)
        {
            _workItem = workItem;
        }


        public async Task<IActionResult> Index(string? selectedSprint = null)
        {

            var pipelineJson = await _workItem.GetPipelinesAsync(ProjectName, FolderPath);
            var pipelineData = JsonConvert.DeserializeObject<PipelineBuildModel>(pipelineJson);

            if (pipelineData?.value == null || !pipelineData.value.Any())
                return View(new List<BuildDefinition>());

            var filteredPipelines = pipelineData.value
            .Where(x => x.folder.Equals(FolderPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

            var buildTasks = filteredPipelines.Select(async pipeline =>
            {
                var buildsJson = await _workItem.GetBuildsAsync(ProjectName, pipeline.id);
                var buildsData = JsonConvert.DeserializeObject<BuildModel>(buildsJson);

                return buildsData?.value?.Any() == true
                ? new BuildDefinition
                {
                    Id = pipeline.id,
                    Name = pipeline.name,
                    buildModels = buildsData.value
                }
                : null;
            });

            var definitions = (await Task.WhenAll(buildTasks))
            .Where(def => def != null)
            .ToList();

            return View(definitions);

        }


        public IActionResult GetBuildDetails(int definitionId)
        {
            var BuildDetails = new BuildModel();
            BuildDetails.value = new List<Models.BuildsModel.Value>();
            var buildsJson = _workItem.GetBuilds(ProjectName, definitionId);
            var buildsData = JsonConvert.DeserializeObject<BuildModel>(buildsJson);
            return PartialView("_BuildGrid", buildsData);
        }


        public class BuildDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<ADOAnalyser.Models.BuildsModel.Value> buildModels { get; set; }
        }

    }
}
