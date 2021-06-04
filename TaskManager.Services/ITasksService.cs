using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;
using TaskManager.Services.Models.ReportModels;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services
{
    public interface ITasksService
    {
        Task<bool> CheckIfTaskIsClosed(int taskId);
        IQueryable<TaskInfoServiceModel> GetTaskDetails(int taskId);

        Task<int> AddNewTaskAsync(AddNewTaskServiceModel newTask);

        Task<string> CreateTasksStatusesAsync();

        Task<string> CreateTasksTypesAsync();

        Task<string> CreateTasksPrioritiesAsync();

        int TasksStatusCount();

        int TasksTypesCount();

        int TasksPrioritysCount();
        Task<string> SetWorkedHoursAsyncOld(TaskWorkedHoursServiceModel workedHours);

        Task<string> SetWorkedHoursAsync(TaskWorkedHoursServiceModel workedHours);
        Task<bool> CloseTaskAsync(int taskId, string endNote, int closerid);

        Task<bool> ReopenTaskAsync(int taskId);
        Task<string> EditTaskAsync(AddNewTaskServiceModel taskToEdit);
        Task<List<ReportServiceModel>> ExportTasksAsync(IList<int> employeesIds, DateTime startDate, DateTime endDate);
        Task<TaskServiceModel> GetParentTaskAsync(int parentTaskId);

        IQueryable<TaskManager.Data.Models.Task> GetAllTasks(bool withClosed = false, bool withDeleted = false);

        Task<bool> CheckTaskByIdAsync(int taskId);

        Task<string> MarkTaskDeletedAsync(int taskId, int userId);

        Task<string> MarkTaskActiveAsync(int taskId, int userId);
 
        IEnumerable<SelectServiceModel> GetParentTaskNames(int? directorateId);

        Task<List<TaskWorkedHoursServiceModel>> GetTaskReport(int taskId, DateTime startDate, DateTime endDate);
    }
}
