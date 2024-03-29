﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services.Models.EmployeeModels;
using TaskManager.Services.Models.TaskModels;

namespace TaskMenager.Client.Models.Report
{
    public class PeriodViewModel
    {
        public int userId { get; set; }

        public int taskId { get; set; }

        public string TaskName { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата *")]
        public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(-6);

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата *")]
        public DateTime EndDate { get; set; } = DateTime.Now.Date;

        public bool CheckRegistrationDate { get; set; } = false;

        public string ReportDay { get; set; }

        public bool ReportApproval { get; set; } = false;

        public bool UserCanApprove { get; set; } = false;

        public bool OnlyApprovedHours { get; set; } = false;

        public List<TaskWorkedHoursServiceModel> DateList { get; set; } = new List<TaskWorkedHoursServiceModel>();

        public ShortEmployeeServiceModel PersonalDateList { get; set; } = new ShortEmployeeServiceModel();
    }
}
