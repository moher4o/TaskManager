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

        IQueryable<TaskInfoServiceModel> GetTaskDetails(int taskId);

        Task<string> AddNewTaskAsync(AddNewTaskServiceModel newTask);

        Task<string> CreateTasksStatusesAsync();

        Task<string> CreateTasksTypesAsync();

        Task<string> CreateTasksPrioritiesAsync();

        int TasksStatusCount();

        int TasksTypesCount();

        int TasksPrioritysCount();
        Task<string> SetWorkedHoursAsync(TaskWorkedHoursServiceModel workedHours);
        Task<bool> CloseTaskAsync(int taskId, string endNote, int closerid);

        Task<bool> ReopenTaskAsync(int taskId);
        Task<string> EditTaskAsync(AddNewTaskServiceModel taskToEdit);
        Task<List<ReportServiceModel>> ExportTasksAsync(IList<int> employeesIds, DateTime startDate, DateTime endDate);
    }
}
