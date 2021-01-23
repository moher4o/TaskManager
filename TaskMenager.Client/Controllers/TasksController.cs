﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserServiceModel currentUser;
        public TasksController(IDirectorateService directorates, IEmployeesService employees, IDepartmentsService departments, ISectorsService sectors, ITaskTypesService tasktypes, ITaskPrioritysService taskprioritys, IHttpContextAccessor httpContextAccessor)
        {
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
        public IActionResult CreateNewTask()
        {
            try
            {
                var newTask = new AddNewTaskViewModel();

                newTask = TasksModelPrepareForView(newTask);

                return View(newTask);
            }
            catch (Exception)
            {
                TempData["Error"] = "Грешка при създаване на новата задача.";
                return RedirectToAction("NotAuthorized");
            }

        }

        private AddNewTaskViewModel TasksModelPrepareForView(AddNewTaskViewModel newTask)
        {
            List<long> subjectsIds = new List<long>();
            newTask.SubjectsIds = subjectsIds.ToArray();
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

            newTask.TaskPrioritys = this.taskprioritys.GetTaskPrioritysNames()
                               .Select(a => new SelectListItem
                               {
                                   Text = a,
                                   Value = a
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
        public IActionResult CreateNewTask(AddNewTaskViewModel model, int? directorateId, int? priority, int? hourslimit)
        {
            AddNewTaskServiceModel newTask = new AddNewTaskServiceModel();
            List<EmployeesTasks> taskSubjects = new List<EmployeesTasks>();
            if (model.SubjectsIds != null)
            {
            }

            if (!ModelState.IsValid || directorateId.HasValue)
            {
                
                    return View(model);
            }


            //newTask.Name = model.TaskName;
            //newTask.Description = model.Description;
            //newTask.StartDate = model.Valid_From;
            //newTask.EndDatePrognose = model.Valid_To;
            //newTask.OwnerId = currentUser.Id;
            //newTask.DirectorateId = directorateId;
            //newTask.DepartmentId = departmentId;
            
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
    }
}
