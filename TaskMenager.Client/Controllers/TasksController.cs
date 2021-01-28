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
using TaskMenager.Client.Models.Tasks;
using static TaskManager.Common.DataConstants;

namespace TaskMenager.Client.Controllers
{
    
    public class TasksController : Controller
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly ITaskTypesService tasktypes;
        private readonly ITaskPrioritysService taskprioritys;
        private readonly IEmployeesService employees;
        private readonly IStatusService statuses;
        private readonly ITasksService tasks;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserServiceModel currentUser;
        public TasksController(IDirectorateService directorates, IEmployeesService employees, IDepartmentsService departments, ISectorsService sectors, ITaskTypesService tasktypes, ITaskPrioritysService taskprioritys, IHttpContextAccessor httpContextAccessor, IStatusService statuses, ITasksService tasks)
        {
            this.tasks = tasks;
            this.statuses = statuses;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.tasktypes = tasktypes;
            this.taskprioritys = taskprioritys;
            this.employees = employees;
            this._httpContextAccessor = httpContextAccessor;
            currentUser = this.employees.GetUserDataForCooky(_httpContextAccessor?.HttpContext?.User?.Identities.FirstOrDefault().Name.ToLower());

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
                TempData["Error"] = "Грешка при създаване на новата задача.";
                return RedirectToAction("NotAuthorized");
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
                newTask.Directorates.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.Departments.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });
                newTask.Sectors = this.sectors.GetSectorsNames(currentUser.SectorId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.Sectors.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });
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
                newTask.Directorates.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });
                newTask.Departments = this.departments.GetDepartmentsNames(currentUser.DepartmentId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue,
                                                       Value = a.Id.ToString(),
                                                       Selected = true
                                                   })
                                                   .ToList();
                newTask.Departments.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });
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
                newTask.Directorates.Insert(0, new SelectListItem
                {
                    Text = ChooseValue,
                    Value = "0"
                });

                
                newTask.Departments = this.departments
                                                   .GetDepartmentsNamesByDirectorate(currentUser.DirectorateId)
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

            newTask.TaskPrioritys = this.taskprioritys.GetTaskPrioritysNames()
                               .Select(a => new SelectListItem
                               {
                                   Text = a.TextValue,
                                   Value = a.Id.ToString()
                                   //Selected = a.TextValue == TaskPriorityNormal ? true : false
                               })
                               .ToList();
            newTask.TaskPrioritys.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = "0",
                Selected = true
            });

            return newTask;
        }

        [HttpPost]
        //public IActionResult CreateNewTask(AddNewTaskViewModel model, int? directorateId, int? departmentId, int? sectorId, int? priorityId, int? hourslimit, string assignerId)
        public async Task<IActionResult> CreateNewTask(AddNewTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Valid_From > model.Valid_To)
            {
                TempData["Error"] = "Невалидни дати";
                return View(model);
            }

            AddNewTaskServiceModel newTask = new AddNewTaskServiceModel();
            
            newTask.Name = model.TaskName;
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

            return View(model);







            //return RedirectToAction("CreateNewTask");
            //if (!ModelState.IsValid || !directorateId.HasValue || !priorityId.HasValue || !hourslimit.HasValue)
            //{

            //        return View(model);
            //}



            //teacher.DateTimeInLocalTime = DateTime.Now;  
            //teacher.DateTimeInUtc = DateTime.UtcNow;  
            //if (model.SubjectsIds.Length > 0)  
            //{  
            //    foreach (var subjectid in model.SubjectsIds)  
            //    {  
            //        teacherSubjects.Add(new TeacherSubjects { SubjectId = subjectid, TeacherId = model.Id });  
            //    }  
            //    teacher.TeacherSubjects = teacherSubjects;  
            //}  
            //db.Teacher.Add(teacher);  
            //db.SaveChanges();  

            return RedirectToAction("index");  
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
