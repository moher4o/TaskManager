using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;

namespace TaskMenager.Client.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IRolesService roles;
        public UsersController(IEmployeesService employees, IRolesService roles, ITasksService tasks, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, employees, tasks)
        {
            this.roles = roles;
        }

        public IActionResult WellCome()
        {
            TempData["account"] = this.User.Identities.FirstOrDefault().Name.ToLower();
            return View();
        }

        public IActionResult Register()
        {
            if (this.User.Claims.Any(cl => cl.Type == "permissionType" && cl.Value == "Guest"))
            {
                var logedUserAccaunt = this.User.Identities.FirstOrDefault().Name.ToLower();
                //var newUser
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }


            return View();
        }
    }
}
