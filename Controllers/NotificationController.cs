using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using ADOAnalyser.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADOAnalyser.Controllers
{
    public class NotificationController : Controller
    {

        private readonly AppDbContext _dbContext;

        public NotificationController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var emailConfig = _dbContext.EmailConfig.ToList();
            return View(emailConfig);
        }

        [HttpPost]
        public  IActionResult SaveEmail(EmailConfig model)
        {
            TempData["AlertEmail"] = null;
            var existing = _dbContext.EmailConfig.Where(a => a.EmailId.ToUpper().Equals(model.EmailId.ToUpper())).Count();
            if (existing > 0)
            {
                TempData["AlertEmail"] = "Email already exist.";
            }
            else
            {
                var runResult = new EmailConfig
                {
                    EmailId = model.EmailId,
                    ModifiedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                };
                _dbContext.EmailConfig.Add(runResult);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Index"); // or return a view // return with validation errors
        }

    }
}
