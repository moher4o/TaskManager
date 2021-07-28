using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;
using TaskManager.Services.Models;
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
        public UsersController(IEmployeesService employees, ITitleService jobTitles, IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IRolesService roles, ITasksService tasks, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
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
            return View();
        }

        [Authorize(Policy = Employee)]
        public async Task<IActionResult> EditUser(int userId)
        {
            try
            {
                var currentEmployee = await this.employees.GetEmployeeByIdAsync(userId);
                if (currentEmployee == null)
                {
                    TempData["Error"] = $"Няма потребител с Id: {userId}. Обърнете се към администратор";
                    return RedirectToAction("Index", "Home");
                }
                if (currentUser.RoleName == SuperAdmin || currentUser.Id == currentEmployee.Id || (currentUser.RoleName == DirectorateAdmin && currentUser.DirectorateId == currentEmployee.DirectorateId) || (currentUser.RoleName == DepartmentAdmin && currentUser.DepartmentId == currentEmployee.DepartmentId) || (currentUser.RoleName == SectorAdmin && currentUser.SectorId == currentEmployee.SectorId))
                {

                    var userToEdit = new UserRegisterViewModel()
                    {
                        Id = currentEmployee.Id,
                        FullName = currentEmployee.FullName,
                        Email = currentEmployee.Email,
                        TelephoneNumber = currentEmployee.TelephoneNumber,
                        MobileNumber = currentEmployee.MobileNumber,
                        JobTitleId = currentEmployee.JobTitleId,
                        DirectorateId = currentEmployee.DirectorateId,
                        DepartmentId = currentEmployee.DepartmentId,
                        SectorId = currentEmployee.SectorId,
                        isActive = currentEmployee.isActive,
                        isDeleted = currentEmployee.isDeleted,
                        DaeuAccaunt = currentEmployee.DaeuAccaunt,
                        RoleId = currentEmployee.RoleId,
                        Notify = currentEmployee.Notify,
                        RepresentativeId = currentEmployee.RepresentativeId
                    };
                    userToEdit = PrepareUserRegisterModel(userToEdit);
                    return View(userToEdit);
                }
                else
                {
                    TempData["Error"] = "Нямате права за редакция на акаунт: " + currentEmployee.FullName + ". Редакция е възможна само на личен акаунт или на подчинените ви.";
                    return RedirectToAction("UsersList", "Users");
                }
            }
            catch (Exception)
            {
                TempData["Error"] = $"Възникна проблем при подготовка на модела за потребител с Id: {userId}. Обърнете се към администратор";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Policy = Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserRegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model = PrepareUserRegisterModel(model);
                    return View(model);
                }

                if ((model.RoleId.Value == await this.roles.GetUserRoleIdByRoleNameAsync(DirectorateAdmin) && (!model.DirectorateId.HasValue || model.DirectorateId == 0)) || (model.RoleId.Value == await this.roles.GetUserRoleIdByRoleNameAsync(DepartmentAdmin) && (!model.DepartmentId.HasValue || model.DepartmentId == 0)) || (model.RoleId.Value == await this.roles.GetUserRoleIdByRoleNameAsync(SectorAdmin) && (!model.SectorId.HasValue || model.SectorId == 0)))
                {
                    TempData["Error"] = "Има логическа грешка при определянето на роля и принадлежност към адм. единица. Пример: Роля \"SectorAdmin\" може да се присвои, когато е зададен \"Сектор\" и т.н.. Моля прегледайте данните или се свържете с администратор от по-горно ниво!";
                    model = PrepareUserRegisterModel(model);
                    return View(model);
                }

                if (currentUser.RoleName == SuperAdmin || currentUser.Id == model.Id || (currentUser.RoleName == DirectorateAdmin && currentUser.DirectorateId == model.DirectorateId) || (currentUser.RoleName == DepartmentAdmin && currentUser.DepartmentId == model.DepartmentId) || (currentUser.RoleName == SectorAdmin && currentUser.SectorId == model.SectorId))
                {
                    var newUser = new UserServiceModel()
                    {
                        Id = model.Id ?? 0,
                        FullName = model.FullName,
                        Email = model.Email,
                        TelephoneNumber = model.TelephoneNumber,
                        MobileNumber = model.MobileNumber,
                        JobTitleId = model.JobTitleId,
                        DaeuAccaunt = model.DaeuAccaunt,
                        Notify = model.Notify,
                        RoleId = model.RoleId.HasValue ? model.RoleId.Value : await this.roles.GetUserRoleIdByRoleNameAsync(Employee),
                    };
                    if (model.RepresentativeId == null || model.RepresentativeId.Value == 0)
                    {
                        newUser.RepresentativeId = null;
                    }
                    else
                    {
                        newUser.RepresentativeId = model.RepresentativeId.Value;
                    }

                    if (model.DirectorateId == null || model.DirectorateId.Value == 0)
                    {
                        newUser.DirectorateId = null;
                    }
                    else
                    {
                        newUser.DirectorateId = model.DirectorateId.Value;
                    }
                    if (model.DepartmentId == null || model.DepartmentId.Value == 0)
                    {
                        newUser.DepartmentId = null;
                    }
                    else
                    {
                        newUser.DepartmentId = model.DepartmentId.Value;
                    }
                    if (model.SectorId == null || model.SectorId.Value == 0)
                    {
                        newUser.SectorId = null;
                    }
                    else
                    {
                        newUser.SectorId = model.SectorId.Value;
                    }
                    bool result = await this.employees.RegisterNewUserAsync(newUser);
                    if (result)
                    {
                        TempData["Success"] = "Профила е променен успешно.";
                    }
                    else
                    {
                        TempData["Error"] = "Грешка при промяната на профила в БД. Обърнете се към администратор";
                    }
                    return RedirectToAction("EditUser", "Users", new { userId = model.Id });
                }
                else
                {
                    TempData["Error"] = "Нямате права за редакция на профила. Обърнете се към администратор";
                    return RedirectToAction("UsersList", "Users");
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка при промяната на профила. Обърнете се към администратор";
                return RedirectToAction("EditUser", "Users", new { userId = model.Id });
            }
        }

        [Authorize(Policy = "Guest")]
        public async Task<IActionResult> Register()
        {
            if (this.User.Claims.Any(cl => cl.Type == "permission" && cl.Value == "Guest"))
            {
                string logedUserAccaunt = this.User.Identities.FirstOrDefault().Name.ToLower();
                var currentEmployee = this.employees.GetUserDataForCooky(logedUserAccaunt);

                if (currentEmployee == null)
                {
                    var newUser = new UserRegisterViewModel()
                    {
                        DaeuAccaunt = logedUserAccaunt,
                        Email = logedUserAccaunt.Substring(logedUserAccaunt.LastIndexOf("\\") + 1) + "@e-gov.bg",
                        isActive = true,    //нов потребител, но се отбелязва така, за да не излиза предупреждение във вюто
                        RoleId = await this.roles.GetUserRoleIdByRoleNameAsync(Employee)
                    };
                    newUser = PrepareUserRegisterModel(newUser);
                    return View(newUser);
                }
                else      //не е активиран
                {
                    var notActiveUser = new UserRegisterViewModel()
                    {
                        Id = currentEmployee.Id,
                        FullName = currentEmployee.FullName,
                        Email = currentEmployee.Email,
                        TelephoneNumber = currentEmployee.TelephoneNumber,
                        MobileNumber = currentEmployee.MobileNumber,
                        JobTitleId = currentEmployee.JobTitleId,
                        DirectorateId = currentEmployee.DirectorateId,
                        DepartmentId = currentEmployee.DepartmentId,
                        SectorId = currentEmployee.SectorId,
                        isActive = currentEmployee.isActive,
                        DaeuAccaunt = currentEmployee.DaeuAccaunt,
                        RoleId = currentEmployee.RoleId
                    };
                    notActiveUser = PrepareUserRegisterModel(notActiveUser);
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
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.isActive = true;
                    model = PrepareUserRegisterModel(model);
                    return View(model);
                }
                var newUser = new UserServiceModel()
                {
                    Id = model.Id ?? 0,
                    FullName = model.FullName,
                    Email = model.Email,
                    TelephoneNumber = model.TelephoneNumber,
                    MobileNumber = model.MobileNumber,
                    JobTitleId = model.JobTitleId,
                    DaeuAccaunt = this.User.Identities.FirstOrDefault().Name.ToLower()
                };

                if (model.DirectorateId == null || model.DirectorateId.Value == 0)
                {
                    newUser.DirectorateId = null;
                }
                else
                {
                    newUser.DirectorateId = model.DirectorateId.Value;
                }
                if (model.DepartmentId == null || model.DepartmentId.Value == 0)
                {
                    newUser.DepartmentId = null;
                }
                else
                {
                    newUser.DepartmentId = model.DepartmentId.Value;
                }
                if (model.SectorId == null || model.SectorId.Value == 0)
                {
                    newUser.SectorId = null;
                }
                else
                {
                    newUser.SectorId = model.SectorId.Value;
                }

                bool result = await this.employees.RegisterNewUserAsync(newUser);
                if (result)
                {
                    TempData["Success"] = "Профила е създаден/променен успешно.";
                }
                else
                {
                    TempData["Error"] = "Грешка при създаването/промяната на профила. Обърнете се към администратор";
                }
                return RedirectToAction("Register", "Users");

            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка при създаването/промяната на профила. Обърнете се към администратор";
                return RedirectToAction("Register", "Users");
            }
        }

        private UserRegisterViewModel PrepareUserRegisterModel(UserRegisterViewModel newUser)
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
                                   Value = a.Id.ToString(),
                                   Selected = newUser.DirectorateId == a.Id ? true : false
                               })
                               .ToList();
            if (newUser.DirectorateId.HasValue)
            {
                newUser.Departments = this.departments.GetDepartmentsNamesByDirectorate(newUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = newUser.DepartmentId == a.Id ? true : false
                                                   })
                                                   .ToList();
            }
            if (newUser.DepartmentId.HasValue)
            {
                newUser.Sectors = this.sectors.GetSectorsNamesByDepartment(newUser.DepartmentId)
                                                      .Result
                                                      .Select(a => new SelectListItem
                                                      {
                                                          Text = a.TextValue,
                                                          Value = a.Id.ToString(),
                                                          Selected = newUser.SectorId == a.Id ? true : false
                                                      })
                                                      .ToList();
            }

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
            if (currentUser != null)   //при първоначална регистрация е null
            {
                if (currentUser.RoleName == SuperAdmin)
                {
                    newUser.RolesNames = this.roles.GetAllRoles()
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = newUser.RoleId.Value == a.Id ? true : false
                                                       })
                                                       .ToList();
                }
                else if (currentUser.RoleName == DirectorateAdmin)
                {
                    newUser.RolesNames = this.roles.GetAllRoles()
                                                       .Where(r => r.TextValue != SuperAdmin)
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = newUser.RoleId.Value == a.Id ? true : false
                                                       })
                                                       .ToList();
                }
                else if (currentUser.RoleName == DepartmentAdmin)
                {
                    newUser.RolesNames = this.roles.GetAllRoles()
                                                       .Where(r => r.TextValue != SuperAdmin && r.TextValue != DirectorateAdmin)
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = newUser.RoleId.Value == a.Id ? true : false
                                                       })
                                                       .ToList();
                }
                else if (currentUser.RoleName == SectorAdmin)
                {
                    newUser.RolesNames = this.roles.GetAllRoles()
                                                       .Where(r => r.TextValue != SuperAdmin && r.TextValue != DirectorateAdmin && r.TextValue != DepartmentAdmin)
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = newUser.RoleId.Value == a.Id ? true : false
                                                       })
                                                       .ToList();
                }
                else if (currentUser.RoleName == Employee)
                {
                    newUser.RolesNames = this.roles.GetAllRoles()
                                                       .Where(r => r.TextValue == Employee)
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = newUser.RoleId.Value == a.Id ? true : false
                                                       })
                                                       .ToList();
                }
                ////////
                var data = this.employees.GetActiveEmployeesNames();
                var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = (newUser.RepresentativeId == item.Id) ? true : false,
                    Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newUser.RepresentativeList = dropdownList;
                ///////

            }

            return newUser;
        }

        [Authorize(Policy = Employee)]
        public IActionResult UsersList()
        {
            return View();
        }

         #region API Calls
        [HttpGet]
        public IActionResult GetDepartments(string direktorateId)
        {
            var idparseResult = int.TryParse(direktorateId, out int id);
            if (idparseResult)
            {
                var result = this.departments.GetDepartmentsNamesByDirectorate(id);
                return Json(result);
            }

            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectors(string departmentId)
        {
            var idparseResult = int.TryParse(departmentId, out int id);
            if (idparseResult)
            {
                var result = await this.sectors.GetSectorsNamesByDepartment(id);
                return Json(result);
            }

            return Json(null);
        }

        [HttpGet]
        public IActionResult UserExistStatus()
        {
            string logedUserAccaunt = this.User.Identities.FirstOrDefault().Name.ToLower();
            var currentEmployee = this.employees.GetUserDataForCooky(logedUserAccaunt);
            bool result = false;
            if (currentEmployee != null)
            {
                result = true;

            }
            return Json(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var result = await this.employees.DeactivateUserAsync(userId);
            return Json(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> АctivateUser(int userId)
        {
            bool result = await this.employees.АctivateUserAsync(userId);
            return Json(result);
        }

        [Authorize(Policy = Employee)]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(bool deleted = false, bool notActivated = false)
        {
            if (notActivated)
            {
                return RedirectToAction(nameof(GetNotActivatedUsers));
            }
            else
            {
                var users = await this.employees.GetAllUsers(deleted);
                var data = users.Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber
                }).ToList();
                foreach (var user in data)
                {
                    if (currentUser.RoleName == SuperAdmin || currentUser.Id == user.Id || (currentUser.RoleName == DirectorateAdmin && currentUser.DirectorateName == user.DirectorateName) || (currentUser.RoleName == DepartmentAdmin && currentUser.DepartmentName == user.DepartmentName) || (currentUser.RoleName == SectorAdmin && currentUser.SectorName == user.SectorName))
                    {
                        user.Status = "editable";
                    }

                }
                return Json(new { data });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetNotActivatedUsers()
        {
            var data = new List<UsersListViewModel>();
            var users = await this.employees.GetAllNotActivatedUsersAsync();
            if (currentUser.RoleName == SuperAdmin)
            {
                data = users.Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber
                }).ToList();
            }
            else if (currentUser.RoleName == DirectorateAdmin)
            {
                data = users.Where(u => u.DirectorateId == currentUser.DirectorateId).Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber
                }).ToList();
            }
            else if (currentUser.RoleName == DepartmentAdmin)
            {
                data = users.Where(u => u.DepartmentId == currentUser.DepartmentId).Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber
                }).ToList();
            }
            else if (currentUser.RoleName == SectorAdmin)
            {
                data = users.Where(u => u.SectorId == currentUser.SectorId).Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber
                }).ToList();
            }
            return Json(new { data });
        }

        [Authorize(Policy = Employee)]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            var userinfo = await this.employees.GetEmployeeByIdAsync(userId);
            var data = new UsersListViewModel()
            {
                Id = userinfo.Id,
                FullName = userinfo.FullName,
                Email = userinfo.Email,
                DirectorateName = userinfo.DirectorateName,
                DepartmentName = userinfo.DepartmentName,
                SectorName = userinfo.SectorName,
                TelephoneNumber = userinfo.TelephoneNumber,
                MobileNumber = userinfo.MobileNumber,
                JobTitleName = userinfo.JobTitleName,
                RoleName = userinfo.RoleName,
                Status = userinfo.isDeleted ? "Деактивиран акаунт" : (userinfo.isActive ? "Активен акаунт" : "Чакащ одобрение")
            };
            return Json(new { data });
        }

        public async Task<IActionResult> GetDominionUsers()
        {
            var data = await this.employees.GetUserDominions(currentUser.Id);
            data = data.Append(new SelectServiceModel
            {
                Id = currentUser.Id,
                TextValue = currentUser.FullName
            });
            return Json(new { data });
        }
            #endregion

        }
    }
