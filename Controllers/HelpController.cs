using Microsoft.AspNetCore.Mvc;

namespace ADOAnalyser.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
