using Microsoft.AspNetCore.Mvc;

namespace ADOAnalyser.Controllers
{
    public class GraphController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
