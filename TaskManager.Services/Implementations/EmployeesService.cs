using AutoMapper.QueryableExtensions;
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

        public async Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(int userId)
        {
            var tasks = await this.db.EmployeesTasks
                    .Where(et => et.EmployeeId == userId && et.isDeleted == false)
                    .Select(t => t.Task)
                    .Where(t => t.isDeleted == false)
                    .Distinct()
                    .OrderBy(t => t.PriorityId)
                    .ThenByDescending(t => t.EndDatePrognose)
                    .ProjectTo<TaskFewInfoServiceModel>(new { currentEmployeeId = userId })
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
                    .Where(et => et.AssignerId == userId && et.isDeleted == false)
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
                    .Where(t => t.isDeleted == withDeleted && t.isActive == true)
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
                .Where(c => c.isDeleted == false && c.isActive == true)
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


        public IEnumerable<SelectServiceModel> GetEmployeesNamesByDepartment(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }
            var names = this.db.Employees
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
                .ToList();
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

        public IEnumerable<SelectServiceModel> GetEmployeesNamesByDirectorate(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }
            var names = this.db.Employees
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
                .ToList();
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

        public IEnumerable<SelectServiceModel> GetEmployeesNamesBySector(int? sectorId)
        {
            if (sectorId == null)
            {
                return null;
            }
            var names = this.db.Employees
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
                .ToList();
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
                        userFromDB.TelephoneNumber = newUser.TelephoneNumber;
                        userFromDB.MobileNumber = newUser.MobileNumber;
                        userFromDB.JobTitleId = newUser.JobTitleId;
                        userFromDB.DirectorateId = newUser.DirectorateId;
                        userFromDB.DepartmentId = newUser.DepartmentId;
                        userFromDB.SectorId = newUser.SectorId;
                        userFromDB.DaeuAccaunt = newUser.DaeuAccaunt;
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
                            TelephoneNumber = newUser.TelephoneNumber,
                            MobileNumber = newUser.MobileNumber,
                            JobTitleId = newUser.JobTitleId,
                            DirectorateId = newUser.DirectorateId,
                            DepartmentId = newUser.DepartmentId,
                            SectorId = newUser.SectorId,
                            DaeuAccaunt = newUser.DaeuAccaunt,
                            RoleId = await this.db.Roles.Where(r => r.Name == DataConstants.Employee).Select(r => r.Id).FirstOrDefaultAsync(),
                            isActive = false
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
    }
}
