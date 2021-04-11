﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;
using TaskManager.Services.Models.EmployeeModels;
using TaskManager.Services.Models.ReportModels;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services
{
    public interface IEmployeesService
    {

        
        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(int userId);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserAssignerTaskAsync(int userId);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserCreatedTaskAsync(int userId);

        Task<string> AddEmployeesCollection(List<AddNewEmployeeServiceModel> employees);

        UserServiceModel GetUserDataForCooky(string daeuAccaunt);

        Task<string> GetEmployeeNameByIdAsync(int userId);

        Task<UserServiceModel> GetEmployeeByIdAsync(int userId);

        IEnumerable<SelectServiceModel> GetActiveEmployeesNames();

        IEnumerable<SelectServiceModel> GetEmployeesNamesBySector(int? sectorId);
        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesBySectorWithDeletedAsync(int? sectorId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDepartment(int? departmentId);
        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDepartmentWithDeletedAsync(int? departmentId);

        IEnumerable<SelectServiceModel> GetEmployeesNamesByDirectorate(int? directorateId);
        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDirectorateWithDeletedAsync(int? directorateId);

        IEnumerable<ReportUserServiceModel> GetEmployeesByList(IEnumerable<int> employeesList);
        Task<bool> RegisterNewUserAsync(UserServiceModel newUser);
        Task<string> DeactivateUserAsync(int userId);
        Task<bool> АctivateUserAsync(int userId);

        Task<IList<UserServiceModel>> GetAllUsers(bool withDeleted = false);
        Task<List<UserServiceModel>> GetAllNotActivatedUsersAsync();

        Task<EmployeeServiceModel> GetPersonalReport(int userId, DateTime startDate, DateTime endDate);
    }
}
