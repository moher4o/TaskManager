using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TasksType
    {
        [Key]
        public int TasksTypeId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}
