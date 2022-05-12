using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private readonly IDateManagementConfiguration dateConfiguration;

        public WorkController(IDateManagementConfiguration _dateConfiguration, IEmployeesService _employees, ITasksService _tasks)
        {
            this.tasks = _tasks;
            this.employees = _employees;
            this.dateConfiguration = _dateConfiguration;
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
        public async Task<IActionResult> Post([FromQuery] string userSecretKey, [FromBody] AuthTaskUpdate requestMob)
        {
            try
            {
                var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
                var user = this.employees.GetUserDataForCooky(username);
                var systemTaskList = await this.tasks.GetSystemTasksAsync();
                //var inTime = requestMob.workDate.Date < DateTime.Now.Date.AddDays(-7) ? false : true;
                
                string result = String.Empty;
                if (systemTaskList.Any(st => st.Id == requestMob.taskId))
                {
                    var systemTaskName = systemTaskList.Where(st => st.Id == requestMob.taskId).Select(st => st.TextValue).FirstOrDefault();
                    if (systemTaskName == "Отпуски")
                    {
                        result = await this.SetDateSystemTasks(user.Id, requestMob.workDate.Date, true, false);
                    }
                    else if (systemTaskName == "Болнични")
                    {
                        result = await this.SetDateSystemTasks(user.Id, requestMob.workDate.Date, false, true);
                    }
                }
                else
                {
                    var inTime = this.CheckDate(requestMob.workDate.Date);
                    var workedHours = new TaskWorkedHoursServiceModel()
                    {
                        EmployeeId = user.Id,
                        TaskId = requestMob.taskId,
                        HoursSpend = requestMob.hoursSpend,
                        WorkDate = requestMob.workDate.Date,
                        RegistrationDate = DateTime.Now.Date,
                        InTimeRecord = inTime
                    };

                    result = await this.tasks.SetWorkedHoursAsync(workedHours);
                }

                if (result == "success")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Database not updated");
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        private async Task<string> SetDateSystemTasks(int userId, DateTime workDate, bool isholiday = false, bool isill = false)
        {
            try
            {
                var result = string.Empty;
                var message = string.Empty;
                if (isholiday || isill)
                {
                    var dateTaskList = await this.employees.GetAllUserTaskAsync(userId, workDate.Date);
                    var inTime = CheckDate(workDate.Date);
                    foreach (var itemTask in dateTaskList)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = itemTask.Id,
                            HoursSpend = 0,
                            WorkDate = workDate.Date,
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
                            WorkDate = workDate.Date,
                            RegistrationDate = DateTime.Now.Date,
                            InTimeRecord = inTime,
                            Approved = false
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
                            WorkDate = workDate.Date,
                            RegistrationDate = DateTime.Now.Date,
                            InTimeRecord = inTime,
                            Approved = false
                        };
                        result = await this.tasks.SetWorkedHoursAsync(workedHours);
                        message = "Болничния е отразен в системата";
                    }

                }
                return result;
            }
            catch (Exception)
            {
                return "exception";
            }
        }


        private bool CheckDate(DateTime workDate)   //проверява дали датата за която ще се прави отчет е в текущия отчетен период
        {
            var daysAfterOtchet = new List<int>();
            var ot4etday = this.dateConfiguration.ReportDate;
            var todayDate = DateTime.Now.Date;
            var todayDayOfWeek = ((int)todayDate.DayOfWeek);
            var diference = todayDate.Date - workDate.Date;
            //int diferenceValue = todayDayOfWeek > ot4etday ? 8 : 7;

            if (diference.TotalDays < 0)
            {
                return true;
            }

            if (diference.TotalDays <= 7)
            {
                if (diference.TotalDays < 7)
                {
                    for (int i = ot4etday + 1; i <= ((todayDayOfWeek < (ot4etday + 1)) ? (todayDayOfWeek + 7) : todayDayOfWeek); i++)
                    {
                        daysAfterOtchet.Add(i > 6 ? i - 7 : i);
                    }
                }
                if (todayDayOfWeek == ot4etday + 1 || (todayDayOfWeek == 0 && ot4etday == 7))    //за да може в деня след отчета да се попълва за миналата седмица (петък е примерен)
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        if (!daysAfterOtchet.Contains(i))
                        {
                            daysAfterOtchet.Add(i);
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            if (daysAfterOtchet.Contains(((int)workDate.DayOfWeek)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
