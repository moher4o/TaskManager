using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Data.Models
{
    public class WorkedHours
    {
        public string TaskId { get; set; }

        public Task Task { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateTime WorkDate { get; set; }

        public int HoursSpend { get; set; }
    }
}
