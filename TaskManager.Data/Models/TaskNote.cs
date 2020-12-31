using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TaskNote
    {
        public string TaskId { get; set; }

        public Task Task { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateTime NoteDate { get; set; }

        public string Text { get; set; }

    }
}
