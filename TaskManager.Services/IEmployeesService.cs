using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services
{
    public interface IEmployeesService
    {
        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(int userId);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserAssignerTaskAsync(int userId);

        Task<string> AddEmployeesCollection(List<AddNewEmployeeServiceModel> employees);

        UserServiceModel GetUserDataForCooky(string daeuAccaunt);

        Task<string> GetEmployeeNameByIdAsync(int userId);

        IEnumerable<SelectServiceModel> GetEmployeesNames();

        IEnumerable<SelectServiceModel> GetEmployeesNamesBySector(int? sectorId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDepartment(int? departmentId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDirectorate(int? directorateId);

    }
}
