using Microsoft.AspNetCore.Mvc;

namespace ADOAnalyser.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
