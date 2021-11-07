using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models.ReportModels
{
    public class EmployeeWorkedHoursByDateServiceModel
    {
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public DateTime WorkDate { get; set; }

        public int HoursSpend { get; set; }

        public string Text { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool InTimeRecord { get; set; } = true;

        public bool ApprovedRecord { get; set; } = false;

        public string ApprovedAdminName { get; set; }
    }
}
