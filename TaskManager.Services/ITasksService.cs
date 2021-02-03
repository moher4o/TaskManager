using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services
{
    public interface ITasksService
    {

        Task<string> AddNewTaskAsync(AddNewTaskServiceModel newTask);

        Task<string> CreateTasksStatusesAsync();

        Task<string> CreateTasksTypesAsync();

        Task<string> CreateTasksPrioritiesAsync();

        int TasksStatusCount();

        int TasksTypesCount();

        int TasksPrioritysCount();


    }
}
