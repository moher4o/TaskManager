﻿using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Services.Models;
using TaskManager.Common;
using TaskManager.Services.Models.TaskModels;
using Microsoft.EntityFrameworkCore;
using static TaskManager.Common.DataConstants;
using TaskManager.Services.Models.ReportModels;
using TaskManager.Services.Models.EmployeeModels;

namespace TaskManager.Services.Implementations
{
    public class EmployeesService : IEmployeesService
    {

        private readonly TasksDbContext db;
        public EmployeesService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(int userId, DateTime dateToProcess)
        {
            var tasks = await this.db.EmployeesTasks
                    .Where(et => et.EmployeeId == userId)
                    .Select(t => t.Task)
                    .Where(t => t.isDeleted == false && t.StartDate.Date <= dateToProcess.Date)
                    .Distinct()
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>(new { currentEmployeeId = userId, workDate = dateToProcess.Date })
                    .ToListAsync();

            foreach (var item in tasks)
            {
                if (await this.db.EmployeesTasks.Where(et => et.EmployeeId == userId && et.TaskId == item.Id).Select(et => et.isDeleted).FirstOrDefaultAsync() == true && item.TaskStatusName != TaskStatusClosed)
                {
                    item.TaskStatusName = "Изтрит";
                }
            }

            return tasks;
        }

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(string userName, DateTime dateToProcess)
        {

            //int userId = await this.db.Employees.Where(e => (e.DaeuAccaunt.Substring(e.DaeuAccaunt.IndexOf("\\") + 1)) == userName).Select(e => e.Id).FirstOrDefaultAsync();
            int userId = await this.db.Employees.Where(e => e.DaeuAccaunt == userName).Select(e => e.Id).FirstOrDefaultAsync();
            var tasks = await this.db.EmployeesTasks
                    .Where(et => et.EmployeeId == userId)
                    .Select(t => t.Task)
                    .Where(t => t.isDeleted == false && t.StartDate.Date <= dateToProcess.Date)
                    .Distinct()
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>(new { currentEmployeeId = userId, workDate = dateToProcess.Date })
                    .ToListAsync();

            foreach (var item in tasks)
            {
                if (await this.db.EmployeesTasks.Where(et => et.EmployeeId == userId && et.TaskId == item.Id).Select(et => et.isDeleted).FirstOrDefaultAsync() == true && item.TaskStatusName != TaskStatusClosed)
                {
                    item.TaskStatusName = "Изтрит";
                }
            }

            return tasks;
        }

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetAllUserTaskAsync(int userId, DateTime dateToProcess)
        {
            var tasks = await this.db.EmployeesTasks
                    .Where(et => et.EmployeeId == userId)
                    .Select(t => t.Task)
                    .Where(t => t.StartDate.Date <= dateToProcess.Date)
                    .Distinct()
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>(new { currentEmployeeId = userId, workDate = dateToProcess.Date })
                    .ToListAsync();

            return tasks;
        }


        public async Task<List<PersonalDateReportServiceModel>> GetDateReport(int userId, DateTime currentDate)
        {
            var report = await this.db.WorkedHours
                .Where(wh => wh.EmployeeId == userId && wh.WorkDate.Date == currentDate.Date)
                .Include(wh => wh.Task)
                .Include(wh => wh.Employee)
                .Select(wh => new PersonalDateReportServiceModel
                {
                    TaskName = wh.Task.TaskName,
                    WorkedHours = wh.HoursSpend,
                    Note = wh.Text
                }).ToListAsync();

            return report;
        }

        public async Task<ShortEmployeeServiceModel> GetPersonalReport(int userId, DateTime startDate, DateTime endDate)
        {
            var report = await this.db.Employees
                .Where(e => e.Id == userId)
                .Include(e => e.WorkedHoursByTask)
                .ProjectTo<ShortEmployeeServiceModel>(new { startDate, endDate })
                .FirstOrDefaultAsync();
            return report;
        }

        public async Task<IEnumerable<SelectServiceModel>> GetUserDominions(int userId)
        {
            var users = new List<SelectServiceModel>();
            var searchedUser = await this.db.Employees.Where(e => e.Id == userId).Include(e => e.UsersRepresentative).FirstOrDefaultAsync();
            foreach (var user in searchedUser.UsersRepresentative)
            {
                users.Add(new SelectServiceModel { 
                Id = user.Id,
                TextValue = user.FullName
                });
            }
            return users;
        }

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetUserAssignerTaskAsync(int userId)
        {
            var tasks = await this.db.Tasks
                    .Where(et => et.AssignerId == userId && et.isDeleted == false && et.TaskType.TypeName != TaskTypeSystem)
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>()
                    .ToListAsync();

            return tasks;
        }

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetUserCreatedTaskAsync(int userId)
        {
            var tasks = await this.db.Tasks
                    .Where(et => et.OwnerId == userId && et.isDeleted == false)
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>()
                    .ToListAsync();

            return tasks;
        }


        public async Task<string> AddEmployeesCollection(List<AddNewEmployeeServiceModel> employees)
        {

            var connectionString = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
            SqlConnection con = new SqlConnection(connectionString);
            //SqlCommand com = new SqlCommand("ALTER TABLE JobTitles NOCHECK CONSTRAINT FK_ClassItems_ClassItems_Classif_Version_ParentItemCode");
            //SqlCommand comend = new SqlCommand("ALTER TABLE JobTitles CHECK CONSTRAINT FK_ClassItems_ClassItems_Classif_Version_ParentItemCode");
            //com.Connection = con;
            //comend.Connection = con;

            con.Open();
            //com.ExecuteNonQuery();
            //SqlTransaction transaction = con.BeginTransaction(IsolationLevel.Serializable);
            int i = 0;
            await this.db.Database.BeginTransactionAsync();
            try
            {
                for (i = 0; i <= employees.Count() - 1; i++)
                {
                    //string role = employees[i].Role ?? throw new ArgumentNullException("Ролята не може да е null");
                    //string jobtitle = employees[i].JobTitle ?? throw new ArgumentNullException("Длъжността не може да е null");
                    var newEmployeeDB = new Employee()
                    {
                        DaeuAccaunt = string.IsNullOrWhiteSpace(employees[i].DaeuAccaunt) ? throw new ArgumentNullException("Акаунта не може да е null") : employees[i].DaeuAccaunt,
                        JobTitleId = string.IsNullOrWhiteSpace(employees[i].JobTitle) ? throw new ArgumentNullException("Длъжността не може да е null") : this.db.JobTitles.Where(d => d.TitleName == employees[i].JobTitle).Select(d => d.Id).FirstOrDefault(),
                        RoleId = string.IsNullOrWhiteSpace(employees[i].Role) ? throw new ArgumentNullException("Ролята не може да е null") : this.db.Roles.Where(d => d.Name == employees[i].Role).Select(d => d.Id).FirstOrDefault(),
                        SectorId = string.IsNullOrWhiteSpace(employees[i].Sector) ? (int?)null : this.db.Sectors.Where(d => d.SectorName == employees[i].Sector).Select(d => d.Id).FirstOrDefault(),
                        DepartmentId = string.IsNullOrWhiteSpace(employees[i].Department) ? (int?)null : this.db.Departments.Where(d => d.DepartmentName == employees[i].Department).Select(d => d.Id).FirstOrDefault(),
                        DirectorateId = string.IsNullOrWhiteSpace(employees[i].Directorate) ? (int?)null : this.db.Directorates.Where(d => d.DirectorateName == employees[i].Directorate).Select(d => d.Id).FirstOrDefault(),
                        FullName = employees[i].FullName,
                        isDeleted = employees[i].isDeleted,
                        isActive = false
                    };

                    await this.db.Employees.AddAsync(newEmployeeDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", employees[i].FullName, " има грешка (плюс минус един ред!)", ex.Message);
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        public async Task<IList<UserServiceModel>> GetAllUsers(bool withDeleted = false)
        {
            return await this.db.Employees
                    .Where(t => t.isDeleted == withDeleted && t.isActive == true && t.Email != DataConstants.SystemEmail)
                    .OrderBy(t => t.DirectorateId)
                    .ThenBy(t => t.DepartmentId)
                    .ThenBy(e => e.SectorId != null ? e.SectorId : 999)
                    .ThenBy(t => t.FullName)
                    .ProjectTo<UserServiceModel>()
                    .ToListAsync();

        }

        public IEnumerable<SelectServiceModel> GetActiveEmployeesNames()
        {
            var names = this.db.Employees
                .Where(c => c.isDeleted == false && c.isActive == true && c.Email != DataConstants.SystemEmail)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName
                })
                .ToList();
            return names;
        }

        public IEnumerable<ReportUserServiceModel> GetEmployeesByList(IEnumerable<int> employeesList)
        {
            var employees = this.db.Employees
                .Where(e => employeesList.Contains(e.Id))
                .OrderBy(e => e.DepartmentId)
                .ThenBy(e => e.SectorId)
                .ProjectTo<ReportUserServiceModel>()
                .ToList();
            return employees;
        }


        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDepartmentAsync(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.isDeleted == false && c.DepartmentId == departmentId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }
        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDepartmentWithDeletedAsync(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.DepartmentId == departmentId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }

        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDirectorateAsync(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.isDeleted == false && c.DirectorateId == directorateId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }

        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDirectorateWithDeletedAsync(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.DirectorateId == directorateId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }

        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesBySectorAsync(int? sectorId)
        {
            if (sectorId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.isDeleted == false && c.SectorId == sectorId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }
        public async Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesBySectorWithDeletedAsync(int? sectorId)
        {
            if (sectorId == null)
            {
                return null;
            }
            var names = await this.db.Employees
                .Where(c => c.SectorId == sectorId && c.isActive == true)
                .OrderBy(e => e.FullName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.FullName,
                    Id = d.Id,
                    DepartmentName = d.Department.DepartmentName,
                    DirectorateName = d.Directorate.DirectorateName,
                    SectorName = d.Sector.SectorName

                })
                .ToListAsync();
            return names;
        }


        public UserServiceModel GetUserDataForCooky(string daeuAccaunt)
        {
            var searchedUser = this.db.Employees
                .Where(u => u.DaeuAccaunt == daeuAccaunt.ToLower())
                .ProjectTo<UserServiceModel>()
                .FirstOrDefault();

            return searchedUser;
        }

        public async Task<string> GetEmployeeNameByIdAsync(int userId)
        {
            return await this.db.Employees
                .Where(e => e.Id == userId)
                .Select(e => e.FullName)
                .FirstOrDefaultAsync();
        }

        public async Task<UserServiceModel> GetEmployeeByIdAsync(int userId)
        {
            return await this.db.Employees
                .Where(e => e.Id == userId)
                .ProjectTo<UserServiceModel>()
                .FirstOrDefaultAsync();
        }


        public async Task<bool> RegisterNewUserAsync(UserServiceModel newUser)
        {
            try
            {
                if (newUser != null)
                {

                    if (newUser.Id > 0)  //има такъв потребител
                    {
                        var userFromDB = await this.db.Employees.FirstOrDefaultAsync(e => e.Id == newUser.Id);
                        userFromDB.FullName = newUser.FullName;
                        userFromDB.Email = newUser.Email;
                        userFromDB.TelephoneNumber = newUser.TelephoneNumber.Replace(" ", "");
                        userFromDB.MobileNumber = newUser.MobileNumber;
                        userFromDB.JobTitleId = newUser.JobTitleId;
                        userFromDB.DirectorateId = newUser.DirectorateId;
                        userFromDB.DepartmentId = newUser.DepartmentId;
                        userFromDB.SectorId = newUser.SectorId;
                        //userFromDB.DaeuAccaunt = newUser.DaeuAccaunt;
                        userFromDB.RoleId = newUser.RoleId;
                        userFromDB.Notify = newUser.Notify;
                        userFromDB.RepresentativeId = newUser.RepresentativeId;
                        await this.db.SaveChangesAsync();
                    }
                    else  //няма такъв потребител
                    {
                        var employeeDb = new Employee()
                        {
                            FullName = newUser.FullName,
                            Email = newUser.Email,
                            TelephoneNumber = newUser.TelephoneNumber.Replace(" ", ""),
                            MobileNumber = newUser.MobileNumber,
                            JobTitleId = newUser.JobTitleId,
                            DirectorateId = newUser.DirectorateId,
                            DepartmentId = newUser.DepartmentId,
                            SectorId = newUser.SectorId,
                            DaeuAccaunt = newUser.DaeuAccaunt,
                            RoleId = await this.db.Roles.Where(r => r.Name == DataConstants.Employee).Select(r => r.Id).FirstOrDefaultAsync(),
                            isActive = false,
                            MessageReaded = false,
                            TwoFAActiv = false,
                            SecretKeyHash = KeyGenerator.Encrypt(KeyGenerator.GenerateRandomString(),newUser.DaeuAccaunt)
                        };
                        await this.db.Employees.AddAsync(employeeDb);
                        await this.db.SaveChangesAsync();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> SetSecretKey(int userId)
        {
            try
            {
                var userFromDB = await this.db.Employees.FirstOrDefaultAsync(e => e.Id == userId);
                if (userFromDB == null)
                {
                    return "error";
                }

                userFromDB.SecretKeyHash = KeyGenerator.Encrypt(KeyGenerator.GenerateRandomString(), userFromDB.DaeuAccaunt);
                await this.db.SaveChangesAsync();
                return KeyGenerator.Decrypt(userFromDB.SecretKeyHash, userFromDB.DaeuAccaunt);
            }
            catch (Exception)
            {
                return "error";
            }
        }

        public async Task<string> DeactivateUserAsync(int userId)
        {
            try
            {
                var userToDeactivate = await this.db.Employees
                    .Where(e => e.Id == userId)
                    .Include(e => e.TasksAssigner)
                    .Include(e => e.Tasks)
                    .FirstOrDefaultAsync();
                if (userToDeactivate == null)
                {
                    return "Няма акаунт с Id : " + userId.ToString();
                }

                var activeUserTask = userToDeactivate.TasksAssigner.Where(t => t.StatusId != this.db.TasksStatuses.Where(st => st.StatusName == TaskStatusClosed).Select(ts => ts.Id).FirstOrDefault()).Count();
                if (activeUserTask > 0)
                {
                    return $"Потребителя е отговорник по ({activeUserTask}) активни задачи. Прехвърлете задачите на друг преди да го деактивирате.";
                }
                foreach (var taskInvolved in userToDeactivate.Tasks)
                {
                    taskInvolved.isDeleted = true;
                    await this.db.SaveChangesAsync();
                }
                userToDeactivate.isDeleted = true;
                await this.db.SaveChangesAsync();

                return "success";

            }
            catch (Exception)
            {
                return "[Service] Грешка в сървиса за обработка на заявката. Свържете се с разработчика.";
            }

        }

        public async Task<bool> AddAllToSystemTasksAsync()
        {
            try
            {
                var usersNotAddedToSystemTasks = await this.db.Employees.ToListAsync();
                var systemTasks = await this.db.Tasks.Where(t => t.TaskType.TypeName == DataConstants.TaskTypeSystem).ToListAsync();
                foreach (var daeuTask in systemTasks)
                {
                    foreach (var user in usersNotAddedToSystemTasks)
                    {
                        if (await this.db.EmployeesTasks.Where(et => et.isDeleted == false && et.TaskId == daeuTask.Id && et.EmployeeId == user.Id).FirstOrDefaultAsync() == null)
                        {
                            daeuTask.AssignedExperts.Add(new EmployeesTasks
                            {
                                EmployeeId = user.Id,
                                TaskId = daeuTask.Id
                            });
                        }

                    }
                    await this.db.SaveChangesAsync();
                }
                
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> АctivateUserAsync(int userId)
        {
            try
            {
                var userToActivate = await this.db.Employees.FirstOrDefaultAsync(e => e.Id == userId);
                if (userToActivate == null)
                {
                    return false;
                }
                userToActivate.isDeleted = false;
                userToActivate.isActive = true;
                await this.db.SaveChangesAsync();
                var systemTasks = await this.db.Tasks.Where(t => t.TaskType.TypeName == DataConstants.TaskTypeSystem).ToListAsync();
                foreach (var daeuTask in systemTasks)
                {
                    daeuTask.AssignedExperts.Add(new EmployeesTasks
                    {
                        EmployeeId = userToActivate.Id,
                        TaskId = daeuTask.Id
                    });
                }
                await this.db.SaveChangesAsync();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<UserServiceModel>> GetAllNotActivatedUsersAsync()
        {
            return await this.db.Employees
                .Where(t => t.isActive == false)
                .OrderBy(t => t.DirectorateId)
                .ProjectTo<UserServiceModel>()
                .ToListAsync();
        }

        public async Task<string> GenerateEmailWhenEmpty()
        {
            try
            {
                var employeesWithoutMail = await this.db.Employees
                    .Where(e => string.IsNullOrWhiteSpace(e.Email))
                    .ToListAsync();
                foreach (var emp in employeesWithoutMail)
                {
                    emp.Email = emp.DaeuAccaunt.Substring(emp.DaeuAccaunt.LastIndexOf("\\") + 1) + "@e-gov.bg";
                }
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<bool> MarkUserReadMessage(int userId)
        {
            try
            {
            var currentuser = await this.db.Employees.Where(u => u.Id == userId).FirstOrDefaultAsync();
            currentuser.MessageReaded = true;
            await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> Set2FAFlagEnabled(int userId)
        {
            try
            {
                var currentuser = await this.db.Employees.Where(u => u.Id == userId).FirstOrDefaultAsync();
                currentuser.TwoFAActiv = true;
                await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Set2FADisabled(int userId)
        {
            try
            {
                var currentuser = await this.db.Employees.Where(u => u.Id == userId).FirstOrDefaultAsync();
                currentuser.TwoFAActiv = false;
                await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> AddTokenHash(int userId, string token)
        {
            try
            {

                var user = await this.db.Employees
                    .Where(e => e.Id == userId).FirstOrDefaultAsync();
                if (user != null)
                {
                    user.TokenHash = KeyGenerator.Encrypt(token, user.DaeuAccaunt);
                    await this.db.SaveChangesAsync();
                    return "success";
                }
                else
                {
                    return $"Няма потребител с Id: {userId}";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetMobileToken(int userId)
        {
            try
            {

                var user = await this.db.Employees
                    .Where(e => e.Id == userId && e.isDeleted == false).FirstOrDefaultAsync();
                if (user != null)
                {
                    return KeyGenerator.Decrypt(user.TokenHash, user.DaeuAccaunt);
                }
                else
                {
                    return "error";
                }
            }
            catch (Exception)
            {
                return "error";
            }
        }

        public async Task<string> GenerateSecretKeyWhenEmpty()
        {
            try
            {
                var employeesWithoutSecret = await this.db.Employees
                    .Where(e => string.IsNullOrWhiteSpace(e.SecretKeyHash))
                    .ToListAsync();
                foreach (var emp in employeesWithoutSecret)
                {
                    var secret = KeyGenerator.GenerateRandomString();
                    emp.SecretKeyHash = KeyGenerator.Encrypt(secret, emp.DaeuAccaunt);
                }
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetUserNameBySKAsync(string secretKey)
        {
            try
            {
                var usersHashs = await this.db.Employees
                                            .Where(e => e.isDeleted == false)
                                            .Select(e => new {
                                                daeuAcount = e.DaeuAccaunt,
                                                secret = KeyGenerator.Decrypt(e.SecretKeyHash, e.DaeuAccaunt)
                                            }).ToListAsync();

                var username = usersHashs.Where(uh => uh.secret == secretKey).Select(uh => uh.daeuAcount).FirstOrDefault();
                return username;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<int> GetUserIdBySKAsync(string secretKey)
        {
            try
            {
                var usersHashs = await this.db.Employees
                                            .Where(e => e.isDeleted == false)
                                            .Select(e => new {
                                                userId = e.Id,
                                                secret = KeyGenerator.Decrypt(e.SecretKeyHash, e.DaeuAccaunt)
                                            }).ToListAsync();

                var userId = usersHashs.Where(uh => uh.secret == secretKey).Select(uh => uh.userId).FirstOrDefault();
                return userId;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public async Task<bool> AddSystemAccountMethod()
        {
            try
            {
                var sistemAccount = new Employee()
                {
                    FullName = DataConstants.SystemName,
                    DaeuAccaunt = DataConstants.SystemUsername,
                    Role = this.db.Roles.Where(r => r.Name == DataConstants.Employee).FirstOrDefault(),
                    Email = DataConstants.SystemEmail
                };
                var secret = KeyGenerator.GenerateRandomString();
                sistemAccount.SecretKeyHash = KeyGenerator.Encrypt(secret, sistemAccount.DaeuAccaunt);

                await this.db.Employees.AddAsync(sistemAccount);
                await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<int> GetSystemAccountId()
        {
            try
            {
                var systemId = await this.db.Employees.Where(e => e.DaeuAccaunt == DataConstants.SystemUsername).Select(e => e.Id).FirstOrDefaultAsync();
                return systemId;
            }
            catch (Exception)
            {
                return -99999;
            }
        }
    }
}
