using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskMenager.Client.Models.Report
{
    public class PeriodReportViewModel
    {
        [Display(Name = "Дирекция")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public string DirectoratesId { get; set; }

        public int[] EmployeesIds { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата *")]
        public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(-7);

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата *")]
        public DateTime EndDate { get; set; } = DateTime.Now.Date;

    }
}
