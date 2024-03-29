﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class WorkedHours
    {
        public int TaskId { get; set; }

        public Task Task { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateTime WorkDate { get; set; }

        public int HoursSpend { get; set; }

        [MaxLength(500)]
        public string Text { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool InTimeRecord { get; set; } = true;

        public bool Approved { get; set; } = false;

        public int? ApprovedBy { get; set; }

        public Employee ApprovedByAdmnin { get; set; }

        public bool isDeleted { get; set; } = false;
    }
}
