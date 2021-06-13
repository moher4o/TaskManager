using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Data.Models;
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

        public async Task<bool> AddNoteAsync(string text, int taskId, int userId)
        {
            try
            {
                var newNote = new TaskNote()
                {
                    TaskId = taskId,
                    EmployeeId = userId,
                    Text = text,
                    NoteDate = DateTime.Now
                };
               await  this.db.Notes.AddAsync(newNote);
               await this.db.SaveChangesAsync();
               return true;
                
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoteAsync(int noteId)
        {
            try
            {
                var noteToDelete = await this.db.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
                this.db.Notes.Remove(noteToDelete);
                await this.db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetNoteText(int noteId)
        {
            try
            {
                var noteToEdit = await this.db.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
                return noteToEdit.Text;
            }
            catch (Exception)
            {
                return "[Service Error]";
            }
        }

        public async Task<string> SetNoteText(int noteId, string noteText)
        {
            try
            {
                var noteToEdit = await this.db.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
                noteToEdit.Text = noteText;
                await this.db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return $"[Service Error] {ex.Message}";
            }
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

        public async Task<int> GetNoteEmployeeIdAsync(int noteId)
        {
            int noteEmployeeId = await this.db.Notes.Where(n => n.Id == noteId).Select(n => n.EmployeeId).FirstOrDefaultAsync();
            return noteEmployeeId;
        }
    }
}
