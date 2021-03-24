using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TaskNote
    {

        public int Id { get; set; }

        public int TaskId { get; set; }

        public Task Task { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateTime NoteDate { get; set; }

        [MaxLength(500)]
        public string Text { get; set; }

        public bool isDeleted { get; set; } = false;

    }
}
