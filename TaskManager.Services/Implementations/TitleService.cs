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
    public class TitleService : ITitleService
    {

        private readonly TasksDbContext db;
        public TitleService(TasksDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public async Task<string> AddTitlesCollection(List<AddNewJobTitlesServiceModel> jobTypes)
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
                for (i = 0; i <= jobTypes.Count() - 1; i++)
                {

                    var newJobTypeDB = new JobTitle()
                    {
                        //Id = jobTypes[i].JobTitleId,
                        TitleName = jobTypes[i].Name,
                        isDeleted = jobTypes[i].isDeleted
                    };
                    await this.db.JobTitles.AddAsync(newJobTypeDB);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception)
            {
                this.db.Database.RollbackTransaction();
                //comend.ExecuteNonQuery();
                con.Close();
                return string.Concat($"Ред N:{i}", " ", jobTypes[i].Name, " има грешка (плюс минус един ред!)");
            }
            this.db.Database.CommitTransaction();
            //transaction.Commit();
            //comend.ExecuteNonQuery();
            con.Close();
            return "success";
        }

        public IEnumerable<SelectServiceModel> GetJobTitlesNames()
        {
            var names = this.db.JobTitles
                .Where(c => c.isDeleted == false)
                .OrderBy(j => j.TitleName)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.TitleName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public async Task<List<SelectServiceModel>> GetJobTitlesAsync(bool deleted = false)
        {
            var result = await this.db.JobTitles.Where(d => d.isDeleted == deleted)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.TitleName,
                    Id = d.Id,
                    isDeleted = d.isDeleted,
                    Count = this.db.Employees.Where(e => e.JobTitleId == d.Id).Count()
                }).ToListAsync();
            return result;
        }

        public async Task<string> MarkTitleDeleted(int jobId)
        {
            var jobToDelete = await this.db.JobTitles.FirstOrDefaultAsync(d => d.Id == jobId);
            if (jobToDelete == null)
            {
                return $"Няма длъжност с номер: {jobId}";
            }

            var check = await this.CheckTitlesByIdAsync(jobId);
            if (check != "success")
            {
                return check;
            }
            jobToDelete.isDeleted = true;
            await this.db.SaveChangesAsync();
            return "success";
        }
        private async Task<string> CheckTitlesByIdAsync(int jobId)
        {
            int employeesInTitle = await this.db.Employees.Where(s => s.JobTitleId == jobId && s.isDeleted == false).CountAsync();
            if (employeesInTitle > 0)
            {
                return "Има служители с тази длъжност.";
            }
            return "success";
        }

        public async Task<string> MarkTitleActiveAsync(int jobId)
        {
            var titleToRestore = await this.db.JobTitles.FirstOrDefaultAsync(d => d.Id == jobId);
            if (titleToRestore == null)
            {
                return $"Няма длъжност с номер: {jobId}";
            }
            titleToRestore.isDeleted = false;
            await this.db.SaveChangesAsync();
            return "success";
        }

        public async Task<string> CreateTitleAsync(string titleName)
        {
            try
            {
                var newTitle = new JobTitle();
                newTitle.TitleName = titleName;
                await this.db.JobTitles.AddAsync(newTitle);
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> RenameTitleAsync(int jobId, string titleName)
        {
            var titleToRename = await this.db.JobTitles.FirstOrDefaultAsync(d => d.Id == jobId);
            if (titleToRename == null)
            {
                return $"Няма дирекция с номер: {jobId}";
            }
            titleToRename.TitleName = titleName;
            await this.db.SaveChangesAsync();
            return "success";

        }

    }
}
