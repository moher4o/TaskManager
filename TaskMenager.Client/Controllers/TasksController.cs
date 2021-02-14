using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Data.Models;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;
using TaskMenager.Client.Models.Tasks;
using static TaskManager.Common.DataConstants;

namespace TaskMenager.Client.Controllers
{

    public class TasksController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly ITaskTypesService tasktypes;
        private readonly ITaskPrioritysService taskprioritys;
        private readonly IStatusService statuses;
        public TasksController(IDirectorateService directorates, IEmployeesService employees, IDepartmentsService departments, ISectorsService sectors, ITaskTypesService tasktypes, ITaskPrioritysService taskprioritys, IHttpContextAccessor httpContextAccessor, IStatusService statuses, ITasksService tasks) : base(httpContextAccessor, employees, tasks)
        {
            this.statuses = statuses;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.tasktypes = tasktypes;
            this.taskprioritys = taskprioritys;

        }


        public async Task<IActionResult> AddWorkHours(string taskName, int taskId, int employeeId)
        {
            try
            {
                var newWork = new AddWorkedHoursViewModel()
                {
                    employeeId = employeeId,
                    taskId = taskId,
                    TaskName = taskName,
                    employeeFullName = await this.employees.GetEmployeeNameByIdAsync(employeeId)
                };
                return View(newWork);
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Неуспешно генериране на модела за отчитане на часове.";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddWorkHours(AddWorkedHoursViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if (model.HoursSpend <= 0)
                {
                    TempData["Error"] = ":) Часовете трябва да са положително цяло число по-голямо от 0";
                    return View(model);
                }
                var workedHours = new TaskWorkedHoursServiceModel()
                {
                    EmployeeId = model.employeeId,
                    TaskId = model.taskId,
                    HoursSpend = model.HoursSpend,
                    Text = model.Text,
                    WorkDate = model.WorkDate.Date
                };

                string result = await this.tasks.SetWorkedHoursAsync(workedHours);
                if (result == "success")
                {
                    TempData["Success"] = "Часовете са успешно добавени.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = result;
                    return View(model);
                }

            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Моля проверете входните данни.";
                return View(model);
            }

        }


        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateNewTask()
        {
            try
            {
                var newTask = new AddNewTaskViewModel();

                newTask = await TasksModelPrepareForView(newTask);

                return View(newTask);
            }
            catch (Exception)
            {
                TempData["Error"] = "Грешка при създаване на модела за нова задача.";
                return RedirectToAction("Index", "Home");
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateNewTask(AddNewTaskViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    model = await TasksModelPrepareForViewWithOldInfo(model);

                    TempData["Error"] = "Невалидни данни. Моля прегледайте въведената информация за новата задача.";

                    return View(model);
                }

                if (model.Valid_From > model.Valid_To)
                {
                    model = await TasksModelPrepareForViewWithOldInfo(model);
                    TempData["Error"] = "Невалидни дати";
                    return View(model);
                }

                AddNewTaskServiceModel newTask = new AddNewTaskServiceModel();

                newTask.TaskName = model.TaskName;
                newTask.Description = model.Description;
                newTask.StartDate = model.Valid_From;
                newTask.EndDatePrognose = model.Valid_To;
                newTask.OwnerId = currentUser.Id;
                newTask.TypeId = int.Parse(model.TaskTypesId);
                newTask.HoursLimit = model.HoursLimit;
                if (model.EmployeesIds != null && model.EmployeesIds.Length > 0)
                {
                    newTask.EmployeesIds = model.EmployeesIds;
                    newTask.StatusId = await this.statuses.GetStatusIdByNameAsync(TaskStatusInProgres);
                }
                else
                {
                    newTask.StatusId = await this.statuses.GetStatusIdByNameAsync(TaskStatusNew);
                }

                if (int.TryParse(model.DirectoratesId, out int directorateId))
                {
                    newTask.DirectorateId = (directorateId != 0) ? directorateId : (int?)null;
                }
                if (int.TryParse(model.DepartmentsId, out int departmentId))
                {
                    newTask.DepartmentId = (departmentId != 0) ? departmentId : (int?)null;
                }
                if (int.TryParse(model.SectorsId, out int sectorId))
                {
                    newTask.SectorId = (sectorId != 0) ? sectorId : (int?)null;
                }
                if (int.TryParse(model.AssignerId, out int assignerId))
                {
                    if (assignerId == 0)
                    {
                        TempData["Error"] = "Задачата трябва да има назначен отговорник";
                        return View(model);
                    }
                    newTask.AssignerId = assignerId;
                }
                else
                {
                    TempData["Error"] = "Задачата трябва да има назначен отговорник";
                    return View(model);
                }
                if (int.TryParse(model.TaskPriorityId, out int priorityId))
                {
                    if (priorityId == 0)
                    {
                        TempData["Error"] = "Задачата трябва да има определен приоритет";
                        return View(model);
                    }
                    newTask.PriorityId = priorityId;
                }
                else
                {
                    TempData["Error"] = "Задачата трябва да има определен приоритет";
                    return View(model);
                }

                var result = await this.tasks.AddNewTaskAsync(newTask);

                if (result == "success")
                {
                    TempData["Success"] = "Задачата е създадена успешно";
                    return RedirectToAction(nameof(CreateNewTask));

                }
                else
                {
                    TempData["Error"] = result;
                    return View(model);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Моля проверете входните данни.";
                return View(model);
            }
        }

        private async Task<AddNewTaskViewModel> TasksModelPrepareForView(AddNewTaskViewModel newTask)
        {
            List<int> subjectsIds = new List<int>();
            newTask.EmployeesIds = subjectsIds.ToArray();
            if (currentUser.RoleName == SectorAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Sectors = this.sectors.GetSectorsNames(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Assigners = this.employees.GetEmployeesNamesBySector(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
                newTask.Assigners.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Employees = this.employees.GetEmployeesNamesBySector(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == DepartmentAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Sectors = this.sectors.GetSectorsNamesByDepartment(currentUser.DepartmentId)
                                                   .Result
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
                newTask.Sectors.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Assigners = this.employees.GetEmployeesNamesByDepartment(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
                newTask.Assigners.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Employees = this.employees.GetEmployeesNamesByDepartment(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == DirectorateAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNamesByDirectorate(currentUser.DirectorateId)
                                                   .Result
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                    .ToList();
                newTask.Departments.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Sectors.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Assigners = this.employees.GetEmployeesNamesByDirectorate(currentUser.DirectorateId)
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString()
                                   })
                                   .ToList();
                newTask.Assigners.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Employees = this.employees.GetEmployeesNamesByDirectorate(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == SuperAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames()
                                               .Select(a => new SelectListItem
                                               {
                                                   Text = a.TextValue,
                                                   Value = a.Id.ToString()
                                               })
                                               .ToList();
                newTask.Directorates.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Departments.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Sectors.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Assigners = this.employees.GetEmployeesNames()
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString()
                                   })
                                   .ToList();
                newTask.Assigners.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0",
                    Selected = true
                });
                newTask.Employees = this.employees.GetEmployeesNames()
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
            }

            newTask.TaskTypes = this.tasktypes.GetTaskTypesNames()
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = a.TextValue == TaskTypeEmployee ? true : false
                   })
                   .ToList();
            newTask.TaskTypesId = newTask.TaskTypes.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
            newTask.TaskPrioritys = this.taskprioritys.GetTaskPrioritysNames()
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString(),
                                   Selected = a.TextValue == TaskPriorityNormal ? true : false
                               })
                               .ToList();
            newTask.TaskPriorityId = newTask.TaskPrioritys.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();

            return newTask;
        }

        private async Task<AddNewTaskViewModel> TasksModelPrepareForViewWithOldInfo(AddNewTaskViewModel oldTask)
        {
            var newTask = new AddNewTaskViewModel();
            List<int> subjectsIds = new List<int>();
            newTask.EmployeesIds = oldTask.EmployeesIds != null ? oldTask.EmployeesIds : subjectsIds.ToArray();

            if (currentUser.RoleName == TaskManager.Common.DataConstants.Employee)
            {

            }

            if (currentUser.RoleName == SectorAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Sectors = this.sectors.GetSectorsNames(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Assigners = this.employees.GetEmployeesNamesBySector(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = (oldTask.AssignerId == a.Id.ToString()) ? true : false
                                                   })
                                                   .ToList();

                newTask.Employees = this.employees.GetEmployeesNamesBySector(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == DepartmentAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Sectors = this.sectors.GetSectorsNamesByDepartment(currentUser.DepartmentId)
                                                   .Result
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = oldTask.SectorsId == a.Id.ToString() ? true : false
                                                   })
                                                   .ToList();
                if (oldTask.SectorsId == "0" || string.IsNullOrWhiteSpace(oldTask.SectorsId))
                {
                    newTask.Sectors.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newTask.SectorsId = "0";
                }
                else
                {
                    newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                }
                newTask.Assigners = this.employees.GetEmployeesNamesByDepartment(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = (oldTask.AssignerId == a.Id.ToString()) ? true : false
                                                   })
                                                   .ToList();

                newTask.Employees = this.employees.GetEmployeesNamesByDepartment(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == DirectorateAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                newTask.Departments = this.departments.GetDepartmentsNamesByDirectorate(currentUser.DirectorateId)
                                                   .Result
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = oldTask.DepartmentsId == a.Id.ToString() ? true : false
                                                   })
                                                    .ToList();
                if (oldTask.DepartmentsId == "0" || string.IsNullOrWhiteSpace(oldTask.DepartmentsId))   //ако не е избран отдел
                {
                    newTask.Departments.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newTask.DepartmentsId = "0";
                    newTask.Sectors.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newTask.SectorsId = "0";
                }
                else    //ако е избран отдел
                {
                    newTask.DepartmentsId = oldTask.DepartmentsId;
                    newTask.Sectors = this.sectors.GetSectorsNamesByDepartment(int.Parse(newTask.DepartmentsId))
                                                       .Result
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = oldTask.SectorsId == a.Id.ToString() ? true : false
                                                       })
                                                       .ToList();
                    if (oldTask.SectorsId == "0" || string.IsNullOrWhiteSpace(oldTask.SectorsId)) //избран отдел , но не е избран сектор
                    {
                        newTask.Sectors.Insert(0, new SelectListItem
                        {
                            Text = ChooseValue,
                            Value = "0",
                            Selected = true
                        });
                        newTask.SectorsId = "0";
                    }
                    else
                    {
                        newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                    }

                }
                newTask.Assigners = this.employees.GetEmployeesNamesByDirectorate(currentUser.DirectorateId)
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString(),
                                       Selected = (oldTask.AssignerId == a.Id.ToString()) ? true : false
                                   })
                                   .ToList();

                newTask.Employees = this.employees.GetEmployeesNamesByDirectorate(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                                                   })
                                                   .ToList();
            }
            else if (currentUser.RoleName == SuperAdmin)
            {
                newTask.Directorates = this.directorates.GetDirectoratesNames()
                                               .Select(a => new SelectListItem
                                               {
                                                   Text = a.TextValue,
                                                   Value = a.Id.ToString(),
                                                   Selected = oldTask.DirectoratesId == a.Id.ToString() ? true : false
                                               })
                                               .ToList();

                if (oldTask.DirectoratesId == "0") //ако не е избрана дирекция
                {
                    newTask.Directorates.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newTask.Departments.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                    newTask.Sectors.Insert(0, new SelectListItem
                    {
                        Text = ChooseValue,
                        Value = "0",
                        Selected = true
                    });
                }
                else //ako e izbrana дирекция
                {
                    newTask.DirectoratesId = oldTask.DirectoratesId;
                    newTask.Departments = this.departments.GetDepartmentsNamesByDirectorate(int.Parse(newTask.DirectoratesId))
                                                       .Result
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = oldTask.DepartmentsId == a.Id.ToString() ? true : false
                                                       })
                                                        .ToList();
                    if (oldTask.DepartmentsId == "0" || string.IsNullOrWhiteSpace(oldTask.DepartmentsId))   //ако не е избран отдел
                    {
                        newTask.Departments.Insert(0, new SelectListItem
                        {
                            Text = ChooseValue,
                            Value = "0",
                            Selected = true
                        });
                        newTask.Sectors.Insert(0, new SelectListItem
                        {
                            Text = ChooseValue,
                            Value = "0",
                            Selected = true
                        });
                    }
                    else    //ако е избран отдел
                    {
                        newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                        newTask.Sectors = this.sectors.GetSectorsNamesByDepartment(int.Parse(newTask.DepartmentsId))
                                                           .Result
                                                           .Select(a => new SelectListItem
                                                           {
                                                               Text = a.TextValue,
                                                               Value = a.Id.ToString(),
                                                               Selected = oldTask.SectorsId == a.Id.ToString() ? true : false
                                                           })
                                                           .ToList();
                        if (oldTask.SectorsId == "0" || string.IsNullOrWhiteSpace(oldTask.SectorsId))  //око не е избран сектор
                        {
                            newTask.Sectors.Insert(0, new SelectListItem
                            {
                                Text = ChooseValue,
                                Value = "0",
                                Selected = true
                            });
                        }
                        else
                        {
                            newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                        }
                    }

                }
                newTask.Assigners = this.employees.GetEmployeesNames()
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString(),
                                       Selected = (oldTask.AssignerId == a.Id.ToString()) ? true : false
                                   })
                                   .ToList();

                newTask.Employees = this.employees.GetEmployeesNames()
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                                                   })
                                                   .ToList();
            }
            newTask.AssignerId = oldTask.AssignerId;
            newTask.TaskTypes = this.tasktypes.GetTaskTypesNames()
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = a.Id.ToString() == oldTask.TaskTypesId ? true : false
                   })
                   .ToList();
            newTask.TaskTypesId = oldTask.TaskTypesId;
            newTask.TaskPrioritys = this.taskprioritys.GetTaskPrioritysNames()
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString(),
                                   Selected = a.Id.ToString() == oldTask.TaskPriorityId ? true : false
                               })
                               .ToList();
            newTask.TaskPriorityId = oldTask.TaskPriorityId;
            return newTask;
        }

        public IActionResult TaskDetails(int taskId)
        {
            var taskDetails = new TaskViewModel();

            taskDetails = this.tasks.GetTaskDetails(taskId)
                .ProjectTo<TaskViewModel>()
                .FirstOrDefault();


            return View(taskDetails);
        }

        public async Task<IActionResult> ReopenTask(int taskId)
        {
            var result = await this.tasks.ReopenTaskAsync(taskId);

            return RedirectToAction(nameof(TaskDetails), new { taskId });
        }



        public async Task<IActionResult> GetDepartments(string direktorateId)
        {
            var idparseResult = int.TryParse(direktorateId, out int id);
            if (idparseResult)
            {
                var result = await this.departments.GetDepartmentsNamesByDirectorate(id);
                return Json(result);
            }

            return Json(null);
        }

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

    }
}
