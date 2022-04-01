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
        public async Task<List<SimpleTask>> Get()
        {
            int identityId = 1;
            DateTime dateToProcess = DateTime.Now.Date;
            var emptasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date);
            var result = new List<SimpleTask>();
            int count = 1;
            foreach (var itemTask in emptasks.Where(at => at.TaskStatusName == DataConstants.TaskStatusClosed))
            {
                var item = new SimpleTask()
                {
                    Id = itemTask.Id,
                    Name = "Еспресо "+count.ToString(),
                    Roaster = itemTask.TaskStatusName,
                    Image = "coffeebag.png"
                };
                result.Add(item);
                count++;
            }
            return result;
        }
    }
}
