﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Data.Models
{
    public class EmployeesTasks
    {
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public string TaskId { get; set; }

        public Task Task { get; set; }
    }
}
