using Microsoft.Data.SqlClient;
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
using static TaskManager.Common.DataConstants;

namespace TaskManager.Services.Implementations
{
    public class RolesService : IRolesService
    {
        private readonly TasksDbContext db;
        public RolesService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> CreateRolesAsync()
        {
            await this.db.Database.BeginTransactionAsync();
            try
            {
                var roles = this.db.Roles.ToList();
                this.db.Roles.RemoveRange(roles);
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при изтриването на ролите.";
            }
            this.db.Database.CommitTransaction();

            await this.db.Database.BeginTransactionAsync();
            try
            {

                var newRoleDB = new Role()
                {
                    Name = DataConstants.SuperAdmin,
                    isDeleted = false,
                };
                await this.db.Roles.AddAsync(newRoleDB);
                newRoleDB = new Role()
                {
                    Name = DataConstants.DirectorateAdmin,
                    isDeleted = false,
                };
                await this.db.Roles.AddAsync(newRoleDB);
                newRoleDB = new Role()
                {
                    Name = DataConstants.DepartmentAdmin,
                    isDeleted = false,
                };
                await this.db.Roles.AddAsync(newRoleDB);
                newRoleDB = new Role()
                {
                    Name = DataConstants.SectorAdmin,
                    isDeleted = false,
                };
                await this.db.Roles.AddAsync(newRoleDB);
                newRoleDB = new Role()
                {
                    Name = DataConstants.Employee,
                    isDeleted = false,
                };
                await this.db.Roles.AddAsync(newRoleDB);

                await this.db.SaveChangesAsync();

                var firstSuperAdmin = new Employee()
                {
                    FullName = string.Concat(DataConstants.DeveloperFirstName, " ", DataConstants.DeveloperLastName),
                    DaeuAccaunt = DataConstants.DeveloperUsername,
                    Role = this.db.Roles.Where(r => r.Name == DataConstants.SuperAdmin).FirstOrDefault(),
                    Email = DataConstants.DeveloperEmail
                };
                await this.db.Employees.AddAsync(firstSuperAdmin);

                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                return $"Грешка!!! Възникна проблем при създаването на ролите.";
            }
            this.db.Database.CommitTransaction();
            return "success";
        }

        public async Task<string> AddRolesCollection(List<AddNewRoleServiceModel> roles)
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
                for (i = 0; i <= roles.Count() - 1; i++)
                {

                    var newRoleDB = new Role()
                    {
                        //Id = jobTypes[i].JobTitleId,
                        Name = roles[i].Name,
                        isDeleted = roles[i].isDeleted
                    };
                    await this.db.Roles.AddAsync(newRoleDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", roles[i].Name, " има грешка (плюс минус един ред!)");
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        public int RolesCount()
        {
            return this.db.Roles.Count();
        }

        public async Task<string> GetUserRoleNameByRoleIdAsync(int roleId)
        {
            return await this.db.Roles.Where(r => r.Id == roleId).Select(r => r.Name).FirstOrDefaultAsync();
        }

        public async Task<int> GetUserRoleIdByRoleNameAsync(string roleName)
        {
            return await this.db.Roles.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefaultAsync();
        }


        public IEnumerable<SelectServiceModel> GetAllRoles()
        {
            var roles = this.db.Roles
                .Where(c => c.isDeleted == false)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.Name,
                    Id = d.Id
                })
                .ToList();
            return roles;
        }
    }
}
