using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.NotesModels
{
    public class TaskNotesListServiceModel : IMapFrom<TaskManager.Data.Models.Task>
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public virtual ICollection<NoteServiceModel> Notes { get; set; } = new List<NoteServiceModel>();

    }
}
