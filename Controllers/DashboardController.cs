using Microsoft.AspNetCore.Mvc;

namespace ADOAnalyser.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
