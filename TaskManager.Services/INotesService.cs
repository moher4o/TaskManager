using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models.NotesModels;

namespace TaskManager.Services
{
    public interface INotesService
    {
        Task<TaskNotesListServiceModel> GetTaskNotesAsync(int taskId);
    }
}
