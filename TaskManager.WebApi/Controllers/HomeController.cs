using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;
using TaskManager.WebApi.Models;

namespace TaskManager.WebApi.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        protected readonly IEmployeesService employees;

        public HomeController(IEmployeesService _employees)
        {
            this.employees = _employees;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult GetDateTasks()
        {
            //int identityId = 1;
            //DateTime dateToProcess = DateTime.Now.Date;
            //var emptasks = this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date);
            //var result = new List<SimpleTask>();
            //foreach (var itemTask in emptasks)
            //{
            //    var item = new SimpleTask()
            //    {
            //        Id = itemTask.Id,
            //        Name=itemTask.TaskName,
            //        Roaster = itemTask.TaskTypeName,
            //        Image = "coffeebag.png"
            //    };
            //    result.Add(item);
            //}
            return Json("225");
        }

    }
}
