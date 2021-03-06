﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class TasksStatus
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string StatusName { get; set; }

        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        public bool isDeleted { get; set; } = false;

    }
}
