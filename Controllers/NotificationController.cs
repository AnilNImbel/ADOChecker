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
            return RedirectToAction("Index"); 
        }

        public IActionResult InActive(int id, bool status)
        {
            TempData["AlertEmail"] = null;
            var emailData = _dbContext.EmailConfig.FirstOrDefault(a => a.Id == id);

            if (emailData != null)
            {
                emailData.IsActive = status ? false : true;
                emailData.ModifiedDate = DateTime.Now;
                _dbContext.EmailConfig.Update(emailData);
                _dbContext.SaveChanges();
                TempData["AlertEmail"] = "Email Updated successfully.";
            }
            else
            {
                TempData["AlertEmail"] = "Unable to retrieve the data.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            TempData["AlertEmail"] = null;
            var emailData = _dbContext.EmailConfig.FirstOrDefault(a => a.Id == id);

            if (emailData != null)
            {
                _dbContext.EmailConfig.Remove(emailData);
                _dbContext.SaveChanges();

                TempData["AlertEmail"] = "Email Deleted successfully.";
            }
            else
            {
                TempData["AlertEmail"] = "Email not found.";
            }
            return RedirectToAction("Index");
        }
    }
}
