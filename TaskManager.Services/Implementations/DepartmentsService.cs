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
                        Name = departments[i].Name,
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
    }
}
