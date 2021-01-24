using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TasksType
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TypeName { get; set; }

        public bool isDeleted { get; set; } = false;

        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}
