using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Outlook;
using System.Net.Http;
using ADOAnalyser.Models.BuildsModel;
using ADOAnalyser.Models.PipelineModel;
using System.Globalization;
using Mono.TextTemplating;

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

        public IActionResult Index()
        {
            var pipelineJson = _workItem.GetPipelines(ProjectName, FolderPath);
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
                            Name = FormatName(pipeline.name)
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
            var buildsJson = _workItem.GetBuilds(ProjectName, definitionId);
            var buildsData = JsonConvert.DeserializeObject<BuildModel>(buildsJson);
            return PartialView("_BuildGrid", buildsData);
        }


        public class BuildDefinition
        {
            public int Id { get; set; }

            public string Name { get; set; }

        }

        public  string FormatName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string trimmed = input.Trim();

            if (trimmed.Contains(" - "))
            {
                string beforeHyphen = trimmed.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim();

                // Preserve mixed-case values like RFC-1222 as-is
                if (beforeHyphen.Any(char.IsDigit) || beforeHyphen.Any(char.IsUpper))
                {
                    return beforeHyphen;
                }
                else
                {
                    return beforeHyphen.ToUpper();
                }
            }
            else
            {
                // Capitalize only the first letter
                return char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();
            }
        }

    }
}
