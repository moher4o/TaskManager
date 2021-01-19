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
    public class SectorsService : ISectorsService
    {
        private readonly TasksDbContext db;
        public SectorsService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> AddSectorsCollection(List<AddNewSectorServiceModel> sectors)
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
                for (i = 0; i <= sectors.Count() - 1; i++)
                {

                    var newSectorDB = new Sector()
                    {
                        DepartmentId = sectors[i].DepartmentId,
                        Name = sectors[i].Name,
                        isDeleted = sectors[i].isDeleted
                    };

                    newSectorDB.DirectorateId = this.db.Departments.Where(d => d.Id == sectors[i].DepartmentId).Select(d => d.DirectorateId).FirstOrDefault();

                    await this.db.Sectors.AddAsync(newSectorDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", sectors[i].Name, " има грешка (плюс минус един ред!)");
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        public IEnumerable<string> GetSectorsNames()
        {
            var names = this.db.Sectors.Where(c => c.isDeleted == false).Select(c => c.Name).ToList();
            return names;
        }

        public IEnumerable<string> GetSectorsNames(int? sectorId)
        {
            if (sectorId == null)
            {
                return null;
            }
            var names = this.db.Sectors.Where(c => c.isDeleted == false && c.Id == sectorId).Select(c => c.Name).ToList();
            return names;
        }
        public IEnumerable<string> GetSectorsNamesByDepartment(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }
            var names = this.db.Sectors.Where(c => c.isDeleted == false && c.DepartmentId == departmentId ).Select(c => c.Name).ToList();
            return names;
        }

        public IEnumerable<string> GetSectorsNamesByDirectorate(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }
            var names = this.db.Sectors.Where(c => c.isDeleted == false && c.DirectorateId == directorateId).Select(c => c.Name).ToList();
            return names;
        }


    }
}
