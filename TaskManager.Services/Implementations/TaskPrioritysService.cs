using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Data;

namespace TaskManager.Services.Implementations
{
    public class TaskPrioritysService : ITaskPrioritysService
    {
        private readonly TasksDbContext db;
        public TaskPrioritysService(TasksDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<string> GetTaskPrioritysNames()
        {
            var names = this.db.Priorities.Where(c => c.isDeleted == false).Select(c => c.PriorityName).ToList();
            return names;
        }
    }
}
