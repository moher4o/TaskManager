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
                        SectorName = sectors[i].Name,
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

        public async Task<string> CreateSectorAsync(int directoratesId, int depId, string sectorName)
        {
            try
            {

                var directorate = await this.db.Directorates.FirstOrDefaultAsync(d => d.Id == directoratesId);
                if (directorate == null)
                {
                    return $"Няма дирекция с номер: ${directoratesId}";
                }
                var department = await this.db.Departments.FirstOrDefaultAsync(d => d.Id == depId);
                if (department == null)
                {
                    return $"Няма отдел с номер: ${depId}";
                }

                var newSector = new Sector()
                {
                    DepartmentId = depId,
                    DirectorateId = directoratesId,
                    SectorName = sectorName
                };
                this.db.Sectors.Add(newSector);
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public IEnumerable<SelectServiceModel> GetSectorsNames()
        {
            var names = this.db.Sectors
                .Where(c => c.isDeleted == false)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.SectorName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }

        public async Task<List<AddNewSectorServiceModel>> GetSectorsAsync(bool deleted = false)
        {
            var result = await this.db.Sectors.Where(d => d.isDeleted == deleted)
                .Select(d => new AddNewSectorServiceModel
                {
                    Name = d.SectorName,
                    Id = d.Id,
                    DirectorateName = d.Directorate.DirectorateName,
                    DirectorateId = d.DirectorateId.Value,
                    DepartmentName = d.Department.DepartmentName,
                    DepartmentId = d.DepartmentId.Value,
                    isDeleted = d.isDeleted
                }).ToListAsync();
            return result;
        }

        public async Task<AddNewSectorServiceModel> GetSectorAsync(int secId)
        {
            var result = await this.db.Sectors.Where(d => d.Id == secId)
                .Select(d => new AddNewSectorServiceModel
                {
                    Name = d.SectorName,
                    Id = d.Id,
                    DirectorateName = d.Directorate.DirectorateName,
                    DirectorateId = d.DirectorateId.Value,
                    DepartmentName = d.Department.DepartmentName,
                    DepartmentId = d.DepartmentId.Value,
                    isDeleted = d.isDeleted
                }).FirstOrDefaultAsync();
            return result;
        }

        public IEnumerable<SelectServiceModel> GetSectorsNames(int? sectorId)
        {
            if (sectorId == null)
            {
                return null;
            }
            var names = this.db.Sectors
                .Where(c => c.isDeleted == false && c.Id == sectorId)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.SectorName,
                    Id = d.Id
                })
                .ToList();
            return names;
        }
        public async Task<IEnumerable<SelectServiceModel>> GetSectorsNamesByDepartment(int? departmentId)
        {
            if (departmentId == null)
            {
                return null;
            }
            var names = await this.db.Sectors
                .Where(c => c.isDeleted == false && c.DepartmentId == departmentId )
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.SectorName,
                    Id = d.Id
                })
                .ToListAsync();
            return names;
        }

        public async Task<IEnumerable<SelectServiceModel>> GetSectorsNamesByDirectorate(int? directorateId)
        {
            if (directorateId == null)
            {
                return null;
            }
            var names = await this.db.Sectors
                .Where(c => c.isDeleted == false && c.DirectorateId == directorateId)
                .Select(d => new SelectServiceModel
                {
                    TextValue = d.SectorName,
                    Id = d.Id
                })
                .ToListAsync();
            return names;
        }

        public async Task<string> MarkSectorDeleted(int secId)
        {
            var sectorToDelete = await this.db.Sectors
                .FirstOrDefaultAsync(d => d.Id == secId);
            if (sectorToDelete == null)
            {
                return $"Няма сектор с номер: {secId}";
            }

            var check = await this.CheckSectorByIdAsync(secId);
            if (check != "success")
            {
                return check;
            }
            sectorToDelete.isDeleted = true;
            await this.db.SaveChangesAsync();
            return "success";
        }

        private async Task<string> CheckSectorByIdAsync(int secId)
        {
            try
            {
                var checkedSector = await this.db.Sectors
                    .Where(d => d.Id == secId)
                    .Include(d => d.Tasks)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync();
                int employeesInSector = checkedSector.Employees.Count();
                if (employeesInSector > 0)
                {
                    return "Има назначени служители в сектора(може да са неактивни). Преместете ги в друг сектор преди да бъде редактиран/изтрит този.";
                }
                int tasksInSector = checkedSector.Tasks.Count();
                if (tasksInSector > 0)
                {
                    return "Има задачи в сектора(може и да са маркирани изтрити или приключени). Преместете ги в друг сектор преди да бъде редактиран/изтрит този.";
                }

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> MarkSectorActiveAsync(int secId)
        {
            var sectorToRestore = await this.db.Sectors.FirstOrDefaultAsync(d => d.Id == secId);
            if (sectorToRestore == null)
            {
                return $"Няма сектор с номер: {secId}";
            }
            var departmentHost = await this.db.Departments.FirstOrDefaultAsync(d => d.Id == sectorToRestore.DepartmentId);
            if (departmentHost.isDeleted == true)
            {
                return $"Отдела {departmentHost.DepartmentName}, към който принадлежи сектора е изтрит. Възстановете първо отдела.";
            }
            sectorToRestore.isDeleted = false;
            await this.db.SaveChangesAsync();
            return "success";

        }

        public async Task<string> EditSectorAsync(int secId, int directoratesId, int departmentsId, string sectorName)
        {
            try
            {
                var sectorToEdit = await this.db.Sectors.FirstOrDefaultAsync(d => d.Id == secId);
                if (sectorToEdit == null)
                {
                    return $"Няма сектор с номер: {secId}";
                }
                if (sectorToEdit.DepartmentId != departmentsId)
                {
                    var check = await CheckSectorByIdAsync(secId);
                    if (check != "success")
                    {
                        return check;
                    }
                    var directorateTarget = await this.db.Directorates.Where(d => d.Id == directoratesId && d.isDeleted == false).FirstOrDefaultAsync();
                    if (directorateTarget == null)
                    {
                        return $"Няма активна дирекция с номер: {directoratesId}";
                    }
                    sectorToEdit.DirectorateId = directorateTarget.Id;

                    var departmentTarget = await this.db.Departments.Where(d => d.Id == departmentsId && d.isDeleted == false).FirstOrDefaultAsync();
                    if (departmentTarget == null)
                    {
                        return $"Няма активен отдел с номер: {departmentsId}";
                    }
                    sectorToEdit.DepartmentId = departmentTarget.Id;

                }
                if (string.IsNullOrWhiteSpace(sectorName))
                {
                    return "Името на сектора не може да е празен стринг!";
                }

                sectorToEdit.SectorName = sectorName;
                await this.db.SaveChangesAsync();
                return "success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> AproveSecReportsAsync(int unitId, DateTime aproveDate, int adminId)
        {
            try
            {

                var sectorToAprove = await this.db.Sectors.FirstOrDefaultAsync(d => d.Id == unitId);
                if (sectorToAprove == null)
                {
                    return $"Няма сектор с номер: {unitId}";
                }

                var secEmployeesIds = await this.db.Employees.Where(e => e.SectorId == unitId && e.isDeleted == false).Select(e => e.Id).ToListAsync();
                var reports = await this.db.WorkedHours.Where(wh => secEmployeesIds.Contains(wh.EmployeeId) && wh.isDeleted == false && wh.Approved == false && wh.WorkDate.Date <= aproveDate).ToListAsync();
                foreach (var report in reports)
                {
                    report.Approved = true;
                    report.ApprovedBy = adminId;
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
