using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface IEmployeesService
    {
        Task<string> AddEmployeesCollection(List<AddNewEmployeeServiceModel> employees);

        UserServiceModel GetUserDataForCooky(string daeuAccaunt);

        IEnumerable<SelectServiceModel> GetEmployeesNames();

        IEnumerable<SelectServiceModel> GetEmployeesNamesBySector(int? sectorId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDepartment(int? departmentId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDirectorate(int? directorateId);

    }
}
