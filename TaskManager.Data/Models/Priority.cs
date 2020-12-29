﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Priority
    {
        [Key]
        public int PriorityId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

    }
}
