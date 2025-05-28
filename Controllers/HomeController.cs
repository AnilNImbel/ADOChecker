using System.Diagnostics;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ADOAnalyser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWorkItem _workItem;

        public HomeController(ILogger<HomeController> logger, IWorkItem workItem)
        {
            _logger = logger;
            _workItem = workItem;
        }

        public IActionResult Index()
        {
            var getProject = _workItem.GetProjects();
            var projectData = JsonConvert.DeserializeObject<ProjectModel>(getProject);
            return View(projectData);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
