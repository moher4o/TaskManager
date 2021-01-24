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

                    StatusName = DataConstants.TaskStatusNew,
                    isDeleted = false,
                };
                await this.db.TasksStatuses.AddAsync(newStatusDB);
                newStatusDB = new TasksStatus()
                {

                    StatusName = DataConstants.TaskStatusInProgres,
                    isDeleted = false,
                };
                await this.db.TasksStatuses.AddAsync(newStatusDB);
                newStatusDB = new TasksStatus()
                {

                    StatusName = DataConstants.TaskStatusClosed,
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

        public async Task<string> CreateTasksPrioritiesAsync()
        {
            await this.db.Database.BeginTransactionAsync();
            try
            {
                var priorities = this.db.Priorities.ToList();
                this.db.Priorities.RemoveRange(priorities);
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при изтриването на приоритетите.";
            }
            this.db.Database.CommitTransaction();

            await this.db.Database.BeginTransactionAsync();
            try
            {

                var newPriorityDb = new Priority()
                {
                    PriorityName = DataConstants.TaskPriorityLow
                };
                await this.db.Priorities.AddAsync(newPriorityDb);
                newPriorityDb = new Priority()
                {

                    PriorityName = DataConstants.TaskPriorityNormal
                };
                await this.db.Priorities.AddAsync(newPriorityDb);

                newPriorityDb = new Priority()
                {

                    PriorityName = DataConstants.TaskPriorityHi
                };
                await this.db.Priorities.AddAsync(newPriorityDb);

                await this.db.SaveChangesAsync();

            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при създаването на приоритетите.";
            }
            this.db.Database.CommitTransaction();
            return "success";
        }

        public async Task<string> CreateTasksTypesAsync()
        {
            await this.db.Database.BeginTransactionAsync();
            try
            {
                var types = this.db.TasksTypes.ToList();
                this.db.TasksTypes.RemoveRange(types);
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при изтриването на типовете задачи.";
            }
            this.db.Database.CommitTransaction();

            await this.db.Database.BeginTransactionAsync();
            try
            {

                var newTypeDb = new TasksType()
                {
                    TypeName = DataConstants.TaskTypeProject
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);

                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeDirectorate
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeDepartment
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeSector
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeEmployee
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeOther
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);

                await this.db.SaveChangesAsync();

            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при създаването на типовете на задачите.";
            }
            this.db.Database.CommitTransaction();
            return "success";
        }
        public int TasksStatusCount()
        {
            return this.db.TasksStatuses.Count();
        }
        public int TasksPrioritysCount()
        {
            return this.db.Priorities.Count();
        }
        public int TasksTypesCount()
        {
            return this.db.TasksTypes.Count();
        }

    }
}
