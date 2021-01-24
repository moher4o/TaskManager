using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Priority
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PriorityName { get; set; }

        public bool isDeleted { get; set; } = false;

        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}
