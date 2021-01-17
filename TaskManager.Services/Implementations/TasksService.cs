using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Data.Models;

namespace TaskManager.Services.Implementations
{
    public class TasksService : ITasksService
    {
        private readonly TasksDbContext db;
        public TasksService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> CreateTasksStatusesAsync()
        {
            await this.db.Database.BeginTransactionAsync();
            try
            {
                var statuses = this.db.TasksStatuses.ToList();
                this.db.TasksStatuses.RemoveRange(statuses);
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при изтриването на статусите.";
            }
            this.db.Database.CommitTransaction();

            await this.db.Database.BeginTransactionAsync();
            try
            {

                var newStatusDB = new TasksStatus()
                {
                    
                    Name = DataConstants.TaskStatusNew,
                    isDeleted = false,
                };
                await this.db.TasksStatuses.AddAsync(newStatusDB);
                newStatusDB = new TasksStatus()
                {

                    Name = DataConstants.TaskStatusInProgres,
                    isDeleted = false,
                };
                await this.db.TasksStatuses.AddAsync(newStatusDB);
                newStatusDB = new TasksStatus()
                {

                    Name = DataConstants.TaskStatusClosed,
                    isDeleted = false,
                };
                await this.db.TasksStatuses.AddAsync(newStatusDB);

                await this.db.SaveChangesAsync();

             }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при създаването на статусите.";
            }
            this.db.Database.CommitTransaction();
            return "success";
        }

        public int TasksStatusCount()
        {
            return this.db.TasksStatuses.Count();
        }

    }
}
