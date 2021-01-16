﻿using System;
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
        public string Name { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

    }
}
