using ADOAnalyser.DBContext;
using ADOAnalyser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static ADOAnalyser.Controllers.BuildController;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    MatchCollection matches = Regex.Matches(pipeline.ResultSummary, @"\d+");

                    if (matches.Count >= 3)
                    {   
                        definitions.Add(new GraphDefinition
                        {
                            Date = pipeline.StartDate?.ToShortDateString() + "-" + pipeline.EndDate?.ToShortDateString(),
                            MissingCount = int.Parse(matches[2].Value),
                            PassingCount = int.Parse(matches[1].Value)
                        });
                    }

                }
            }

            return View(definitions);
        }

        public class GraphDefinition
        {
            public string Date { get; set; }
            public int MissingCount { get; set; }
            public int PassingCount { get; set; }
        }
    }
}
