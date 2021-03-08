using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;
using TaskManager.Services.Models.EmployeeModels;
using TaskMenager.Client.Models.Users;
using static TaskManager.Common.DataConstants;

namespace TaskMenager.Client.Controllers
{

    public class UsersController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly IRolesService roles;
        private readonly ITitleService jobTitles;
        public UsersController(IEmployeesService employees, ITitleService jobTitles, IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IRolesService roles, ITasksService tasks, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, employees, tasks)
        {
            this.roles = roles;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.jobTitles = jobTitles;
        }

        [Authorize(Policy = "Guest")]
        public IActionResult WellCome()
        {

            //TempData["account"] = this.User.Identities.FirstOrDefault().Name.ToLower();
            return View();
        }

        [Authorize(Policy = "Guest")]
        public IActionResult Register()
        {
            if (this.User.Claims.Any(cl => cl.Type == "permissionType" && cl.Value == "Guest"))
            {
                string logedUserAccaunt = this.User.Identities.FirstOrDefault().Name.ToLower();
                var currentEmployee = this.employees.GetUserDataForCooky(logedUserAccaunt);
                if (currentEmployee == null)
                {
                    var newUser = new UserRegisterViewModel()
                    {
                        DaeuAccaunt = logedUserAccaunt,
                        Email = logedUserAccaunt.Substring(logedUserAccaunt.LastIndexOf("\\") + 1) + "@e-gov.bg",
                        isActive = true    //нов потребител, но се отбелязва така, за да не излиза предупреждение във вюто
                    };
                    newUser = PrepareUserModel(newUser);
                    return View(newUser);
                }
                else      //не е активиран
                {
                    var notActiveUser = new UserRegisterViewModel();
                    return View(notActiveUser);
                }
                
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Policy = "Guest")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserRegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.isActive = true;
                    return View(model);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Register", "Users");
            }
            return RedirectToAction("NotImplemented", "Home");
        }

        private UserRegisterViewModel PrepareUserModel(UserRegisterViewModel newUser)
        {
            newUser.JobTitles = this.jobTitles.GetJobTitlesNames()
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString()
                               })
                               .ToList();
            newUser.JobTitles.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });
            newUser.Directorates = this.directorates.GetDirectoratesNames(null)
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString()
                               })
                               .ToList();
            newUser.Directorates.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });
            newUser.Departments.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });
            newUser.Sectors.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });
            return newUser;
        }
    }
}
