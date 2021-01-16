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
        public string Name { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}
