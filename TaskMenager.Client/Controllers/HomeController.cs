using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskMenager.Client.Infrastructure.Extensions;
using TaskMenager.Client.Models;
using TaskMenager.Client.Models.Home;
using static TaskManager.Common.DataConstants;


namespace TaskMenager.Client.Controllers
{
    public class HomeController : BaseController
    {
        //private readonly ILogger<HomeController> _logger;
        //private readonly IEmployeesService employees;
        //private readonly IRolesService roles;
        private readonly IManageFilesService files;
        //private readonly ITasksService tasks;

        public HomeController(ILogger<HomeController> logger, IEmployeesService employees, IManageFilesService files, ITasksService tasks, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env) : base(httpContextAccessor, employees, tasks, email, env)
        {
            //_logger = logger;
            this.files = files;
            //this.roles = roles;
            //this.tasks = tasks;
        }


        
        //public async Task<IActionResult> Index()
        //{
        //    if (this.User.Claims.Any(cl => cl.Value == "Guest"))
        //    {
        //       return RedirectToAction("WellCome", "Users");
        //    }


        //    if (this.User.Claims.Any(cl => cl.Type == "DbUpdated"))
        //    {
        //        TempData["Error"] = this.User.Claims.Where(cl => cl.Type == "DbUpdated").Select(cl => cl.Value).FirstOrDefault();
        //        return RedirectToAction("NotAuthorized", "Base");
        //    }

        //    var currentEmployee = new UserTasksViewModel()
        //    {
        //        userId = currentUser.Id,
        //        ActiveTasks = await this.employees.GetUserActiveTaskAsync(currentUser.Id),
        //        //AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
        //    };
        //    foreach (var task in currentEmployee.ActiveTasks)
        //    {
        //        task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
        //    }

        //    currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
        //    return View(currentEmployee);
        //}

        public IActionResult Index()
        {
            if (this.User.Claims.Any(cl => cl.Value == "Guest"))
            {
                return RedirectToAction("WellCome", "Users");
            }


            if (this.User.Claims.Any(cl => cl.Type == "DbUpdated"))
            {
                TempData["Error"] = this.User.Claims.Where(cl => cl.Type == "DbUpdated").Select(cl => cl.Value).FirstOrDefault();
                return RedirectToAction("NotAuthorized", "Base");
            }


            return View();
        }

        //public async Task<IActionResult> Index(int? userId, DateTime? workDate)
        //{
        //    if (this.User.Claims.Any(cl => cl.Value == "Guest"))
        //    {
        //        return RedirectToAction("WellCome", "Users");
        //    }


        //    if (this.User.Claims.Any(cl => cl.Type == "DbUpdated"))
        //    {
        //        TempData["Error"] = this.User.Claims.Where(cl => cl.Type == "DbUpdated").Select(cl => cl.Value).FirstOrDefault();
        //        return RedirectToAction("NotAuthorized", "Base");
        //    }

        //    int identityId = userId.HasValue ? userId.Value : currentUser.Id;
        //    DateTime dateToProcess = workDate.HasValue ? workDate.Value.Date : DateTime.Now.Date;

        //    if (identityId != currentUser.Id)
        //    {
        //        var dominions = await this.employees.GetUserDominions(currentUser.Id);
        //        if (!dominions.Any(d => d.Id == identityId))
        //        {
        //            var targetEmployee = await this.employees.GetEmployeeByIdAsync(identityId);
        //            TempData["Error"] = $"[Index]. {currentUser.FullName} не е представител на {targetEmployee.FullName} ";
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }


        //    var currentEmployee = new UserTasksViewModel()
        //    {
        //        userId = identityId,
        //        workDate = dateToProcess.Date,
        //        ActiveTasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date),
        //        //AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
        //    };
        //    foreach (var task in currentEmployee.ActiveTasks)
        //    {
        //        task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
        //    }

        //    currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
        //    return View(currentEmployee);
        //}

        public IActionResult Privacy()
        {
            return View();
        }

         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotImplemented()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> GetDateTasks(int? userId, DateTime? workDate)
        {
            int identityId = userId.HasValue ? userId.Value : currentUser.Id;
            DateTime dateToProcess = workDate.HasValue ? workDate.Value.Date : DateTime.Now.Date;

            var currentEmployee = new UserTasksViewModel()
            {
                userId = identityId,
                workDate = dateToProcess.Date,
                ActiveTasks =await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date),
                //AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
            };
            foreach (var task in currentEmployee.ActiveTasks)
            {
                task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
            }

            currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);

            return PartialView("~/Views/Home/_ShowTasksPartial.cshtml", currentEmployee);
        }
        #region API Calls
        //[Authorize(Policy = Employee)]
        //[HttpGet]
        //public async Task<IActionResult> GetDateTasksAsync(int? userId, DateTime? workDate)
        //{
        //    int identityId = userId.HasValue ? userId.Value : currentUser.Id;
        //    DateTime dateToProcess = workDate.HasValue ? workDate.Value.Date : DateTime.Now.Date;

        //    var currentEmployee = new UserTasksViewModel()
        //    {
        //        userId = identityId,
        //        workDate = dateToProcess.Date,
        //        ActiveTasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date),
        //    };
        //    foreach (var task in currentEmployee.ActiveTasks)
        //    {
        //        task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
        //    }

        //    currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
        //    return Json(new { currentEmployee });
        //}
        #endregion

    }
}
