using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models.TaskModels;
using TaskManager.WebApi.Models;

namespace TaskManager.WebApi.Controllers
{
    
    public class WorkController : BaseController
    {
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;

        public WorkController(IEmployeesService _employees, ITasksService _tasks)
        {
            this.tasks = _tasks;
            this.employees = _employees;
        }

        [HttpGet]
        public async Task<List<TaskApiModel>> Get([FromQuery] string userSecretKey, [FromQuery] DateTime workdate)
        {
            var result = new List<TaskApiModel>();
            var nulltask = new TaskApiModel()             //за информация, ако няма параметри
            {
                Id = 1,
                TaskName = "Не са подадени валидни параметри",
                TaskStatusName = "работи се",
                EmployeeHoursToday = 0,
                TaskPriorityName = "нисък",
                ApprovedByAdmninName = "Ангел Вуков",
                ApprovedToday = true,
                ChildrenCount = 0,
                EmployeeHours = 0,
                EndDate = DateTime.Now.Date,
                EndDatePrognose = DateTime.Now.Date,
                FilesCount = 0,
                HoursLimit = 0,
                NotesCount = 0,
                ParentTaskId = 0,
                TaskNoteForToday = "",
                TaskTypeName = DataConstants.TaskTypeSystem
            };


            if (string.IsNullOrWhiteSpace(userSecretKey))
            {
                nulltask.TaskName = "Невалидни параметри - потребителско име";
                result.Add(nulltask);
                return result;
            }
            else if (workdate.Date < DateTime.Today.AddYears(-30))
            {
                nulltask.TaskName = "Невалидни параметри - дата";
                result.Add(nulltask);
                return result;
            }

            var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
            if (string.IsNullOrWhiteSpace(username))
            {
                nulltask.TaskName = "Невалидни параметри - Няма такъв потребител или е неактивен";
                result.Add(nulltask);
                return result;
            }

            var emptasks = await this.employees.GetUserActiveTaskAsync(username, workdate.Date);
            
            foreach (var itemTask in emptasks.Where(at => at.TaskStatusName == DataConstants.TaskStatusInProgres))
            {
                var item = new TaskApiModel()
                {
                    Id = itemTask.Id,
                    TaskName = itemTask.TaskName,
                    TaskStatusName = itemTask.TaskStatusName,
                    EmployeeHoursToday = itemTask.EmployeeHoursToday,
                    TaskPriorityName = itemTask.TaskPriorityName,
                    ApprovedByAdmninName = itemTask.ApprovedByAdmninName,
                    ApprovedToday = itemTask.ApprovedToday,
                    ChildrenCount = itemTask.ChildrenCount,
                    EmployeeHours = itemTask.EmployeeHours,
                    EndDate = itemTask.EndDate,
                    EndDatePrognose = itemTask.EndDatePrognose,
                    FilesCount = itemTask.FilesCount,
                    HoursLimit = itemTask.HoursLimit,
                    NotesCount = itemTask.NotesCount,
                    ParentTaskId = itemTask.ParentTaskId,
                    TaskNoteForToday = itemTask.TaskNoteForToday,
                    TaskTypeName = itemTask.TaskTypeName
                };
                result.Add(item);
                
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Post(AuthTaskUpdate requestMob)
        {
            return Ok();
            //try
            //{

            //    var inTime = workDate.Date < DateTime.Now.Date.AddDays(-7) ? false : true; 
            //    var workedHours = new TaskWorkedHoursServiceModel()
            //    {
            //        EmployeeId = userId,
            //        TaskId = taskId,
            //        HoursSpend = hours,
            //        WorkDate = workDate.Date,
            //        RegistrationDate = DateTime.Now.Date,
            //        InTimeRecord = inTime
            //    };

            //    string result = await this.tasks.SetWorkedHoursAsync(workedHours);
            //    if (result == "success")
            //    {
            //        return Ok();
            //        //return Json(new { success = true, message = ("Часовете са отразени успешно" + inTime.ToString() + Environment.NewLine) });
            //    }
            //    else
            //    {
            //        return BadRequest("Database not updated");
            //    }

            //}
            //catch (Exception)
            //{
            //    return BadRequest();
            //}
        }
    }
}
