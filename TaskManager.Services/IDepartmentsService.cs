using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface IDepartmentsService
    {
        Task<string> AddDepartmentsCollection(List<AddNewDepartmentServiceModel> departments);

        IEnumerable<SelectServiceModel> GetDepartmentsNames();

        IEnumerable<SelectServiceModel> GetDepartmentsNames(int? departmentId);

        IEnumerable<SelectServiceModel> GetDepartmentsNamesByDirectorate(int? directorateId);

        Task<List<AddNewDepartmentServiceModel>> GetDepartmentsAsync(bool deleted = false);

        Task<AddNewDepartmentServiceModel> GetDepartmentAsync(int depId);

        Task<string> MarkDepartmentDeleted(int depId);
        Task<string> CreateDepartmentAsync(int directoratesId, string departmentName);
        Task<string> MarkDepartmentActiveAsync(int depId);
        Task<string> EditDepartmentDetails(int depId, int directoratesId, string departmentName);

        bool CheckDepartmentInDirectorate(int dirId, int depId);
        Task<string> AproveDepReportsAsync(int depId, DateTime aproveDate, int adminId);
    }
}
