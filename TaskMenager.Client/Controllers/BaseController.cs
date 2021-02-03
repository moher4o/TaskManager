using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class BaseController : Controller
    {
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly UserServiceModel currentUser;
        public BaseController(IHttpContextAccessor httpContextAccessor, IEmployeesService employees, ITasksService tasks)
        {
            this.tasks = tasks;
            this.employees = employees;
            this._httpContextAccessor = httpContextAccessor;
            currentUser = this.employees.GetUserDataForCooky(_httpContextAccessor?.HttpContext?.User?.Identities.FirstOrDefault().Name.ToLower());
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
