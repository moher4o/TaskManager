using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private readonly IRolesService roles;
        //private readonly ITasksService tasks;

        public HomeController(ILogger<HomeController> logger, IEmployeesService employees, IRolesService roles, ITasksService tasks, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, employees, tasks)
        {
            //_logger = logger;
            //this.employees = employees;
            this.roles = roles;
            //this.tasks = tasks;
        }



        public async Task<IActionResult> Index()
        {

            if (this.User.Claims.Any(cl => cl.Type == "DbUpdated"))
            {
                TempData["Error"] = this.User.Claims.Where(cl => cl.Type == "DbUpdated").Select(cl => cl.Value).FirstOrDefault();
                return RedirectToAction("NotAuthorized", "Base");
            }

            var currentEmployee = new UserTasksViewModel()
            {
                userId = currentUser.Id,
                ActiveTasks = await this.employees.GetUserActiveTaskAsync(currentUser.Id),
                AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
            };

            currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
            return View(currentEmployee);
            //string result = string.Empty;
            //if (this.roles.RolesCount() != DataConstants.RolesCount)
            //{
            //    result = await this.roles.CreateRolesAsync();
            //    if (!result.Equals("success"))
            //    {
            //        TempData["Error"] = "Грешка след опит за инициализиране на ролите : " + result + " ";
            //    }
            //    else
            //    {
            //        TempData["Success"] = "Заредени ролите";
            //    }


            //    if (this.tasks.TasksStatusCount() != DataConstants.TasksStatusCount)
            //    {
            //        result = await this.tasks.CreateTasksStatusesAsync();
            //        if (!result.Equals("success"))
            //        {
            //            TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на статусите на задачите : " + result;
            //        }
            //        else
            //        {
            //            TempData["Success"] = TempData["Success"] + "<  >" + "Заредени статусите на задачите";
            //        }

            //    }
            //    if (this.tasks.TasksPrioritysCount() != DataConstants.TasksPriorityCount)
            //    {
            //        result = await this.tasks.CreateTasksPrioritiesAsync();
            //        if (!result.Equals("success"))
            //        {
            //            TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на приоритетите на задачите : " + result;
            //        }
            //        else
            //        {
            //            TempData["Success"] = TempData["Success"] + "<  >" + "Заредени приоритетите на задачите";
            //        }

            //    }
            //    if (this.tasks.TasksTypesCount() != DataConstants.TasksTypesCount)
            //    {
            //        result = await this.tasks.CreateTasksTypesAsync();
            //        if (!result.Equals("success"))
            //        {
            //            TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на типовете задачи : " + result;
            //        }
            //        else
            //        {
            //            TempData["Success"] = TempData["Success"] + "<  >" + "Заредени типовете задачи";
            //        }

            //    }

            //    return RedirectToAction("NotAuthorized");
            //}



            //var logedUser = this.User.Identities.FirstOrDefault().Name.ToLower();
            //var currentEmployee = this.employees.GetUserDataForCooky(logedUser);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        //public IActionResult NotAuthorized()
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CloseTask(int taskId, string taskName)
        {
            var model = new CloseTaskViewModel();

            model.TaskId = taskId;
            model.TaskName = taskName;

            return PartialView("_CloseTaskModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseTask(CloseTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool result = await this.tasks.CloseTaskAsync(model.TaskId, model.EndNote, currentUser.Id);
                if (result)
                {
                    TempData["Success"] = "Задачата е приключена успешно!";
                }
                else
                {
                    TempData["Error"] = "Сървиз грешка! Уведомете администратора.";
                }
                
            }
            return PartialView("_CloseTaskModalPartial", model);
        }

    }
}
