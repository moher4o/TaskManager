using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskMenager.Client.Models.Report
{
    public class PeriodReportViewModel : PeriodViewModel
    {
        [Display(Name = "Дирекция:")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public string DirectoratesId { get; set; }

        public int[] EmployeesIds { get; set; }

        public bool WithDepTabs { get; set; } = true;

        public bool OnlyApprovedHours { get; set; } = false;

        public bool ConfigurationApprovedHours { get; set; } = false;

    }
}
