using System;
using System.ComponentModel.DataAnnotations;

namespace TaskMenager.Client.Models.Tasks
{
    public class AddWorkedHoursViewModel
    {
        public int taskId { get; set; }

        [Display(Name = "Задача")]
        public string TaskName { get; set; }

        public int employeeId { get; set; }

        [Display(Name = "Име на служителя")]
        public string employeeFullName { get; set; }

        [Display(Name = "Часове")]
        public int HoursSpend { get; set; } = 0;

        [MaxLength(250)]
        [Display(Name = "Коментар")]
        public string Text { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;

    }
}
