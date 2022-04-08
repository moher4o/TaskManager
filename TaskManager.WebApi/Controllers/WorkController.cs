using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.WebApi.Models;

namespace TaskManager.WebApi.Controllers
{
    [Route("api/[Controller]")]
    public class WorkController : Controller
    {
        protected readonly IEmployeesService employees;

        public WorkController(IEmployeesService _employees)
        {
            this.employees = _employees;
        }

        [HttpGet]
        public async Task<List<TaskApiModel>> Get()
        {
            int identityId = 1;
            DateTime dateToProcess = DateTime.Now.Date;
            var emptasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date);
            var result = new List<TaskApiModel>();
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
    }
}
