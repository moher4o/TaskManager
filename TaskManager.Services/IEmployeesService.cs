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

        UserCookyServiceModel GetUserDataForCooky(string daeuAccaunt);
    }
}
