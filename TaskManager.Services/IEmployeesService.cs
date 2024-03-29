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

        Task<IEnumerable<SelectServiceModel>> GetUserDominions(int userId);
        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(int userId, DateTime dateToProcess);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserActiveTaskAsync(string userName, DateTime dateToProcess);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserAssignerTaskAsync(int userId);

        Task<IEnumerable<TaskFewInfoServiceModel>> GetUserCreatedTaskAsync(int userId);

        Task<string> AddEmployeesCollection(List<AddNewEmployeeServiceModel> employees);

        UserServiceModel GetUserDataForCooky(string daeuAccaunt);

        Task<string> GetEmployeeNameByIdAsync(int userId);

        Task<UserServiceModel> GetEmployeeByIdAsync(int userId);

        IEnumerable<SelectServiceModel> GetActiveEmployeesNames();

         Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesBySectorAsync(int? sectorId);

        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesBySectorWithDeletedAsync(int? sectorId);

         Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDepartmentAsync(int? departmentId);
        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDepartmentWithDeletedAsync(int? departmentId);

        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDirectorateAsync(int? directorateId);
        Task<IEnumerable<SelectServiceModel>> GetEmployeesNamesByDirectorateWithDeletedAsync(int? directorateId);

        IEnumerable<ReportUserServiceModel> GetEmployeesByList(IEnumerable<int> employeesList);
        Task<bool> RegisterNewUserAsync(UserServiceModel newUser);
        Task<string> SetSecretKey(int userId);
        Task<string> DeactivateUserAsync(int userId);
        Task<bool> АctivateUserAsync(int userId);

        Task<IList<UserServiceModel>> GetAllUsers(bool withDeleted = false);
        Task<List<UserServiceModel>> GetAllNotActivatedUsersAsync();

        Task<ShortEmployeeServiceModel> GetPersonalReport(int userId, DateTime startDate, DateTime endDate);

        Task<List<PersonalDateReportServiceModel>> GetDateReport(int userId, DateTime currentDate);
        Task<string> GenerateEmailWhenEmpty();

        Task<bool> AddAllToSystemTasksAsync();

        Task<bool> AddSystemAccountMethod();

        Task<IEnumerable<TaskFewInfoServiceModel>> GetAllUserTaskAsync(int userId, DateTime dateToProcess);

        Task<bool> MarkUserReadMessage(int userId);

        Task<bool> Set2FAFlagEnabled(int userId);
        Task<bool> Set2FADisabled(int userId);
        Task<string> GenerateSecretKeyWhenEmpty();

        Task<string> GetUserNameBySKAsync(string secretKey);

        Task<int> GetUserIdBySKAsync(string secretKey);

        Task<string> AddTokenHash(int userId, string token);

        Task<string> GetMobileToken(int userId);
        Task<int> GetSystemAccountId();
    }
}
