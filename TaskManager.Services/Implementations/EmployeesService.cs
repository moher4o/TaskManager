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
                        JobTitleId = string.IsNullOrWhiteSpace(employees[i].JobTitle) ? throw new ArgumentNullException("Длъжността не може да е null") : this.db.JobTitles.Where(d => d.Name == employees[i].JobTitle).Select(d => d.Id).FirstOrDefault(),
                        RoleId = string.IsNullOrWhiteSpace(employees[i].Role) ? throw new ArgumentNullException("Ролята не може да е null") : this.db.Roles.Where(d => d.Name == employees[i].Role).Select(d => d.Id).FirstOrDefault(),
                        SectorId = string.IsNullOrWhiteSpace(employees[i].Sector) ? (int?) null : this.db.Sectors.Where(d => d.Name == employees[i].Sector).Select(d => d.Id).FirstOrDefault(),
                        DepartmentId = string.IsNullOrWhiteSpace(employees[i].Department) ? (int?)null : this.db.Departments.Where(d => d.Name == employees[i].Department).Select(d => d.Id).FirstOrDefault(),
                        DirectorateId = string.IsNullOrWhiteSpace(employees[i].Directorate) ? (int?)null : this.db.Directorates.Where(d => d.Name == employees[i].Directorate).Select(d => d.Id).FirstOrDefault(),
                        FullName = employees[i].FullName,
                        isDeleted = employees[i].isDeleted
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

        public UserCookyServiceModel GetUserDataForCooky(string daeuAccaunt)
        {
            var searchedUser = this.db.Employees
                .Where(u => u.DaeuAccaunt == daeuAccaunt.ToLower())
                .ProjectTo<UserCookyServiceModel>()
                .FirstOrDefault();

            return searchedUser;
        }
    }
}
