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
using TaskMenager.Client.Models.Home;
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

        public async Task<IActionResult> CreatedTasks()
        {
            var currentEmployee = new UserTasksViewModel()
            {
                userId = currentUser.Id,
                CreatedTasks = await this.employees.GetUserCreatedTaskAsync(currentUser.Id)
            };
            return View(currentEmployee);
        }

        public async Task<IActionResult> AssignerTasks()
        {
            var currentEmployee = new UserTasksViewModel()
            {
                userId = currentUser.Id,
                AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
            };
            return View(currentEmployee);
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
        [ValidateAntiForgeryToken]
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
                    EmployeeName = model.employeeFullName,
                    TaskName = model.TaskName,
                    TaskId = model.taskId,
                    HoursSpend = model.HoursSpend,
                    Text = model.Text,
                    WorkDate = model.WorkDate.Date
                };

                string result = await this.tasks.SetWorkedHoursAsync(workedHours);
                if (result == "success")
                {
                    TempData["Success"] = "Часовете са добавени успешно.";
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

        public async Task<IActionResult> EditTask(int taskId)
        {
            try
            {
               var taskDetails = this.tasks.GetTaskDetails(taskId)
                        .ProjectTo<TaskViewModel>()
                        .FirstOrDefault();
                if (currentUser.RoleName != SuperAdmin && currentUser.Id != taskDetails.OwnerId && currentUser.Id != taskDetails.AssignerId)
                {
                    TempData["Error"] = "Суперадмин, създател и отговорник имат право да променят задача!";
                    return RedirectToAction(nameof(TaskDetails), new { taskId });
                }

                if (taskDetails.TaskStatusName == TaskStatusClosed)
                {
                    TempData["Error"] = "Не се променя приключена задача!";
                    return RedirectToAction(nameof(TaskDetails), new { taskId });
                }

                var taskToEdit = new AddNewTaskViewModel()
                {
                     DirectoratesId = taskDetails.DirectorateId.ToString(),
                     DepartmentsId = taskDetails.DepartmentId.ToString(),
                     SectorsId = taskDetails.SectorId.ToString(),
                     AssignerId = taskDetails.AssignerId.ToString(),
                     TaskPriorityId = taskDetails.PriorityId.ToString(),
                     TaskTypesId = taskDetails.TypeId.ToString(),
                     Valid_From = taskDetails.StartDate.Date,
                     Valid_To = taskDetails.EndDatePrognose.Value.Date
                };

                 //var assignedEmployees = new List<int>();
                var assignedEmployees = new List<SelectServiceModel>();
                assignedEmployees.AddRange(taskDetails.Colleagues.ToList());

                taskToEdit.EmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToArray(); //за да изключи премахнатите експерти

                taskToEdit = await TasksModelPrepareForViewWithOldInfo(taskToEdit);

                taskToEdit.EmployeesIds = assignedEmployees.Select(a => a.Id).ToArray();   //за да включи всички работили по задачата в списъка(може и да не са активни, но трябва да са в списъка)

                if (taskToEdit.Employees.Count < taskToEdit.EmployeesIds.Length)    //добавям членовете на задачата, които не са в йерархиата на отговорника
                {
                    foreach (var empId in taskToEdit.EmployeesIds)
                    {
                        if (taskToEdit.Employees.FirstOrDefault(e => e.Value == empId.ToString()) == null)
                        {
                            var curentEmployee = assignedEmployees.Where(e => e.Id == empId).FirstOrDefault();
                            taskToEdit.Employees.Add(new SelectListItem
                            {
                                Text = curentEmployee.TextValue,
                                Value = empId.ToString(),
                                Selected = curentEmployee.isDeleted ? false : true
                            });
                            taskToEdit.Assigners.Add(new SelectListItem
                            {
                                Text = curentEmployee.TextValue,
                                Value = empId.ToString()
                            });

                        }
                    }
                }
                taskToEdit.EmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToArray();  //за да изключи премахнатите експерти

                taskToEdit.Id = taskDetails.Id;
                taskToEdit.OwnerId = taskDetails.OwnerId;
                taskToEdit.TaskName = taskDetails.TaskName;
                taskToEdit.Description = taskDetails.Description;
                taskToEdit.HoursLimit = taskDetails.HoursLimit;
                taskToEdit.ParentTaskId = taskDetails.ParentTaskId;

                return View(taskToEdit);
            }
            catch (Exception)
            {
                TempData["Error"] = "Грешка при създаване на модела за промяна на задачата.";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(AddNewTaskViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    model = await TasksModelPrepareForViewWithOldInfo(model);

                    TempData["Error"] = "Невалидни данни. Моля прегледайте въведената информация за новата задача.";

                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                if (model.Valid_From > model.Valid_To)
                {
                    TempData["Error"] = "Невалидни дати";
                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                AddNewTaskServiceModel taskToEdit = new AddNewTaskServiceModel();

                taskToEdit.Id = model.Id;
                taskToEdit.TaskName = model.TaskName;
                taskToEdit.Description = model.Description;
                taskToEdit.StartDate = model.Valid_From;
                taskToEdit.EndDatePrognose = model.Valid_To;
                taskToEdit.OwnerId = model.OwnerId;
                taskToEdit.TypeId = int.Parse(model.TaskTypesId);
                taskToEdit.HoursLimit = model.HoursLimit;
                if (model.EmployeesIds != null && model.EmployeesIds.Length > 0)
                {
                    taskToEdit.EmployeesIds = model.EmployeesIds;
                    taskToEdit.StatusId = await this.statuses.GetStatusIdByNameAsync(TaskStatusInProgres);
                }
                else
                {
                    taskToEdit.StatusId = await this.statuses.GetStatusIdByNameAsync(TaskStatusNew);
                }

                if (int.TryParse(model.DirectoratesId, out int directorateId))
                {
                    taskToEdit.DirectorateId = (directorateId != 0) ? directorateId : (int?)null;
                }
                if (int.TryParse(model.DepartmentsId, out int departmentId))
                {
                    taskToEdit.DepartmentId = (departmentId != 0) ? departmentId : (int?)null;
                }
                if (int.TryParse(model.SectorsId, out int sectorId))
                {
                    taskToEdit.SectorId = (sectorId != 0) ? sectorId : (int?)null;
                }
                if (int.TryParse(model.AssignerId, out int assignerId))
                {
                    if (assignerId == 0)
                    {
                        TempData["Error"] = "Задачата трябва да има назначен отговорник";
                        return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                    }
                    taskToEdit.AssignerId = assignerId;
                }
                else
                {
                    TempData["Error"] = "Задачата трябва да има назначен отговорник";
                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }
                if (int.TryParse(model.TaskPriorityId, out int priorityId))
                {
                    if (priorityId == 0)
                    {
                        TempData["Error"] = "Задачата трябва да има определен приоритет";
                        return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                    }
                    taskToEdit.PriorityId = priorityId;
                }
                else
                {
                    TempData["Error"] = "Задачата трябва да има определен приоритет";
                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                string result = await this.tasks.EditTaskAsync(taskToEdit);

                if (result == "success")
                {
                    TempData["Success"] = "Промените са записани успешно";
                    return RedirectToAction(nameof(TaskDetails), new { taskId = model.Id });
                }
                else if(result == "halfsuccess")
                {
                    TempData["Success"] = "Промените са записани успешно, но началната дата е съобразена с първите отчетени часове!";
                    return RedirectToAction(nameof(TaskDetails), new { taskId = model.Id });
                }
                else
                {
                    TempData["Error"] = result;
                    return RedirectToAction(nameof(TaskDetails), new { taskId = model.Id });
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Неуспешен запис на промените.";
                return RedirectToAction("Index", "Home");
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
        [ValidateAntiForgeryToken]
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
                newTask.Directorates = this.directorates.GetDirectoratesNames(null)
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
                newTask.Assigners = this.employees.GetActiveEmployeesNames()
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
                newTask.Employees = this.employees.GetActiveEmployeesNames()
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
                       Selected = a.TextValue == TaskTypeSpecificWork ? true : false
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
                if (currentUser.SectorId != null)
                {
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
                else
                {
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
                newTask.Directorates = this.directorates.GetDirectoratesNames(null)
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
                newTask.Assigners = this.employees.GetActiveEmployeesNames()
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue,
                                       Value = a.Id.ToString(),
                                       Selected = (oldTask.AssignerId == a.Id.ToString()) ? true : false
                                   })
                                   .ToList();

                newTask.Employees = this.employees.GetActiveEmployeesNames()
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
            newTask.HoursLimit = oldTask.HoursLimit;
            newTask.Valid_From = oldTask.Valid_From;
            newTask.Valid_To = oldTask.Valid_To;
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
