using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TasksStatus
    {
        [Key]
        public int TasksStatusId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

    }
}
