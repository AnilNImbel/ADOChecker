using ADOAnalyser.PipelineModel;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Outlook;
using System.Net.Http;
using ADOAnalyser.Models.BuildsModel;

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

        //
        public IActionResult Index(string? selectedSprint = null)
        {
           
            var pipelineJson =  _workItem.GetPipelines(ProjectName, FolderPath);
            var definitions = new List<BuildDefinition>();
            var pipelineData = JsonConvert.DeserializeObject<PipelineBuildModel>(pipelineJson);
            if (pipelineData?.value?.Any() == true)
            {
                pipelineData.value = pipelineData.value.Where(x => x.folder.Equals(FolderPath)).ToList();

                if (pipelineData.value?.Any() == true)
                {
                    foreach (var pipeline in pipelineData.value)
                    {

                        definitions.Add(new BuildDefinition
                        {
                            Id = pipeline.id,
                            Name = pipeline.name
                        });
                    }
                }
            }
            return View(definitions);
        }

        public IActionResult GetBuildDetails(int definitionId)
        {
            var BuildDetails = new BuildModel();
            BuildDetails.value = new List<Models.BuildsModel.Value>();
            var buildsJson = _workItem.GetBuilds(ProjectName, definitionId, "all");
            var buildsData = JsonConvert.DeserializeObject<BuildModel>(buildsJson);
            return PartialView("_BuildGrid", buildsData);
        }


        public class BuildDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

    }
}
