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
        Task<bool> AddNoteAsync(string text, int taskId, int userId);
        Task<bool> DeleteNoteAsync(int noteId);
        Task<string> GetNoteText(int noteId);
        Task<string> SetNoteText(int noteId, string noteText);
        Task<int> GetNoteEmployeeIdAsync(int noteId);
    }
}
