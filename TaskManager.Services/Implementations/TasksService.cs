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

        public async Task<List<TaskWorkedHoursServiceModel>> GetTaskReport(int taskId, DateTime startDate, DateTime endDate)
        {
            var report = await this.db.WorkedHours
                .Where(wh => wh.TaskId == taskId && wh.WorkDate.Date >= startDate.Date && wh.WorkDate.Date <= endDate.Date && !wh.isDeleted)
                .OrderBy(wh => wh.WorkDate.Date)
                .ThenBy(wh => wh.EmployeeId)
                .ProjectTo<TaskWorkedHoursServiceModel>()
                .ToListAsync();
            return report;
        }


        public async Task<bool> CheckIfTaskIsClosed(int taskId)
        {
            var task = await this.db.Tasks.Include(t => t.TaskStatus).Where(t => t.Id == taskId).FirstOrDefaultAsync();
            if (task.TaskStatus.StatusName == DataConstants.TaskStatusClosed)
            {
                return true;
            }
            return false;
        }

        public IQueryable<TaskManager.Data.Models.Task> GetAllTasks(int userId, bool withClosed = false, bool withDeleted = false)
        {
            var currentUser = this.db.Employees.Include(e => e.Role).FirstOrDefault(u => u.Id == userId);

            if (currentUser.Role.Name == SuperAdmin)
            {
                return GetSuperAdminTasks(withClosed, withDeleted);
            }
            else if (currentUser.Role.Name == DirectorateAdmin)
            {
                return GetDirectorateAdminTasks(withClosed, withDeleted, currentUser.DirectorateId.HasValue ? currentUser.DirectorateId.Value : 1);
            }
            else if (currentUser.Role.Name == DepartmentAdmin)
            {
                return GetDepartmentAdminTasks(withClosed, withDeleted, currentUser.DirectorateId.Value, currentUser.DepartmentId.Value);
            }
            else if (currentUser.Role.Name == SectorAdmin)
            {
                return GetSectorAdminTasks(withClosed, withDeleted, currentUser.DirectorateId.Value, currentUser.DepartmentId.Value, currentUser.SectorId.Value);
            }
            else
            {
                return GetEmployeeListTasks(withClosed, withDeleted, currentUser.Id);
            }



        }

        private IQueryable<Data.Models.Task> GetEmployeeListTasks(bool withClosed, bool withDeleted, int userId)
        {
            var empTaskIds = this.db.EmployeesTasks
                    .Where(wh => wh.EmployeeId == userId && !wh.isDeleted)
                    .OrderBy(wh => wh.TaskId)
                    .Select(wh => wh.TaskId)
                    .Distinct()
                    .ToList();

            if (withClosed)
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName == TaskStatusClosed && t.isDeleted == false && empTaskIds.Contains(t.Id))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.StatusId)
                         .AsQueryable();
            }
            else if (withDeleted)
            {
                return this.db.Tasks
                        .Where(t => t.isDeleted == true && empTaskIds.Contains(t.Id))
                        .Include(te => te.AssignedExperts)
                        .OrderBy(t => t.StatusId)
                        .AsQueryable();
            }
            else
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName != TaskStatusClosed && t.isDeleted == false && empTaskIds.Contains(t.Id))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.Id)
                         .AsQueryable();

            }
        }
        private IQueryable<Data.Models.Task> GetSectorAdminTasks(bool withClosed, bool withDeleted, int dirId, int depId, int secId)
        {
            if (withClosed)
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName == TaskStatusClosed && t.isDeleted == false && (t.SectorId == secId || ((t.DirectorateId == null || t.DirectorateId == dirId || t.DepartmentId == depId) && t.TaskType.TypeName == TaskTypeGlobal)))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.StatusId)
                         .AsQueryable();
            }
            else if (withDeleted)
            {
                return this.db.Tasks
                        .Where(t => t.isDeleted == true && (t.SectorId == secId || ((t.DirectorateId == null || t.DirectorateId == dirId || t.DepartmentId == depId) && t.TaskType.TypeName == TaskTypeGlobal)))
                        .Include(te => te.AssignedExperts)
                        .OrderBy(t => t.StatusId)
                        .AsQueryable();
            }
            else
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName != TaskStatusClosed && t.isDeleted == false && (t.SectorId == secId || ((t.DirectorateId == null || t.DirectorateId == dirId || t.DepartmentId == depId) && t.TaskType.TypeName == TaskTypeGlobal)))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.Id)
                         .AsQueryable();
            }
        }

        private IQueryable<Data.Models.Task> GetDepartmentAdminTasks(bool withClosed, bool withDeleted, int dirId, int depId)
        {
            if (withClosed)
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName == TaskStatusClosed && t.isDeleted == false && (t.DepartmentId == depId || ((t.DirectorateId == null || t.DirectorateId == dirId) && t.TaskType.TypeName == TaskTypeGlobal)))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.StatusId)
                         .AsQueryable();
            }
            else if (withDeleted)
            {
                return this.db.Tasks
                        .Where(t => t.isDeleted == true && (t.DepartmentId == depId || ((t.DirectorateId == null || t.DirectorateId == dirId) && t.TaskType.TypeName == TaskTypeGlobal)))
                        .Include(te => te.AssignedExperts)
                        .OrderBy(t => t.StatusId)
                        .AsQueryable();
            }
            else
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName != TaskStatusClosed && t.isDeleted == false && (t.DepartmentId == depId || ((t.DirectorateId == null || t.DirectorateId == dirId) && t.TaskType.TypeName == TaskTypeGlobal)))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.Id)
                         .AsQueryable();
            }
        }

        private IQueryable<Data.Models.Task> GetDirectorateAdminTasks(bool withClosed, bool withDeleted, int dirId)
        {
            if (withClosed)
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName == TaskStatusClosed && t.isDeleted == false && (t.DirectorateId == dirId || t.DirectorateId == null))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.StatusId)
                         .AsQueryable();
            }
            else if (withDeleted)
            {
                return this.db.Tasks
                        .Where(t => t.isDeleted == true && (t.DirectorateId == dirId || t.DirectorateId == null))
                        .Include(te => te.AssignedExperts)
                        .OrderBy(t => t.StatusId)
                        .AsQueryable();
            }
            else
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName != TaskStatusClosed && t.isDeleted == false && (t.DirectorateId == dirId || t.DirectorateId == null))
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.Id)
                         .AsQueryable();
            }
        }

        private IQueryable<Data.Models.Task> GetSuperAdminTasks(bool withClosed, bool withDeleted)
        {
            if (withClosed)
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName == TaskStatusClosed && t.isDeleted == false)
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.StatusId)
                         .AsQueryable();
            }
            else if (withDeleted)
            {
                return this.db.Tasks
                        .Where(t => t.isDeleted == true)
                        .Include(te => te.AssignedExperts)
                        .OrderBy(t => t.StatusId)
                        .AsQueryable();
            }
            else
            {
                return this.db.Tasks
                         .Where(t => t.TaskStatus.StatusName != TaskStatusClosed && t.isDeleted == false)
                         .Include(te => te.AssignedExperts)
                         .OrderBy(t => t.Id)
                         .AsQueryable();
            }
        }

        public async Task<string> MarkTaskActiveAsync(int taskId, int userId)
        {
            try
            {
                var taskToRestore = await this.db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                var expert = await this.db.Employees.FirstOrDefaultAsync(e => e.Id == userId);
                taskToRestore.isDeleted = false;
                try
                {
                    taskToRestore.EndNote += $"Задачата е възстановена след изтриване на {DateTime.Now.Date} от {expert.FullName}";
                }
                catch (Exception)
                {
                }
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> MarkTaskDeletedAsync(int taskId, int userId)
        {
            try
            {
                var taskToDelete = await this.db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
                var expert = await this.db.Employees.FirstOrDefaultAsync(e => e.Id == userId);
                if (expert == null || taskToDelete == null)
                {
                    return "Няма такъв експерт или задача";
                }
                taskToDelete.isDeleted = true;
                taskToDelete.DeletedByUserId = expert.Id;
                if (taskToDelete.StatusId != await this.db.TasksStatuses.Where(ts => ts.StatusName == TaskStatusClosed).Select(ts => ts.Id).FirstOrDefaultAsync())
                {
                    taskToDelete.StatusId = this.db.TasksStatuses.Where(ts => ts.StatusName == TaskStatusClosed).Select(ts => ts.Id).FirstOrDefault();
                    taskToDelete.EndNote = $"Задачата е приключена служебно при изтриването й на {DateTime.Now.Date} от {expert.FullName}";
                }
                if (!taskToDelete.EndDate.HasValue)
                {
                    taskToDelete.EndDate = DateTime.Now;
                }
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }
        public async Task<bool> CheckTaskByIdAsync(int taskId)
        {
            var taskFromDb = await this.db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (taskFromDb == null)
            {
                return false;
            }
            return true;
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

        public async Task<int> AddNewTaskAsync(AddNewTaskServiceModel newTask)
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
                if (newTask.ParentTaskId.Value == 0 || newTask.ParentTaskId == null)
                {
                    taskDB.TypeId = await this.db.TasksTypes.Where(tt => tt.TypeName == TaskTypeGlobal).Select(tt => tt.Id).FirstOrDefaultAsync();
                }
                else
                {
                    taskDB.ParentTaskId = newTask.ParentTaskId;
                }

                await this.db.Tasks.AddAsync(taskDB);
                await this.db.SaveChangesAsync();
                return taskDB.Id;
            }
            catch (Exception)
            {
                return -1;
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
                newTypeDb = new TasksType()
                {
                    TypeName = DataConstants.TaskTypeGlobal
                };
                await this.db.TasksTypes.AddAsync(newTypeDb);
                newTypeDb = new TasksType()
                {
                    TypeName = DataConstants.TaskTypeSystem
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

        public async Task<string> SystemTasksAsync()
        {
            await this.db.Database.BeginTransactionAsync();
            try
            {
                var admin = await this.db.Employees.FirstOrDefaultAsync();

                var vacationTask = new TaskManager.Data.Models.Task()
                {
                    TaskName = "Отпуски",
                    OwnerId = admin.Id,
                    //OwnerId = await this.db.Employees.Select(e => e.Id).FirstOrDefaultAsync(),
                    AssignerId = admin.Id,
                    HoursLimit = 1000000,
                    StartDate = DateTime.Now.Date.AddDays(-30),
                    RegCreated = DateTime.Now.Date,
                    EndDatePrognose = DateTime.Now.Date.AddDays(8000),
                    Description = "Системна задача за отчитане на отпуските",
                    StatusId = await this.db.TasksStatuses.Where(s => s.StatusName == DataConstants.TaskStatusInProgres).Select(s => s.Id).FirstOrDefaultAsync(),
                    PriorityId = await this.db.Priorities.Where(p => p.PriorityName == DataConstants.TaskPriorityNormal).Select(s => s.Id).FirstOrDefaultAsync(),
                    TypeId = await this.db.TasksTypes.Where(p => p.TypeName == DataConstants.TaskTypeSystem).Select(s => s.Id).FirstOrDefaultAsync(),
                    isDeleted = false
                };
                var illTask = new TaskManager.Data.Models.Task()
                {
                    TaskName = "Болнични",
                    OwnerId = admin.Id,
                    AssignerId = admin.Id,
                    HoursLimit = 1000000,
                    StartDate = DateTime.Now.Date.AddDays(-30),
                    RegCreated = DateTime.Now.Date,
                    EndDatePrognose = DateTime.Now.Date.AddDays(8000),
                    Description = "Системна задача за отчитане на болничните",
                    StatusId = await this.db.TasksStatuses.Where(s => s.StatusName == DataConstants.TaskStatusInProgres).Select(s => s.Id).FirstOrDefaultAsync(),
                    PriorityId = await this.db.Priorities.Where(p => p.PriorityName == DataConstants.TaskPriorityNormal).Select(s => s.Id).FirstOrDefaultAsync(),
                    TypeId = await this.db.TasksTypes.Where(p => p.TypeName == DataConstants.TaskTypeSystem).Select(s => s.Id).FirstOrDefaultAsync(),
                    isDeleted = false
                };

                await this.db.Tasks.AddAsync(illTask);
                await this.db.Tasks.AddAsync(vacationTask);
                await this.db.SaveChangesAsync();
                vacationTask.AssignedExperts.Add(new EmployeesTasks { 
                EmployeeId = admin.Id,
                TaskId = vacationTask.Id
                });
                illTask.AssignedExperts.Add(new EmployeesTasks
                {
                    EmployeeId = admin.Id,
                    TaskId = illTask.Id
                });
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при създаването на системните задачи.";
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

        public async Task<AddNoteToTaskServiceModel> GetTaskEmpNoteForDateAsync(int taskId, int empId, DateTime workDate)
        {
            var note = await this.db.WorkedHours
                .Where(wh => wh.TaskId == taskId && wh.EmployeeId == empId && wh.WorkDate.Date == workDate.Date)
                .ProjectTo<AddNoteToTaskServiceModel>()
                .FirstOrDefaultAsync();
            return note;
        }

        public async Task<bool> SetTaskEmpNoteForDateAsync(AddNoteToTaskServiceModel model)
        {
            try
             {
                var employeeDateWork = await this.db.WorkedHours
                    .Where(wh => wh.TaskId == model.TaskId && wh.EmployeeId == model.EmployeeId && wh.WorkDate.Date == model.WorkDate.Date)
                    .FirstOrDefaultAsync();

                if (employeeDateWork == null)
                {
                    var newEmployeeDateWork = new WorkedHours()
                    {
                        EmployeeId = model.EmployeeId,
                        TaskId = model.TaskId,
                        HoursSpend = 0,
                        isDeleted = false,
                        Text = model.Text,
                        WorkDate = model.WorkDate.Date
                    };
                    await this.db.WorkedHours.AddAsync(newEmployeeDateWork);
                }
                else
                {
                    employeeDateWork.Text = model.Text;
                }
                await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> SetWorkedHoursAsyncOld(TaskWorkedHoursServiceModel workedHours)
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
                        if (workedHours.HoursSpend < 1)
                        {
                            return "Няма данни да е работено по задачата за тази дата. Въвеждането на нулева или отрицателна стойност на часовете, е невалидно в този случай.";
                        }
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
                        if ((currentTaskHours.HoursSpend + workedHours.HoursSpend) >= 0)  //проверка да не се окаже , че експерта е работил отрицателни часове
                        {
                            currentTaskHours.HoursSpend += workedHours.HoursSpend;
                            currentTaskHours.Text = workedHours.Text;
                        }
                        else
                        {
                            return $"Невалидна стойност! Можете да коригирате до {currentTaskHours.HoursSpend} часа по тази задача.";
                        }

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
                }
                var currentTaskHours = await this.db.WorkedHours
                    .Where(d => d.WorkDate == workedHours.WorkDate && d.EmployeeId == workedHours.EmployeeId && d.TaskId == workedHours.TaskId)
                    .FirstOrDefaultAsync();
                if (currentTaskHours == null)   // ако по конкретната задача не е работено за тази дата
                {
                    if (workedHours.HoursSpend > 0)
                    {
                        var workedHoursDB = new WorkedHours()
                        {
                            EmployeeId = workedHours.EmployeeId,
                            TaskId = workedHours.TaskId,
                            HoursSpend = workedHours.HoursSpend,
                            WorkDate = workedHours.WorkDate
                        };
                        await this.db.WorkedHours.AddAsync(workedHoursDB);
                    }
                }
                else
                {
                    currentTaskHours.HoursSpend = workedHours.HoursSpend;
                }
                await this.db.SaveChangesAsync();
                return "success";

            }
            catch (Exception)
            {
                return "Service[SetWorkedHoursAsync]. Неуспешен запис.";
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
                currentTask.ParentTaskId = taskToEdit.ParentTaskId;
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

                if (taskToEdit.ParentTaskId.Value == 0 || taskToEdit.ParentTaskId == null)
                {
                    currentTask.TypeId = await this.db.TasksTypes.Where(tt => tt.TypeName == TaskTypeGlobal).Select(tt => tt.Id).FirstOrDefaultAsync();
                    currentTask.ParentTaskId = null;
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
                .ProjectTo<ReportServiceModel>(new { employeesIds = employeesIds.ToArray(), startDate, endDate })
                .ToListAsync();

            return searchedTasks;
        }

        public async Task<TaskServiceModel> GetParentTaskAsync(int parentTaskId = 0)
        {
            var searchedTask = await this.db.Tasks
                .Where(t => t.Id == parentTaskId)
                .OrderBy(t => t.Id)
                .ProjectTo<TaskServiceModel>()
                .FirstOrDefaultAsync();

            return searchedTask;
        }

        public IEnumerable<SelectServiceModel> GetParentTaskNames(int? directorateId)
        {
            return this.db.Tasks.Where(t => t.ParentTaskId == null && (t.DirectorateId == directorateId || t.DirectorateId == null) && t.isDeleted == false)
                .Select(t => new SelectServiceModel
                {
                    Id = t.Id,
                    TextValue = t.TaskName,
                    isDeleted = t.isDeleted
                })
                .ToList();
        }

        public async Task<int> GetSystemTaskIdByNameAsync(string name)
        {
            return await this.db.Tasks
                    .Where(et => et.TaskName == name && et.TaskType.TypeName == TaskTypeSystem)
                    .Select(et => et.Id)
                    .FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveSystemTaskForDate(int userId, DateTime currentDate)
        {
            var systemTasksIds = await this.db.Tasks
                    .Where(et => et.TaskType.TypeName == TaskTypeSystem)
                    .Select(et => et.Id)
                    .ToListAsync();
            foreach (var taskid in systemTasksIds)
            {
                var systemEmployeeTask = await this.db.WorkedHours
                    .Where(et => et.TaskId == taskid && et.EmployeeId == userId && et.WorkDate.Date == currentDate.Date)
                    .FirstOrDefaultAsync();
                if (systemEmployeeTask != null)
                {
                    this.db.WorkedHours.Remove(systemEmployeeTask);
                }
            }
            await this.db.SaveChangesAsync();
            return true;
        }
    }
}
