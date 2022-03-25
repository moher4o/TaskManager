using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            foreach (var itemTask in emptasks)
            {
                var item = new SimpleTask()
                {
                    Id = itemTask.Id,
                    Name = itemTask.TaskName,
                    Roaster = itemTask.TaskTypeName,
                    Image = "coffeebag.png"
                };
                result.Add(item);
            }
            return result;
        }
    }
}
