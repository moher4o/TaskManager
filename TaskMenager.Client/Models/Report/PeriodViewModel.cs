using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Report
{
    public class PeriodViewModel
    {
        public int userId { get; set; }

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
