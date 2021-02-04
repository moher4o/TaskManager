using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services.Implementations
{
    public class TasksService : ITasksService
    {
        private readonly TasksDbContext db;
        public TasksService(TasksDbContext db)
        {
            this.db = db;

        }

        public async Task<string> AddNewTaskAsync(AddNewTaskServiceModel newTask)
        {
            try
            {
                List<EmployeesTasks> assignedEmployeesDB = new List<EmployeesTasks>();
                Data.Models.Task taskDB = new Data.Models.Task()
                {
                    TaskName = newTask.TaskName,
                    Description = newTask.Description,
                    AssignerId = newTask.AssignerId,
                    DirectorateId = newTask.DirectorateId,
                    DepartmentId = newTask.DepartmentId,
                    SectorId = newTask.SectorId,
                    OwnerId = newTask.OwnerId,
                    HoursLimit = newTask.HoursLimit,
                    RegCreated = DateTime.UtcNow.Date,
                    StartDate = newTask.StartDate,
                    EndDatePrognose = newTask.EndDatePrognose,
                    PriorityId = newTask.PriorityId,
                    TypeId = newTask.TypeId,
                    StatusId = newTask.StatusId
                };

                if (newTask.EmployeesIds != null && newTask.EmployeesIds.Length > 0)
                {
                    foreach (var employee in newTask.EmployeesIds)
                    {
                        assignedEmployeesDB.Add(new EmployeesTasks
                        {
                            EmployeeId = employee,
                            Task = taskDB
                        });
                    }
                    taskDB.AssignedExperts = assignedEmployeesDB;
                }
                await this.db.Tasks.AddAsync(taskDB);
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception)
            {
                return "Неуспешен запис. Проверете входните данни.";
            }
        }

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

        public async Task<string> SetWorkedHoursAsync(TaskWorkedHoursServiceModel workedHours)
        {
            try
            {
                var workedHousDB = new WorkedHours()
                {
                    EmployeeId = workedHours.EmployeeId,
                    TaskId = workedHours.TaskId,
                    HoursSpend = workedHours.HoursSpend,
                    Text = workedHours.Text,
                    WorkDate = workedHours.WorkDate
                };
                await this.db.WorkedHours.AddAsync(workedHousDB);
                await this.db.SaveChangesAsync();
                return "success";

            }
            catch (Exception)
            {
                return "Неуспешен запис. Проверете входните данни.";
            }
        }
    }
}
