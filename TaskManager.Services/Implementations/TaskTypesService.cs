using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Data;

namespace TaskManager.Services.Implementations
{
    public class TaskTypesService : ITaskTypesService
    {
        private readonly TasksDbContext db;
        public TaskTypesService(TasksDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<string> GetTaskTypesNames()
        {
            var names = this.db.TasksTypes.Where(c => c.isDeleted == false).Select(c => c.TypeName).ToList();
            return names;
        }
    }
}
