using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using ADOAnalyser.PipelineModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static ADOAnalyser.Controllers.BuildController;

namespace ADOAnalyser.Controllers
{
    public class GraphController : Controller
    {
        private readonly AppDbContext _context;

        public GraphController(AppDbContext context)
        {
            _context = context;            
        }
        public IActionResult Index()
        {
            var pipelineJson = _context.TestRunResults
                  .OrderByDescending(r => r.RunDate).Take(5)
                  .ToList();
            var definitions = new List<GraphDefinition>();



            if (pipelineJson.Any() == true)
            {
                foreach (var pipeline in pipelineJson)
                {
                    int number = Regex.Matches(pipeline.ResultSummary, @"\d+")
                    .Cast<Match>()
                    .Select(m => int.Parse(m.Value))
                    .FirstOrDefault();


                    definitions.Add(new GraphDefinition
                    {
                        Date = pipeline.StartDate?.ToShortDateString() + "-" + pipeline.EndDate?.ToShortDateString(),
                        Count = number
                    });
                }
            }

            return View(definitions);
        }

        public class GraphDefinition
        {
            public string Date { get; set; }
            public int Count { get; set; }
        }
    }
}
