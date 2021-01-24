using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Data;
using TaskManager.Services.Models;

namespace TaskManager.Services.Implementations
{
    public class TaskTypesService : ITaskTypesService
    {
        private readonly TasksDbContext db;
        public TaskTypesService(TasksDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<SelectServiceModel> GetTaskTypesNames()
        {
            var names = this.db.TasksTypes
                .Where(c => c.isDeleted == false)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.TypeName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }
    }
}
