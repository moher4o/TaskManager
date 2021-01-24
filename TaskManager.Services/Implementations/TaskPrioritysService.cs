using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Data;
using TaskManager.Services.Models;

namespace TaskManager.Services.Implementations
{
    public class TaskPrioritysService : ITaskPrioritysService
    {
        private readonly TasksDbContext db;
        public TaskPrioritysService(TasksDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<SelectServiceModel> GetTaskPrioritysNames()
        {
            var names = this.db.Priorities
                .Where(c => c.isDeleted == false)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.PriorityName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }
    }
}
