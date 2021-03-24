using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Services.Models.NotesModels;

namespace TaskManager.Services.Implementations
{
    public class NotesService : INotesService
    {
        private readonly TasksDbContext db;
        public NotesService(TasksDbContext db)
        {
            this.db = db;
        }

        public async Task<TaskNotesListServiceModel> GetTaskNotesAsync(int taskId)
        {
            var taskWithNotes = await this.db.Tasks.Where(t => t.Id == taskId)
                .ProjectTo<TaskNotesListServiceModel>()
                .FirstOrDefaultAsync();

            if (taskWithNotes == null)
            {
                return null;
            }

            taskWithNotes.Notes = await this.db.Notes
                .Where(n => n.TaskId == taskId && n.isDeleted == false)
                .ProjectTo<NoteServiceModel>()
                .ToListAsync();

            return taskWithNotes;
        }
    }
}
