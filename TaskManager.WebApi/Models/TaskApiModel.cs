using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class TaskApiModel
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ParentTaskId { get; set; }

        public string TaskStatusName { get; set; }

        public string TaskPriorityName { get; set; }

        public string TaskTypeName { get; set; }

        public int HoursLimit { get; set; }

        public int NotesCount { get; set; }

        public int EmployeeHours { get; set; }

        public int EmployeeHoursToday { get; set; }

        public int FilesCount { get; set; }

        public string TaskNoteForToday { get; set; }

        public int ChildrenCount { get; set; }

        public bool ApprovedToday { get; set; } = false;

        public string ApprovedByAdmninName { get; set; }
    }
}
