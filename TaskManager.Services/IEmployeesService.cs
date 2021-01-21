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

        IEnumerable<string> GetEmployeesNames();

        IEnumerable<string> GetEmployeesNamesBySector(int? sectorId);

        IEnumerable<string> GetEmployeesNamesByDepartment(int? departmentId);

        IEnumerable<string> GetEmployeesNamesByDirectorate(int? directorateId);

    }
}
