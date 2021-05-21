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

        public async Task<IActionResult> Index(int? userId)
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

            int identityId = userId.HasValue ? userId.Value : currentUser.Id;
            var currentEmployee = new UserTasksViewModel()
            {
                userId = identityId,
                ActiveTasks = await this.employees.GetUserActiveTaskAsync(identityId),
                //AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
            };
            foreach (var task in currentEmployee.ActiveTasks)
            {
                task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
            }

            currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
            return View(currentEmployee);
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

        public IActionResult NotImplemented()
        {
            return View();
        }


    }
}
