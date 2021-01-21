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


namespace TaskMenager.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmployeesService employees;
        private readonly IRolesService roles;
        private readonly ITasksService tasks;

        public HomeController(ILogger<HomeController> logger, IEmployeesService employees, IRolesService roles, ITasksService tasks)
        {
            _logger = logger;
            this.employees = employees;
            this.roles = roles;
            this.tasks = tasks;
        }

        public async Task<IActionResult> Index()
        {
            string result = string.Empty;
            if (this.roles.RolesCount() != DataConstants.RolesCount)
            {
                result = await this.roles.CreateRolesAsync();
                if (!result.Equals("success"))
                {
                    TempData["Error"] = "Грешка след опит за инициализиране на ролите : " + result + " ";
                }
                else
                {
                    TempData["Success"] = "Заредени ролите";
                }


                if (this.tasks.TasksStatusCount() != DataConstants.TasksStatusCount)
                {
                    result = await this.tasks.CreateTasksStatusesAsync();
                    if (!result.Equals("success"))
                    {
                        TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на статусите на задачите : " + result;
                    }
                    else
                    {
                        TempData["Success"] = TempData["Success"] + "<  >" + "Заредени статусите на задачите";
                    }

                }
                if (this.tasks.TasksPrioritysCount() != DataConstants.TasksPriorityCount)
                {
                    result = await this.tasks.CreateTasksPrioritiesAsync();
                    if (!result.Equals("success"))
                    {
                        TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на приоритетите на задачите : " + result;
                    }
                    else
                    {
                        TempData["Success"] = TempData["Success"] + "<  >" + "Заредени приоритетите на задачите";
                    }

                }
                if (this.tasks.TasksTypesCount() != DataConstants.TasksTypesCount)
                {
                    result = await this.tasks.CreateTasksTypesAsync();
                    if (!result.Equals("success"))
                    {
                        TempData["Error"] = TempData["Error"] + "<  >" + "Грешка след опит за инициализиране на типовете задачи : " + result;
                    }
                    else
                    {
                        TempData["Success"] = TempData["Success"] + "<  >" + "Заредени типовете задачи";
                    }

                }

                return RedirectToAction("NotAuthorized");
            }



            var logedUser = this.User.Identities.FirstOrDefault().Name.ToLower();
            var currentEmployee = this.employees.GetUserDataForCooky(logedUser);
            if (currentEmployee == null)
            {
                TempData["Error"] = logedUser + " Няма създаден акаунт в системата";
                return RedirectToAction("NotAuthorized");
            }

            //var clTr = new ClaimsTransformer();
            //var br = this.User.Claims.Count();
            //this.User.Identities.FirstOrDefault().AddClaim(new Claim("permission", "DepartmentAdmin"));
            //br = this.User.Claims.Count();
            //if (userForCoocy == null)
            //{
            //    var claims = this.User.Claims.ToList();
            //    //this.User.Claims.Append(new System.Security.Claims.Claim())
            //    foreach (var cookie in Request.Cookies.Keys)
            //    {
            //        Response.Cookies.Delete(cookie);
            //    }


            //    await HttpContext.AuthenticateAsync();
            //    // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //    Response.Cookies.Append("EdgeAccessCookie", "", new Microsoft.AspNetCore.Http.CookieOptions()
            //    {
            //        Path = "/",
            //        HttpOnly = true,
            //        SameSite = SameSiteMode.Lax,
            //        Expires = DateTime.Now.AddDays(-1)
            //    });

            //    TempData["Error"] = "Потребител: " + logedUser + " няма конфигуриран достъп!";
            //    return RedirectToAction("NotAuthorized");
            //}

            //var userFromCoocy = new UserCookyServiceModel();
            //if (this.HttpContext.Request.Cookies.ContainsKey("daeuuser"))
            //{
            //    try
            //    {
            //        this.HttpContext.Request.Cookies.TryGetValue("daeuuser", out string userSerialized);
            //        userFromCoocy = JsonConvert.DeserializeObject<UserCookyServiceModel>(userSerialized);
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

            //Response.Cookies.Append("user", JsonConvert.SerializeObject(model.SearchPatern))
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
