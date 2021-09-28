using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Data.Models;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;
using TaskMenager.Client.Models.Home;
using TaskMenager.Client.Models.Tasks;
using static TaskManager.Common.DataConstants;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class TasksController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly ITaskTypesService tasktypes;
        private readonly ITaskPrioritysService taskprioritys;
        private readonly IStatusService statuses;
        private readonly IManageFilesService files;
        public TasksController(IManageFilesService files, IDirectorateService directorates, IEmployeesService employees, IDepartmentsService departments, ISectorsService sectors, ITaskTypesService tasktypes, ITaskPrioritysService taskprioritys, IHttpContextAccessor httpContextAccessor, IStatusService statuses, ITasksService tasks, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
        {
            this.statuses = statuses;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.tasktypes = tasktypes;
            this.taskprioritys = taskprioritys;
            this.files = files;
        }

        public IActionResult TasksList()
        {
            return View();
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
            foreach (var task in currentEmployee.AssignerTasks)
            {
                task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
            }

            return View(currentEmployee);
        }

        public async Task<IActionResult> AddWorkHours(string taskName, int taskId, int employeeId)
        {
            try
            {
                if (employeeId != currentUser.Id)
                {
                    var dominions = await this.employees.GetUserDominions(currentUser.Id);
                    if (!dominions.Any(d => d.Id == employeeId))
                    {
                        var targetEmployee = await this.employees.GetEmployeeByIdAsync(employeeId);
                        TempData["Error"] = $"[AddWorkHours]. {currentUser.FullName} не е представител на {targetEmployee.FullName} ";
                        return RedirectToAction("Index", "Home");
                    }
                }

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
                TempData["Error"] = "[AddWorkHours]. Неуспешно генериране на модела за отчитане на часове.";
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

                if (model.employeeId != currentUser.Id)
                {
                    var dominions = await this.employees.GetUserDominions(currentUser.Id);
                    if (!dominions.Any(d => d.Id == model.employeeId))
                    {
                        var targetEmployee = await this.employees.GetEmployeeByIdAsync(model.employeeId);
                        TempData["Error"] = $"[AddWorkHours]. {currentUser.FullName} не е представител на {targetEmployee.FullName} ";
                        return RedirectToAction("Index", "Home");
                    }
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

                string result = await this.tasks.SetWorkedHoursAsyncOld(workedHours);
                if (result == "success")
                {
                    TempData["Success"] = "Часовете са актуализирани успешно.";
                    return View(model);
                }
                else
                {
                    TempData["Error"] = result;
                    return View(model);
                }

            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Моля проверете логиката на входните данни.";
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
                if (currentUser.RoleName != SuperAdmin && currentUser.Id != taskDetails.OwnerId && currentUser.Id != taskDetails.AssignerId && (currentUser.RoleName == DepartmentAdmin && currentUser.DepartmentId != taskDetails.DepartmentId) && (currentUser.RoleName == DirectorateAdmin && currentUser.DirectorateId != taskDetails.DirectorateId) && (currentUser.RoleName == SectorAdmin && currentUser.SectorId != taskDetails.SectorId))
                {
                    TempData["Error"] = "Суперадмин, създател и отговорник имат право да променят задача!";
                    return RedirectToAction(nameof(TaskDetails), new { taskId });
                }

                if (taskDetails.TaskStatusName == TaskStatusClosed)
                {
                    TempData["Error"] = "Не се променя приключена задача!";
                    return RedirectToAction(nameof(TaskDetails), new { taskId });
                }

                if (taskDetails.TaskName.ToLower() == "отпуски" || taskDetails.TaskName.ToLower() == "болнични")
                {
                    TempData["Error"] = "Системните задачи не се редактират!";
                    return RedirectToAction("Index", "Home");
                }


                var taskToEdit = new AddNewTaskViewModel()
                {
                     ParentTaskId = taskDetails.ParentTaskId,
                     DirectoratesId = taskDetails.DirectorateId.ToString(),
                     DepartmentsId = taskDetails.DepartmentId.ToString(),
                     SectorsId = taskDetails.SectorId.ToString(),
                     AssignerIdInt = taskDetails.AssignerId,
                     TaskPriorityId = taskDetails.PriorityId.ToString(),
                     TaskTypesId = taskDetails.TypeId.ToString(),
                     Valid_From = taskDetails.StartDate.Date,
                     Valid_To = taskDetails.EndDatePrognose.Value.Date
            };
                //var assignedEmployees = new List<SelectServiceModel>();
                //assignedEmployees.AddRange(taskDetails.Colleagues.ToList());

                //taskToEdit.EmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToArray(); //за да изключи премахнатите експерти

                //taskToEdit = await TasksModelPrepareForViewWithOldInfo(taskToEdit);

                //taskToEdit.EmployeesIds = assignedEmployees.Select(a => a.Id).ToArray();   //за да включи всички работили по задачата в списъка(може и да не са активни, но трябва да са в списъка)

                //if (!taskToEdit.EmployeesIds.All(elem => taskToEdit.Employees.Select(e => int.Parse(e.Value)).ToArray().Contains(elem) )) //добавям членовете на задачата, които не са в йерархиата на отговорника
                //{
                //    var tempSelectListItems = new List<SelectListItem>();
                //    foreach (var empId in taskToEdit.EmployeesIds)
                //    {
                //        if (taskToEdit.Employees.FirstOrDefault(e => e.Value == empId.ToString()) == null)
                //        {
                //            var curentEmployee = assignedEmployees.Where(e => e.Id == empId).FirstOrDefault();
                //            if (this.employees.GetEmployeeByIdAsync(empId).Result.isDeleted == false)   //проверка дали колегата, който е вкл. в задачата, междувременно не е маркиран като изтрит акаунт
                //            {
                //                taskToEdit.Employees.Add(new SelectListItem
                //                {
                //                    Text = curentEmployee.TextValue,
                //                    Value = empId.ToString(),
                //                    Selected = curentEmployee.isDeleted ? false : true
                //                });
                //                /////
                //                tempSelectListItems.Add(new SelectListItem
                //                {
                //                    Text = curentEmployee.TextValue,
                //                    Value = curentEmployee.Id.ToString(),
                //                    Selected = false,
                //                    Group = new SelectListGroup { Name = curentEmployee.DepartmentName }
                //                });
                //                /////
                //            }
                //        }
                //    }
                //     if (tempSelectListItems.Count > 0)
                //    {
                //        List<SelectListItem> _assignersList = taskToEdit.AssignersList.Select(asl => new SelectListItem
                //        {
                //            Text = asl.Text,
                //            Value = asl.Value,
                //            Selected = false,
                //            Group = new SelectListGroup { Name = (asl.Group.Name ?? "0_Служители без присвоен отдел") }
                //        }).ToList();
                //        _assignersList = _assignersList.Concat(tempSelectListItems).ToList();
                //        taskToEdit.AssignersList = new SelectList(_assignersList, "Value", "Text", "-1", "Group.Name");
                //    }

                //}
                //taskToEdit.EmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToArray();  //за да изключи премахнатите експерти

                //???????????????????
                //taskToEdit.AssignersList = new SelectList(;
                taskToEdit = await TasksModelPrepareForViewWithOldInfo(taskToEdit);
                var employeesOnTask = await this.GetEmployeesOnTask(taskDetails.TaskTypeName == TaskTypeSpecialTeam ? true : false, taskDetails.Id);
                taskToEdit.EmployeesList = employeesOnTask;
                taskToEdit.EmployeesIds = taskDetails.Colleagues.ToList().Where(e => e.isDeleted == false).Select(a => a.Id).ToArray();  //за да изключи премахнатите експерти
                taskToEdit.AssignersList = employeesOnTask;
                taskToEdit.AssignerIdInt = taskDetails.AssignerId;
                //???????????????????

                taskToEdit.Id = taskDetails.Id;
                taskToEdit.OwnerId = taskDetails.OwnerId;
                taskToEdit.TaskName = taskDetails.TaskName;
                taskToEdit.Description = taskDetails.Description;
                taskToEdit.HoursLimit = taskDetails.HoursLimit;
                taskToEdit.ParentTaskId = taskDetails.ParentTaskId;

                if (taskDetails.TypeId == await this.tasktypes.GetTaskTypeIdByNameAsync(TaskTypeGlobal))
                {
                    var childrens = await this.tasks.GetTaskChildsIdsAsync(taskToEdit.Id);
                    taskToEdit.ChildrenTasksCount = childrens.Count();
                }
                else
                {
                    taskToEdit.ChildrenTasksCount = 0;
                }


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
                    TempData["Error"] = "Невалидни данни. Моля прегледайте въведената информация за новата задача.";

                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                var globalTaskId = await this.tasktypes.GetTaskTypeIdByNameAsync(TaskTypeGlobal);
                if ((model.ParentTaskId == 0 || model.ParentTaskId == null) && model.TaskTypesId != globalTaskId.ToString())
                {
                    TempData["Error"] = "Невалидни данни. Или изберете задача родител (полето \"Подзадача на:\"), или сменете типа на задачата на \"Глобална\".";
                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                if (model.TaskName.ToLower() == "отпуски" || model.TaskName.ToLower() == "отпуска" || model.TaskName.ToLower() == "болнични" || model.TaskName.ToLower() == "болничен")
                {
                    TempData["Error"] = "Името на задачата е заето за системни нужди!";
                    return RedirectToAction("Index", "Home");
                }


                if (model.Valid_From > model.Valid_To)
                {
                    TempData["Error"] = "Некоректни дати за начало и край на задачата";
                    return RedirectToAction(nameof(EditTask), new { taskId = model.Id });
                }

                // номерата на старите колеги преди ъпдейта
                var taskDetails = this.tasks.GetTaskDetails(model.Id)
                                             .ProjectTo<TaskViewModel>()
                                             .FirstOrDefault();
                var assignedEmployees = new List<SelectServiceModel>();
                assignedEmployees.AddRange(taskDetails.Colleagues.ToList());

                var oldActiveEmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToList(); //за да изключи премахнатите експерти
                oldActiveEmployeesIds.Insert(0,taskDetails.AssignerId);
                // номерата на старите колеги преди ъпдейта  край

                AddNewTaskServiceModel taskToEdit = new AddNewTaskServiceModel();

                taskToEdit.Id = model.Id;
                taskToEdit.TaskName = model.TaskName;
                taskToEdit.Description = model.Description;
                taskToEdit.StartDate = model.Valid_From;
                taskToEdit.EndDatePrognose = model.Valid_To;
                taskToEdit.OwnerId = model.OwnerId;
                taskToEdit.ParentTaskId = model.ParentTaskId;
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
                if (model.AssignerIdInt > 0)
                {
                    taskToEdit.AssignerId = model.AssignerIdInt;
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
                    try
                    {
                        await this.NotificationAsync(taskToEdit.Id, oldActiveEmployeesIds);
                    }
                    catch (Exception)
                    {
                        TempData["Success"] = "Проблем с изпращането на уведомителни имейли. ";
                    }
                    TempData["Success"] = TempData["Success"] + "Промените са записани успешно";
                    return RedirectToAction(nameof(TaskDetails), new { taskId = model.Id });
                }
                else if(result == "halfsuccess")
                {
                    try
                    {
                        await this.NotificationAsync(taskToEdit.Id, oldActiveEmployeesIds);
                    }
                    catch (Exception)
                    {
                        TempData["Success"] = "Проблем с изпращането на уведомителни имейли. ";
                    }
                    TempData["Success"] = TempData["Success"] + "Промените са записани успешно, но началната дата е съобразена с първите отчетени часове!";
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
                TempData["Error"] = "[Service]Основна грешка. Неуспешен запис на промените.";
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

                if (model.TaskName.ToLower() == "отпуски" || model.TaskName.ToLower() == "отпуска" || model.TaskName.ToLower() == "болнични" || model.TaskName.ToLower() == "болничен")
                {
                    model = await TasksModelPrepareForViewWithOldInfo(model);
                    TempData["Error"] = "Името на задачата е заето за системни нужди!";
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
                newTask.EndDatePrognose = model.Valid_To.HasValue ? model.Valid_To.Value.Date : DateTime.Now.AddDays(10);
                newTask.OwnerId = currentUser.Id;
                newTask.ParentTaskId = model.ParentTaskId;
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
                if (model.AssignerIdInt > 0)
                {
                    newTask.AssignerId = model.AssignerIdInt;
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

                var taskId = await this.tasks.AddNewTaskAsync(newTask);

                if (taskId > 0)
                {
                    await this.NotificationAsync(taskId, EmailType.Create);
                    TempData["Success"] = "Задачата е създадена успешно";
                    return RedirectToAction(nameof(CreateNewTask));
                }
                else
                {
                    TempData["Error"] = $"[Service] Задачата не е създадена {taskId}";
                    return View(model);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Основна грешка. Моля проверете входните данни.";
                return View(model);
            }
        }

        public async Task<IActionResult> AddDateNote(int taskId, int userId, string taskName, DateTime workDate)
        {
            var currentNote = await this.tasks.GetTaskEmpNoteForDateAsync(taskId, userId, workDate);
            if (currentNote == null)
            {
                var model = new AddNoteToTaskServiceModel()
                {
                    TaskId = taskId,
                    EmployeeId = userId,
                    TaskName = taskName,
                    WorkDate = workDate.Date
                };
                return PartialView("_AddNoteModalPartial", model);
            }
            return PartialView("_AddNoteModalPartial", currentNote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDateNote(AddNoteToTaskServiceModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddNoteModalPartial", model);
            }
            bool result = await this.tasks.SetTaskEmpNoteForDateAsync(model);

            if (!result)
            {
                TempData["Error"] = "[AddDateNote] Сървиз грешка! Уведомете администратора.";
            }
            return PartialView("_AddNoteModalPartial", model);
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
            try
            {
                if (ModelState.IsValid)
                {
                    bool result = await this.tasks.CloseTaskAsync(model.TaskId, model.EndNote, currentUser.Id);
                    if (result)
                    {
                        await this.NotificationAsync(model.TaskId, EmailType.Close);
                        TempData["Success"] = "Задачата е приключена успешно!";
                    }
                    else
                    {
                        TempData["Error"] = "Сървиз грешка! Уведомете администратора.";
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Сървиз грешка! Уведомете администратора. {ex.Message}";
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

                ////////
                var data = await this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId);
                var sectors = data.GroupBy(x => x.SectorName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Group = sectors.Where(d => d.Name == item.SectorName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newTask.AssignersList = dropdownList;
                ///////
                newTask.Employees = this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId).Result
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

                ////////
                var data = await this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId);
                var sectors = data.GroupBy(x => x.SectorName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Group = sectors.Where(d => d.Name == item.SectorName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");

                newTask.AssignersList = dropdownList;

                ///////
                newTask.Employees = this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId).Result
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

                ////////
                var data = await this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId);
                var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");

                newTask.AssignersList = dropdownList;

                ///////

                newTask.Employees = this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId).Result
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

                ///////
                var data = this.employees.GetActiveEmployeesNames();
                //data.Append<SelectServiceModel>(new SelectServiceModel { Id = 0, TextValue = "Моля изберете..." });
                // Note - the -1 is needed at the end of this - pre-selected value is determined further down
                // Note .OrderBy() determines the order in which the groups are displayed in the dropdown
                var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");

                //newTask.AssignerIdInt = data.FirstOrDefault().Id;
                newTask.AssignersList = dropdownList;
                //////

                newTask.Employees = this.employees.GetActiveEmployeesNames()
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString()
                                                   })
                                                   .ToList();
            }

            if (currentUser.RoleName == SuperAdmin)
            {
                newTask.TaskParetns = this.tasks.GetParentTaskNames(currentUser.DirectorateId, true)
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = false
                   })
                   .ToList();
            }
            else
            {
                newTask.TaskParetns = this.tasks.GetParentTaskNames(currentUser.DirectorateId, false)
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = false
                   })
                   .ToList();

            }

            newTask.TaskParetns.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });

            newTask.TaskTypes = this.tasktypes.GetTaskTypesNames()
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = a.TextValue == TaskTypeGlobal ? true : false
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
                if (currentUser.DirectorateId != null)
                {
                    newTask.Directorates = this.directorates.GetDirectoratesNames(currentUser.DirectorateId)
                                                       .Select(a => new SelectListItem
                                                       {
                                                           Text = a.TextValue,
                                                           Value = a.Id.ToString(),
                                                           Selected = true
                                                       })
                                                       .ToList();
                    newTask.DirectoratesId = currentUser.DirectorateId.ToString();
                }
                else
                {
                    newTask.DirectoratesId = null;
                }

                //if (!string.IsNullOrWhiteSpace(oldTask.DirectoratesId) && newTask.Directorates.Where(d => d.Value == oldTask.DirectoratesId).FirstOrDefault() == null) //ако има дирекция, но не е от списъка. Пример: Ако задачата е от друга дирекция и др.
                //{
                //    var directorateIdInt = int.Parse(oldTask.DirectoratesId);
                //    newTask.Directorates.Add(new SelectListItem
                //    {
                //        Text = this.directorates.GetDirectoratesNames(directorateIdInt).Select(d => d.TextValue).FirstOrDefault(),
                //        Value = oldTask.DirectoratesId,
                //        Selected = true
                //    });
                //}

                newTask.DirectoratesId = newTask.Directorates.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();

                if (currentUser.DepartmentId != null)   //currentUser има departmentId
                {
                    newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString(),
                                   Selected = true
                               })
                               .ToList();
                    newTask.DepartmentsId = currentUser.DepartmentId.ToString();


                }
                else   //currentUser няма departmentId
                {
                    //newTask.Departments = this.departments.GetDepartmentsNamesByDirectorate(currentUser.DirectorateId)
                    //                                   .Select(a => new SelectListItem
                    //                                   {
                    //                                       Text = a.TextValue,
                    //                                       Value = a.Id.ToString(),
                    //                                       Selected = oldTask.DepartmentsId == a.Id.ToString() ? true : false
                    //                                   })
                    //                                    .ToList();
                    newTask.DepartmentsId = null;
                }

                //if (!string.IsNullOrWhiteSpace(oldTask.DepartmentsId) && newTask.Departments.Where(d => d.Value == oldTask.DepartmentsId).FirstOrDefault() == null) //ако има отдел, но не е от списъка с отдели от горните два селекта. Пример: Ако задачата е от друга дирекция и др.
                //{
                //    var departmentIdInt = int.Parse(oldTask.DepartmentsId);
                //        newTask.Departments.Add(new SelectListItem
                //        {
                //            Text = this.departments.GetDepartmentsNames(departmentIdInt).Select(d => d.TextValue).FirstOrDefault(),
                //            Value = oldTask.DepartmentsId,
                //            Selected = true
                //        });

                //}

                //newTask.DepartmentsId = newTask.Departments.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();

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
                        newTask.SectorsId = currentUser.SectorId.ToString();

                    ////////
                    var data = await this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId);
                    var sectors = data.GroupBy(x => x.SectorName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                    var dropdownList = new SelectList(data.Select(item => new SelectListItem
                    {
                        Text = item.TextValue,
                        Value = item.Id.ToString(),
                        Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                        Group = sectors.Where(d => d.Name == item.SectorName).FirstOrDefault()
                    }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                    newTask.AssignersList = dropdownList;
                    ///////

                    newTask.Employees = data.Select(a => new SelectListItem
                    {
                        Text = a.TextValue,
                        Value = a.Id.ToString(),
                        Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                    })
                                                       .ToList();
                }
                else
                {
                    ////////
                    IEnumerable<SelectServiceModel> data = new List<SelectServiceModel>();
                    if (currentUser.DepartmentId != null)
                    {
                        data = await this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId);
                    }
                    else if (currentUser.DirectorateId != null)
                    {
                        data = await this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId);
                    }
                    else
                    {
                        data = this.employees.GetActiveEmployeesNames();
                    }

                    var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                    var dropdownList = new SelectList(data.Select(item => new SelectListItem
                    {
                        Text = item.TextValue,
                        Value = item.Id.ToString(),
                        Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                        Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                    }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                    newTask.AssignersList = dropdownList;
                    ///////

                    newTask.Employees = data.Select(a => new SelectListItem
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
                ////////
                var data = await this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId);
                var sectors = data.GroupBy(x => x.SectorName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                    Group = sectors.Where(d => d.Name == item.SectorName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newTask.AssignersList = dropdownList;
                ///////

                newTask.Employees = data.Select(a => new SelectListItem
                {
                    Text = a.TextValue,
                    Value = a.Id.ToString(),
                    Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                }).ToList();
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
                ////////
                var data = await this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId);
                //var sectors = data.GroupBy(x => x.SectorName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                    //Group = sectors.Where(d => d.Name == item.SectorName).FirstOrDefault()
                    Group = new SelectListGroup { Name = currentUser.DepartmentName }
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newTask.AssignersList = dropdownList;
                ///////

                newTask.Employees = data.Select(a => new SelectListItem
                {
                    Text = a.TextValue,
                    Value = a.Id.ToString(),
                    Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                }).ToList();
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
                newTask.DirectoratesId = currentUser.DirectorateId.ToString();
                newTask.Departments = this.departments.GetDepartmentsNamesByDirectorate(currentUser.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = oldTask.DepartmentsId == a.Id.ToString() ? true : false
                                                   })
                                                    .ToList();
                if (oldTask.DepartmentsId == "0" || string.IsNullOrWhiteSpace(oldTask.DepartmentsId) || !this.departments.CheckDepartmentInDirectorate(currentUser.DirectorateId.Value,int.Parse(oldTask.DepartmentsId)))   //ако не е избран отдел или е избран отдел от друга дирекция
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
                else    //ако е избран отдел от дирекцията
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
                    if (oldTask.SectorsId == "0" || string.IsNullOrWhiteSpace(oldTask.SectorsId) ) //избран отдел , но не е избран сектор
                    {
                        newTask.Sectors.Insert(0, new SelectListItem
                        {
                            Text = ChooseValue,
                            Value = "0",
                            Selected = true
                        });
                    }
                    newTask.SectorsId = newTask.Sectors.Where(t => t.Selected == true).Select(t => t.Value).FirstOrDefault();
                }
                ////////
                var data = await this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId);
                var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                    Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newTask.AssignersList = dropdownList;
                ///////

                newTask.Employees = data.Select(a => new SelectListItem
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

                if (oldTask.DirectoratesId == "0" || string.IsNullOrWhiteSpace(oldTask.DirectoratesId)) //ако не е избрана дирекция
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
                ////////
                var data = this.employees.GetActiveEmployeesNames();
                var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
                var dropdownList = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = (oldTask.AssignerIdInt == item.Id) ? true : false,
                    Group = departments.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                newTask.AssignersList = dropdownList;
                ///////

                newTask.Employees = data.Select(a => new SelectListItem
                {
                    Text = a.TextValue,
                    Value = a.Id.ToString(),
                    Selected = newTask.EmployeesIds.Contains(a.Id) ? true : false
                }).ToList();
            }
            newTask.AssignerIdInt = oldTask.AssignerIdInt;
            newTask.TaskParetns = this.tasks.GetParentTaskNames(currentUser.DirectorateId, true)
                   .Select(a => new SelectListItem
                   {
                       Text = a.TextValue,
                       Value = a.Id.ToString(),
                       Selected = a.Id == oldTask.ParentTaskId ? true : false
                   })
                   .ToList();
             newTask.TaskParetns.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = oldTask.ParentTaskId > 0 ? false : true
            });
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
            try
            {
                var result = await this.tasks.ReopenTaskAsync(taskId);
                TempData["Success"] = "Задачата е отворена успешно!";
                return RedirectToAction(nameof(TaskDetails), new { taskId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"[ReopenTask] Сървиз грешка! Уведомете администратора. {ex.Message}";
                return RedirectToAction(nameof(TaskDetails), new { taskId });
            }
        }

        public async Task<IActionResult> UndeleteTask(int taskId)
        {
            try
            {
                var result = await this.tasks.MarkTaskActiveAsync(taskId, currentUser.Id);
                TempData["Success"] = "Задачата е възстановена успешно!";
                return RedirectToAction(nameof(TaskDetails), new { taskId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"[UndeleteTask] Сървиз грешка! Уведомете администратора. {ex.Message}";
                return RedirectToAction(nameof(TaskDetails), new { taskId });
            }
        }

        private async Task<IEnumerable<SelectServiceModel>> GetEmployeesByUserRoleAsync(bool isAll)
        {
            IEnumerable<SelectServiceModel> data = new List<SelectServiceModel>();
            if (isAll)   //ако са нужни всички служители
            {
                data = this.employees.GetActiveEmployeesNames();
            }
            else   //ако НЕ са нужни всички служители
            {
                if (currentUser.RoleName == DataConstants.SuperAdmin)
                {
                    data = this.employees.GetActiveEmployeesNames();
                }
                else if (currentUser.RoleName == DataConstants.SectorAdmin)
                {
                    data = await this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId);
                }
                else if (currentUser.RoleName == DataConstants.DepartmentAdmin)
                {
                    data = await this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId);
                }
                else if (currentUser.RoleName == DataConstants.DirectorateAdmin)
                {
                    data = await this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId);
                }
                else   // ако е Employee
                {
                    if (currentUser.SectorId != null)
                    {
                        data = await this.employees.GetEmployeesNamesBySectorAsync(currentUser.SectorId);
                    }
                    else if (currentUser.DepartmentId != null)
                    {
                        data = await this.employees.GetEmployeesNamesByDepartmentAsync(currentUser.DepartmentId);
                    }
                    else if (currentUser.DirectorateId != null)
                    {
                        data = await this.employees.GetEmployeesNamesByDirectorateAsync(currentUser.DirectorateId);
                    }
                    else
                    {
                        data = this.employees.GetActiveEmployeesNames();
                    }
                }
            }
            return data;
        }

        private async Task<SelectList> GetEmployeesOnTask(bool isAll, int? taskId)
        {
            var employeesIds = new List<int>();  // номерата на експерти работещи по задачата  (ако има такива)
            var taskEmployees = new SelectList(new List<SelectListItem>(), "Value", "Text", "-1", "Group.Name");
            var data = await this.GetEmployeesByUserRoleAsync(isAll);  // колекция от експерти според ролята на потребителя
                                                                       // var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
            var departmentlist = this.departments.GetDepartmentsNames();
            var depNameGroups = departmentlist.GroupBy(x => x.TextValue).Select(x => new SelectListGroup { Name = x.Key }).ToList();

            if (taskId.HasValue)
            {

                var taskDetails = this.tasks.GetTaskDetails(taskId.Value)
                                             .ProjectTo<TaskViewModel>()
                                             .FirstOrDefault();
                var assignedEmployees = new List<SelectServiceModel>();
                assignedEmployees.AddRange(taskDetails.Colleagues.ToList());
                employeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToList(); //за да изключи премахнатите експерти
                taskEmployees = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = employeesIds.Contains(item.Id) ? true : false,
                    Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == item.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
                employeesIds = assignedEmployees.Select(a => a.Id).ToList(); //за да включи всички експерти работили по задачата(изтрити и такива дето не са в йерархията на експерта)
                if (!employeesIds.All(elem => taskEmployees.Select(e => int.Parse(e.Value)).ToArray().Contains(elem))) //добавям членовете на задачата, които не са в йерархиата на отговорника
                {
                    var tempSelectListItems = new List<SelectListItem>();
                    foreach (var empId in employeesIds)
                    {
                        if (taskEmployees.FirstOrDefault(e => e.Value == empId.ToString()) == null)
                        {
                            var curentEmployee = assignedEmployees.Where(e => e.Id == empId).FirstOrDefault();
                            var employeeFromDB = await this.employees.GetEmployeeByIdAsync(empId);
                            if (employeeFromDB.isDeleted == false)   //проверка дали колегата, който е вкл. в задачата, междувременно не е маркиран като изтрит акаунт
                            {
                                tempSelectListItems.Add(new SelectListItem
                                {
                                    Text = curentEmployee.TextValue,
                                    Value = empId.ToString(),
                                    Selected = curentEmployee.isDeleted ? false : true,
                                    Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == curentEmployee.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
                                });
                            }
                        }
                    }
                    if (tempSelectListItems.Count > 0)
                    {
                        List<SelectListItem> _assignersList = taskEmployees.Select(asl => new SelectListItem
                        {
                            Text = asl.Text,
                            Value = asl.Value,
                            Selected = false,
                            Group = new SelectListGroup { Name = asl.Group.Name }
                        }).ToList();
                        _assignersList = _assignersList.Concat(tempSelectListItems).ToList();
                        taskEmployees = new SelectList(_assignersList, "Value", "Text", "-1", "Group.Name");
                    }
                }
            }
            else
            {
                taskEmployees = new SelectList(data.Select(item => new SelectListItem
                {
                    Text = item.TextValue,
                    Value = item.Id.ToString(),
                    Selected = false,
                    Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == item.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
                }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");

            }
            return taskEmployees;
        }

        #region API Calls

        //[Authorize(Policy = DataConstants.Employee)]               
        [HttpGet]
        public async Task<IActionResult> GetEmployees(bool isAll, int? taskId)
        {
         //   var employeesIds = new List<int>();  // номерата на експерти работещи по задачата  (ако има такива)
         //   //var taskEmployees = new List<SelectListItem>();
         //   var taskEmployees = new SelectList(new List<SelectListItem>(), "Value", "Text", "-1", "Group.Name");
         //   var data = await this.GetEmployeesByUserRoleAsync(isAll);  // колекция от експерти според ролята на потребителя
         //// var departments = data.GroupBy(x => x.DepartmentName).Select(x => new SelectListGroup { Name = x.Key }).ToList();
         //   var departmentlist = this.departments.GetDepartmentsNames();
         //   var depNameGroups = departmentlist.GroupBy(x => x.TextValue).Select(x => new SelectListGroup { Name = x.Key }).ToList();
         //   //depNameGroups.Add(new SelectListGroup { Name = "0_Служители без присвоен отдел" });

         //   if (taskId.HasValue)
         //   {
                
         //       var taskDetails = this.tasks.GetTaskDetails(taskId.Value)
         //                                    .ProjectTo<TaskViewModel>()
         //                                    .FirstOrDefault();
         //       var assignedEmployees = new List<SelectServiceModel>();
         //       assignedEmployees.AddRange(taskDetails.Colleagues.ToList());
         //       employeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToList(); //за да изключи премахнатите експерти
         //       //taskEmployees = data.Select(a => new SelectListItem
         //       //{
         //       //    Text = a.TextValue,
         //       //    Value = a.Id.ToString(),
         //       //    Selected = employeesIds.Contains(a.Id) ? true : false
         //       //})
         //       //.ToList();
         //       taskEmployees = new SelectList(data.Select(item => new SelectListItem
         //       {
         //           Text = item.TextValue,
         //           Value = item.Id.ToString(),
         //           Selected = employeesIds.Contains(item.Id) ? true : false,
         //           Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == item.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
         //           // Group = depNameGroups.Where(d => d.Name == item.DepartmentName).FirstOrDefault()
         //       }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");
         //       employeesIds = assignedEmployees.Select(a => a.Id).ToList(); //за да включи всички експерти работили по задачата(изтрити и такива дето не са в йерархията на експерта)
         //       if (!employeesIds.All(elem => taskEmployees.Select(e => int.Parse(e.Value)).ToArray().Contains(elem))) //добавям членовете на задачата, които не са в йерархиата на отговорника
         //       {
         //           var tempSelectListItems = new List<SelectListItem>();
         //           foreach (var empId in employeesIds)
         //           {
         //               if (taskEmployees.FirstOrDefault(e => e.Value == empId.ToString()) == null)
         //               {
         //                   var curentEmployee = assignedEmployees.Where(e => e.Id == empId).FirstOrDefault();
         //                   var employeeFromDB = await this.employees.GetEmployeeByIdAsync(empId);
         //                   if (employeeFromDB.isDeleted == false)   //проверка дали колегата, който е вкл. в задачата, междувременно не е маркиран като изтрит акаунт
         //                   {
         //                       tempSelectListItems.Add(new SelectListItem
         //                       {
         //                           Text = curentEmployee.TextValue,
         //                           Value = empId.ToString(),
         //                           Selected = curentEmployee.isDeleted ? false : true,
         //                           Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == curentEmployee.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
         //                       });

         //                       //taskEmployees.Append(new SelectListItem
         //                       //{
         //                       //    Text = curentEmployee.TextValue,
         //                       //    Value = empId.ToString(),
         //                       //    Selected = curentEmployee.isDeleted ? false : true,
         //                       //    Group = departments.Where(d => d.Name == curentEmployee.DepartmentName).FirstOrDefault()
         //                       //});
         //                   }
         //               }
         //           }
         //           if (tempSelectListItems.Count > 0)
         //           {
         //               List<SelectListItem> _assignersList = taskEmployees.Select(asl => new SelectListItem
         //               {
         //                   Text = asl.Text,
         //                   Value = asl.Value,
         //                   Selected = false,
         //                   Group = new SelectListGroup { Name = asl.Group.Name }
         //               }).ToList();
         //               _assignersList = _assignersList.Concat(tempSelectListItems).ToList();
         //               taskEmployees = new SelectList(_assignersList, "Value", "Text", "-1", "Group.Name");
         //           }
         //       }
         //   }
         //   else
         //   {
         //      taskEmployees = new SelectList(data.Select(item => new SelectListItem
         //       {
         //           Text = item.TextValue,
         //           Value = item.Id.ToString(),
         //           Selected = false,
         //           Group = new SelectListGroup { Name = (depNameGroups.Where(d => d.Name == item.DepartmentName).Select(d => d.Name).FirstOrDefault() ?? "0_Служители без присвоен отдел") }
         //      }).OrderBy(a => a.Group.Name).ToList(), "Value", "Text", "-1", "Group.Name");

         //   }

            var taskEmployees = await this.GetEmployeesOnTask(isAll,taskId);
            return Json(new { taskEmployees });
        }   

        [HttpGet]
        public async Task<IActionResult> SetDateTasksHours(int userId, DateTime workDate, int taskId, int hours)
        {
            try
            {
                if (userId != currentUser.Id)
                {
                    var dominions = await this.employees.GetUserDominions(currentUser.Id);
                    if (!dominions.Any(d => d.Id == userId))
                    {
                        var targetEmployee = await this.employees.GetEmployeeByIdAsync(userId);
                        return Json(new { success = false, message = $"[SetDateTasksHours]. {currentUser.FullName} не е представител на {targetEmployee.FullName} "});
                    }
                }


                var workedHours = new TaskWorkedHoursServiceModel()
                {
                    EmployeeId = userId,
                    TaskId = taskId,
                    HoursSpend = hours,
                    WorkDate = workDate.Date
                };

                string result = await this.tasks.SetWorkedHoursAsync(workedHours);
                if (result == "success")
                {
                    return Json(new { success = true, message = ("Часовете са отразени успешно" + Environment.NewLine) });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Основна грешка. Моля проверете логиката на входните данни." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidayDates(int userId)
        {
          var data = await this.tasks.GetHolidayDatesAsync(userId, "Отпуски");
          return Json(new { data });
        }
        [HttpGet]
        public async Task<IActionResult> GetIlldayDates(int userId)
        {
            var data = await this.tasks.GetHolidayDatesAsync(userId, "Болнични");
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> RemoveSystemTasks(int userId, DateTime workDate)
        {
            try
            {
                if (userId != currentUser.Id)
                {
                    var dominions = await this.employees.GetUserDominions(currentUser.Id);
                    if (!dominions.Any(d => d.Id == userId))
                    {
                        var targetEmployee = await this.employees.GetEmployeeByIdAsync(userId);
                        return Json(new { success = false, message = $"[SetDateTasksHours]. {currentUser.FullName} не е представител на {targetEmployee.FullName} " });
                    }
                }
                bool result = await this.tasks.RemoveSystemTaskForDate(userId, workDate);
                if (result)
                {
                    return Json(new { success = true, message = ("Премахната е системна задача болничен/отпуск" + Environment.NewLine) });
                }
                else
                {
                    return Json(new { success = false, message = "[RemoveSystemTasks / RemoveSystemTaskForDate] грешка." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "[RemoveSystemTasks / RemoveSystemTaskForDate] грешка. Основна грешка при премахване на системен тип задачи" + Environment.NewLine });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SetDateSystemTasks(int userId, DateTime workDate, bool isholiday = false, bool isill = false)
        {
            try
            {
                if (userId != currentUser.Id)
                {
                    var dominions = await this.employees.GetUserDominions(currentUser.Id);
                    if (!dominions.Any(d => d.Id == userId))
                    {
                        var targetEmployee = await this.employees.GetEmployeeByIdAsync(userId);
                        return Json(new { success = false, message = $"[SetDateTasksHours]. {currentUser.FullName} не е представител на {targetEmployee.FullName} " });
                    }
                }
                var result = string.Empty;
                var message = string.Empty;
                if (isholiday || isill)
                {
                    var dateTaskList = await this.employees.GetAllUserTaskAsync(userId, workDate.Date);
                    foreach (var itemTask in dateTaskList)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = itemTask.Id,
                            HoursSpend = 0,
                            WorkDate = workDate.Date
                        };

                        await this.tasks.SetWorkedHoursWithDeletedAsync(workedHours);
                    }
                    if (isholiday)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = await this.tasks.GetSystemTaskIdByNameAsync("Отпуски"),
                            HoursSpend = 8,
                            WorkDate = workDate.Date
                        };
                      result = await this.tasks.SetWorkedHoursAsync(workedHours);
                        message = "Отпускът е отразен в системата";
                    }
                    else if (isill)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = await this.tasks.GetSystemTaskIdByNameAsync("Болнични"),
                            HoursSpend = 8,
                            WorkDate = workDate.Date
                        };
                      result = await this.tasks.SetWorkedHoursAsync(workedHours);
                        message = "Болничния е отразен в системата";
                    }

                }
                if (result == "success")
                {
                    return Json(new { success = true, message = (message + Environment.NewLine) });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Основна грешка. Моля проверете логиката на входните данни." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool withClosed = false, bool withDeleted = false)
        {
            var data = this.tasks.GetAllTasks(currentUser.Id, withClosed, withDeleted)
                .ProjectTo<TasksListViewModel>()
                .ToList();
            foreach (var task in data)
            {
                task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
            }

            return Json(new { data });
        }

        [Authorize(Policy = SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> Delete(int taskId)
        {
            
            var taskFromDb = await this.tasks.CheckTaskByIdAsync(taskId);
            if (!taskFromDb)
            {
                return Json(new { success = false, message = "Грешка при изтриване" });
            }
            string result = await this.tasks.MarkTaskDeletedAsync(taskId, currentUser.Id);
            if (result != "success")
            {
                return Json(new { success = false, message = $"[Грешка при изтриване] {result}" });
            }
            return Json(new { success = result, message = "Задачата е изтрита" });
        }

        [Authorize(Policy = SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> TotalDelete(int taskId)
        {

            var taskFromDb = await this.tasks.CheckTaskByIdAsync(taskId);
            if (!taskFromDb)
            {
                return Json(new { success = false, message = $"Грешка при изтриване! Няма задача с номер: {taskId}" });
            }

            try
            {
                var dirDelResult = this.files.DeleteTaskDirectory(taskId);
                if (dirDelResult)
                {
                    taskFromDb = await this.tasks.TotalTaskDeletedAsync(taskId);
                    if (taskFromDb)
                    {
                        return Json(new { success = true, message = "Задачата е тотално изтрита" });
                    }
                    else
                    {
                        return Json(new { success = false, message = $"Грешка при изтриване на задачата {taskId} от DB" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"Грешка при изтриване на файловете по задача {taskId}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"[Грешка при изтриване] {ex.Message}" });
            }

        }

        public async Task<IActionResult> GetDateWorkedHours(DateTime searchedDate, int userId)
        {
            var data = await this.employees.GetDateReport(userId, searchedDate);
            return Json(new { data });
        }


        #endregion
    }
}
