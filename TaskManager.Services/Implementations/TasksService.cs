using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Services.Models;
using TaskManager.Services.Models.ReportModels;
using TaskManager.Services.Models.TaskModels;
using static TaskManager.Common.DataConstants;

namespace TaskManager.Services.Implementations
{
    public class TasksService : ITasksService
    {
        private readonly TasksDbContext db;
        public TasksService(TasksDbContext db)
        {
            this.db = db;

        }


        public IQueryable<TaskInfoServiceModel> GetTaskDetails(int taskId)
        {
            var taskData = this.db.Tasks
                    .Where(et => et.Id == taskId)
                    //.Include(te => te.AssignedExperts)
                    .ProjectTo<TaskInfoServiceModel>()
                    .AsQueryable();

            return taskData;
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
                    RegCreated = DateTime.Now.Date,
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
                    TypeName = DataConstants.TaskTypeSpecificWork
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);

                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeProcurement
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeLearning
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeAdminActivity
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {

                    TypeName = DataConstants.TaskTypeMeetings
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
                var isDeletedEmployee = await this.db.EmployeesTasks
                    .Where(e => e.EmployeeId == workedHours.EmployeeId && e.TaskId == workedHours.TaskId)
                    .Select(t => t.isDeleted).FirstOrDefaultAsync();

                if (isDeletedEmployee)
                {
                    return workedHours.EmployeeName + " не е активен участник по задача: " + workedHours.TaskName;
                }

                var currentTask = await this.db.Tasks.Where(t => t.Id == workedHours.TaskId && t.isDeleted == false).Include(t => t.TaskStatus).FirstOrDefaultAsync();
                if (currentTask.StartDate.Date > workedHours.WorkDate.Date)
                {
                    return "Началната дата на задачата е : " + currentTask.StartDate.Date.ToString("dd/MM/yyyy") + "г.";
                }
                if (currentTask.TaskStatus.StatusName == TaskStatusClosed)
                {
                    return "Задачата е приключена!";
                    //if (currentTask.EndDate.Value.Date < workedHours.WorkDate.Date)
                    //{
                    //    return "Задачата е приключена на : " + currentTask.EndDate.Value.Date.ToString("dd/MM/yyyy") + "г.";
                    //}
                }
                var currentTaskHours = await this.db.WorkedHours
                    .Where(d => d.WorkDate == workedHours.WorkDate && d.EmployeeId == workedHours.EmployeeId && d.TaskId == workedHours.TaskId)
                    .FirstOrDefaultAsync();

                var dayWorkedHurs = this.db.WorkedHours        //отработени часове за датата по всички задачи
                    .Where(d => d.WorkDate == workedHours.WorkDate && d.EmployeeId == workedHours.EmployeeId)
                    .Sum(d => d.HoursSpend);

                var totalHoursPerDayPrognose = dayWorkedHurs + workedHours.HoursSpend;
                if (totalHoursPerDayPrognose <= DataConstants.TotalHoursPerDay)     //ако не се надвишава лимита часове
                {
                    if (currentTaskHours == null)   // ако по конкретната задача не е работено за тази дата
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
                    }
                    else
                    {
                        currentTaskHours.HoursSpend += workedHours.HoursSpend;
                    }
                    await this.db.SaveChangesAsync();
                    return "success";

                }
                else
                {
                    return "Остават " + (DataConstants.TotalHoursPerDay - dayWorkedHurs).ToString() + "часа за дата " + workedHours.WorkDate.Date.ToString("dd/MM/yyyy") + "г. Записа е неуспешен";
                }

            }
            catch (Exception)
            {
                return "Неуспешен запис. Проверете входните данни.";
            }
        }

        public async Task<bool> CloseTaskAsync(int taskId, string endNote, int closerid)
        {
            var currentTask = await this.db.Tasks
                       .Where(t => t.Id == taskId)
                       .FirstOrDefaultAsync();
            if (currentTask == null || endNote.Length > 500)
            {
                return false;
            }

            currentTask.EndNote = endNote;
            currentTask.CloseUserId = closerid;
            currentTask.EndDate = DateTime.Now;
            currentTask.TaskStatus = await this.db.TasksStatuses
                                                    .Where(t => t.StatusName == DataConstants.TaskStatusClosed)
                                                    .FirstOrDefaultAsync();
            await this.db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReopenTaskAsync(int taskId)
        {
            var currentTask = await this.db.Tasks
                       .Where(t => t.Id == taskId)
                       .FirstOrDefaultAsync();
            if (currentTask == null)
            {
                return false;
            }
            CultureInfo.CurrentCulture.ClearCachedData();
            currentTask.EndDate = null;
            currentTask.EndNote = "Задачата е отново отворена на дата " + DateTime.Now.ToString("dd/MM/yyyy") + "г. " + "--> " + (currentTask.EndNote.Length > 430 ? currentTask.EndNote.Substring(0, 430) + "..." : currentTask.EndNote);
            currentTask.TaskStatus = await this.db.TasksStatuses
                                                    .Where(t => t.StatusName == DataConstants.TaskStatusInProgres)
                                                    .FirstOrDefaultAsync();
            await this.db.SaveChangesAsync();
            return true;
        }

        public async Task<string> EditTaskAsync(AddNewTaskServiceModel taskToEdit)
        {
            try
            {
                var result = "success";
                if (taskToEdit == null)
                {
                    return "Service error! EditTaskAsync --> taskToEdit is null. Send result to admin";
                }
                var currentTask = await this.db.Tasks
               .Where(t => t.Id == taskToEdit.Id)
               .FirstOrDefaultAsync();
                if (currentTask == null)
                {
                    return "Не съществува задача с N: " + taskToEdit.Id.ToString();
                }

                currentTask.TaskName = taskToEdit.TaskName;
                currentTask.Description = taskToEdit.Description;
                currentTask.AssignerId = taskToEdit.AssignerId;
                currentTask.DirectorateId = taskToEdit.DirectorateId;
                currentTask.DepartmentId = taskToEdit.DepartmentId;
                currentTask.SectorId = taskToEdit.SectorId;
                currentTask.HoursLimit = taskToEdit.HoursLimit;
                currentTask.EndDatePrognose = taskToEdit.EndDatePrognose.Value.Date;
                currentTask.PriorityId = taskToEdit.PriorityId;
                currentTask.TypeId = taskToEdit.TypeId;
                currentTask.StatusId = taskToEdit.StatusId;
                //OwnerId = newTask.OwnerId,
                //RegCreated = DateTime.UtcNow.Date,
                var firstDateWorkedHours = await this.db.WorkedHours
                    .Where(d => d.TaskId == currentTask.Id)
                    .OrderBy(d => d.WorkDate.Date)
                    //.Select(d => d.WorkDate.Date)
                    .FirstOrDefaultAsync();
                if (firstDateWorkedHours != null)
                {
                    //currentTask.StartDate = taskToEdit.StartDate.Date < firstDateWorkedHours.Value.Date ? taskToEdit.StartDate.Date : firstDateWorkedHours.Value.Date;
                    if (taskToEdit.StartDate.Date > firstDateWorkedHours.WorkDate.Date)
                    {
                        result = "halfsuccess";
                        currentTask.StartDate = firstDateWorkedHours.WorkDate.Date;
                    }
                    else
                    {
                        currentTask.StartDate = taskToEdit.StartDate.Date;
                    }
                }
                else
                {
                    currentTask.StartDate = taskToEdit.StartDate.Date;
                    
                }

                this.db.EmployeesTasks.Where(et => et.TaskId == currentTask.Id)
                                      .ToList()
                                      .ForEach(e => e.isDeleted = true);   //премахвам всички експерти
                await this.db.SaveChangesAsync();

                if (taskToEdit.EmployeesIds != null && taskToEdit.EmployeesIds.Length > 0)    //добавям експерти
                {
                    
                    foreach (var employee in taskToEdit.EmployeesIds)
                    {
                        var expert = await this.db.EmployeesTasks.Where(e => e.EmployeeId == employee && e.TaskId == currentTask.Id).FirstOrDefaultAsync();
                        if (expert != null)
                        {
                            expert.isDeleted = false;
                            await this.db.SaveChangesAsync();
                        }
                        else
                        {
                            var newExpert = new EmployeesTasks()
                            {
                                EmployeeId = employee,
                                TaskId = currentTask.Id
                            };
                            await this.db.EmployeesTasks.AddAsync(newExpert);
                            await this.db.SaveChangesAsync();
                        }

                    }
                }
                
                await this.db.SaveChangesAsync();

                return result;
               
            }
            catch (Exception)
            {
                return "Неуспешено редактиране. Проверете входните данни.";
            }

        }

        public async Task<List<ReportServiceModel>> ExportTasksAsync(IList<int> employeesIds, DateTime startDate, DateTime endDate)
        {
            var report = new ReportServiceModel();
            var tasksIdList = new List<int>();
            foreach (var employeeId in employeesIds)
            {
                var empTaskIds = await this.db.WorkedHours
                    .Where(wh => wh.EmployeeId == employeeId && wh.WorkDate.Date >= startDate.Date && wh.WorkDate <= endDate.Date && !wh.isDeleted)
                    .OrderBy(wh => wh.TaskId)
                    .Select(wh => wh.TaskId)
                    .Distinct()
                    .ToListAsync();

                tasksIdList = tasksIdList.Union(empTaskIds).ToList();
            }

            var searchedTasks = await this.db.Tasks
                .Where(t => tasksIdList.Contains(t.Id))
                .OrderBy(t => t.Id)
                .ProjectTo<ReportServiceModel>(new {employeesIds = employeesIds.ToArray(), startDate, endDate})
                .ToListAsync();

            return searchedTasks;
        }

        public async Task<TaskServiceModel> GetTaskAsync(int parentTaskId = 0)
        {
            var searchedTask = await this.db.Tasks
                .Where(t => t.Id == parentTaskId)
                .OrderBy(t => t.Id)
                .ProjectTo<TaskServiceModel>()
                .FirstOrDefaultAsync();

            return searchedTask;
        }
    }
}
