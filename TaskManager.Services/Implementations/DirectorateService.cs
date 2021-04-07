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
    public class DirectorateService : IDirectorateService
    {
        private readonly TasksDbContext db;
        public DirectorateService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> CreateDirectorateAsync(string directorateName)
        {
            try
            {
                var newDirectorate = new Directorate();
                newDirectorate.DirectorateName = directorateName;
                await this.db.Directorates.AddAsync(newDirectorate);
                await this.db.SaveChangesAsync();
                return "success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> RenameDirectorateAsync(int dirId, string directorateName)
        {
            var directorateToRename = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == dirId);
            if (directorateToRename == null)
            {
                return $"Няма дирекция с номер: {dirId}";
            }
            directorateToRename.DirectorateName = directorateName;
            await this.db.SaveChangesAsync();
            return "success";

        }

        public async Task<string> MarkDirectorateDeleted(int dirId)
        {
            var directorateToDelete = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == dirId);
            if (directorateToDelete == null)
            {
                return $"Няма дирекция с номер: {dirId}";
            }

            var check = await this.CheckDirectorateByIdAsync(dirId);
            if (check != "success")
            {
                return check;
            }
            directorateToDelete.isDeleted = true;
            await this.db.SaveChangesAsync();
            return "success";
        }
        private async Task<string> CheckDirectorateByIdAsync(int dirId)
        {
            int employeesInDirectorate = await this.db.Employees.Where(s => s.DirectorateId == dirId && s.isDeleted == false).CountAsync();
            if (employeesInDirectorate > 0)
            {
                return "Има назначени служители в дирекцията. Преместете ги в друга дирекция преди да бъде изтрита тази.";
            }
            int tasksInDirectorate = await this.db.Tasks.Where(s => s.DirectorateId == dirId && s.isDeleted == false).CountAsync();
            if (tasksInDirectorate > 0)
            {
                return "Има задачи в дирекцията. Преместете ги в друга дирекция преди да бъде изтрита тази.";
            }

            int departmentInDirectorate = await this.db.Departments.Where(s => s.DirectorateId == dirId && s.isDeleted == false).CountAsync();
            if (departmentInDirectorate > 0)
            {
                return "Има отдели в дирекцията. Преместете ги в друга дирекция преди да бъде изтрита тази.";
            }

            int sectorsInDirectorate = await this.db.Sectors.Where(s => s.DirectorateId == dirId && s.isDeleted == false).CountAsync();
            if (sectorsInDirectorate > 0)
            {
                return "Има сектори в дирекцията. Преместете ги в друга дирекция преди да бъде изтрита тази.";
            }

            return "success";
        }
        public async Task<List<AddNewDirectorateServiceModel>> GetDirectoratesAsync(bool deleted = false)
        {
            var result = await this.db.Directorates.Where(d => d.isDeleted == deleted)
                .OrderByDescending(d => d.Employees.Count)
                .Select(d => new AddNewDirectorateServiceModel
                {
                    Name = d.DirectorateName,
                    Id = d.Id,
                    isDeleted = d.isDeleted
                }).ToListAsync();
            return result;
        }

        public async Task<string> AddDirectoratesCollection(List<AddNewDirectorateServiceModel> directorates)
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
                for (i = 0; i <= directorates.Count() - 1; i++)
                {

                    var newDirectorateDB = new Directorate()
                    {
                        //DirectorateId = directorates[i].DirectorateId,
                        DirectorateName = directorates[i].Name,
                        isDeleted = directorates[i].isDeleted
                    };
                    await this.db.Directorates.AddAsync(newDirectorateDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", directorates[i].Name, " има грешка (плюс минус един ред!)");
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        //public IEnumerable<SelectServiceModel> GetDirectoratesNames()
        //{
        //    var names = this.db.Directorates
        //        .Where(c => c.isDeleted == false)
        //        .Select(d => new SelectServiceModel {
        //        TextValue = d.DirectorateName,
        //        Id = d.Id
        //        })
        //        .ToList();
        //    return names;
        //}

        public IEnumerable<SelectServiceModel> GetDirectoratesNames(int? directorateId)
        {
            var names = new List<SelectServiceModel>();
            if (directorateId == null)
            {
                names = this.db.Directorates
                    .Where(c => c.isDeleted == false)
                    .Select(d => new SelectServiceModel
                    {
                        TextValue = d.DirectorateName,
                        Id = d.Id
                    })
                    .ToList();
                return names;
            }
            names = this.db.Directorates
                .Where(c => c.isDeleted == false && c.Id == directorateId)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.DirectorateName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public async Task<string> MarkDirectorateActiveAsync(int dirId)
        {
            var directorateToRestore = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == dirId);
            if (directorateToRestore == null)
            {
                return $"Няма дирекция с номер: {dirId}";
            }
            directorateToRestore.isDeleted = false;
            await this.db.SaveChangesAsync();
            return "success";

        }


    }
}
