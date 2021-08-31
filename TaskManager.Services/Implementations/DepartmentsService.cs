using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Services.Models;

namespace TaskManager.Services.Implementations
{
    public class DepartmentsService : IDepartmentsService
    {
        private readonly TasksDbContext db;
        public DepartmentsService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> AddDepartmentsCollection(List<AddNewDepartmentServiceModel> departments)
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
                for (i = 0; i <= departments.Count() - 1; i++)
                {

                    var newDepartmentDB = new Department()
                    {
                        DirectorateId = departments[i].DirectorateId,
                        DepartmentName = departments[i].Name,
                        isDeleted = departments[i].isDeleted
                    };
                    await this.db.Departments.AddAsync(newDepartmentDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", departments[i].Name, " има грешка (плюс минус един ред!)");
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        public IEnumerable<SelectServiceModel> GetDepartmentsNames()
        {
            var names = this.db.Departments
                .Where(c => c.isDeleted == false)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.DepartmentName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public async Task<List<AddNewDepartmentServiceModel>> GetDepartmentsAsync(bool deleted = false)
        {
            var result = await this.db.Departments.Where(d => d.isDeleted == deleted)
                .OrderByDescending(d => d.Employees.Count)
                .Select(d => new AddNewDepartmentServiceModel
                {
                    Name = d.DepartmentName,
                    Id = d.Id,
                    DirectorateName = d.Directorate.DirectorateName,
                    isDeleted = d.isDeleted
                }).ToListAsync();
            return result;
        }

        public async Task<AddNewDepartmentServiceModel> GetDepartmentAsync(int depId)
        {
            var result = await this.db.Departments.Where(d => d.Id == depId)
                .Select(d => new AddNewDepartmentServiceModel
                {
                    Name = d.DepartmentName,
                    Id = d.Id,
                    DirectorateName = d.Directorate.DirectorateName,
                    DirectorateId = d.DirectorateId,
                    isDeleted = d.isDeleted
                }).FirstOrDefaultAsync();
            return result;
        }


        public IEnumerable<SelectServiceModel> GetDepartmentsNames(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }

            var names = this.db.Departments
                .Where(c => c.isDeleted == false && c.Id == departmentId)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.DepartmentName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public IEnumerable<SelectServiceModel> GetDepartmentsNamesByDirectorate(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }

            var names = this.db.Departments
                .Where(c => c.isDeleted == false && c.DirectorateId == directorateId)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.DepartmentName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public async Task<string> MarkDepartmentDeleted(int depId)
        {
            var departmentToDelete = await this.db.Departments
                .FirstOrDefaultAsync(d => d.Id == depId);
            if (departmentToDelete == null)
            {
                return $"Няма отдел с номер: {depId}";
            }

            var check = await this.CheckDepartmentByIdAsync(depId);
            if (check != "success")
            {
                return check;
            }
            departmentToDelete.isDeleted = true;
            await this.db.SaveChangesAsync();
            return "success";
        }

        private async Task<string> CheckDepartmentByIdAsync(int depId)
        {
            try
            {
            var checkedDepartment = await this.db.Departments
                .Where(d => d.Id == depId)
                .Include(d => d.Sectors)
                .Include(d => d.Tasks)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync();
            int employeesInDepartment = checkedDepartment.Employees.Count();
            if (employeesInDepartment > 0)
            {
                return "Има назначени служители в отдела(може да са неактивни). Преместете ги в друг отдел преди да бъде редактиран/изтрит този.";
            }
            int tasksInDepartment = checkedDepartment.Tasks.Count();
            if (tasksInDepartment > 0)
            {
                return "Има задачи в отдела(може да са маркирани изтрити или приключени). Преместете ги в друг отдел преди да бъде редактиран/изтрит този.";
            }

            int sectorsInDepartment = checkedDepartment.Sectors.Count();
            if (sectorsInDepartment > 0)
            {
                return "Има сектори в отдела(може да са маркирани изтрити). Преместете ги в друг отдел преди да бъде редактиран/изтрит този.";
            }

            return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> CreateDepartmentAsync(int directoratesId, string departmentName)
        {
            try
            {

                var directorate = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == directoratesId);
                if (directorate == null)
                {
                    return $"Няма дирекция с номер: ${directoratesId}";
                }
                var newDepartment = new Department()
                {
                    DirectorateId = directoratesId,
                    DepartmentName = departmentName
                };
                await this.db.Departments.AddAsync(newDepartment);
                await this.db.SaveChangesAsync();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> EditDepartmentDetails(int depId, int directorateId, string departmentName)
        {
            try
            {
                var departmentToEdit = await this.db.Departments.FirstOrDefaultAsync(d => d.Id == depId);
                if (departmentToEdit == null)
                {
                    return $"Няма отдел с номер: {depId}";
                }
                if (departmentToEdit.DirectorateId != directorateId)
                {
                    var check = await CheckDepartmentByIdAsync(depId);
                    if (check != "success")
                    {
                        return check;
                    }
                    var directorateTarget = await this.db.Directorates.Where(d => d.Id == directorateId && d.isDeleted == false).FirstOrDefaultAsync();
                    if (directorateTarget == null)
                    {
                        return $"Няма активна дирекция с номер: {directorateId}";
                    }
                    departmentToEdit.DirectorateId = directorateTarget.Id;
                }
                if (string.IsNullOrWhiteSpace(departmentName))
                {
                    return "Името на отдела не може да е празен стринг!";
                }

                departmentToEdit.DepartmentName = departmentName;
                await this.db.SaveChangesAsync();
                return "success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public async Task<string> MarkDepartmentActiveAsync(int depId)
        {
            var departmentToRestore = await this.db.Departments.FirstOrDefaultAsync(d => d.Id == depId);
            if (departmentToRestore == null)
            {
                return $"Няма отдел с номер: {depId}";
            }
            var directorateHost = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == departmentToRestore.DirectorateId);
            if (directorateHost.isDeleted == true)
            {
                return $"Дирекцията {directorateHost.DirectorateName}, към която принадлежи отдела е изтрита. Възстановете първо дирекцията.";
            }
            departmentToRestore.isDeleted = false;
            await this.db.SaveChangesAsync();
            return "success";

        }

        public bool CheckDepartmentInDirectorate(int dirId, int depId)
        {
            var departmentsInDir = this.db.Departments.Where(d => d.DirectorateId == dirId).Select(d => d.Id).ToArray();
            if (departmentsInDir.Contains(depId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
