using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Tasks
{
    public class AddNewTaskViewModel
    {
        [Required]
        [MaxLength(250)]
        [Display(Name = "Име *")]
        public string Name { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Дирекция")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        [Display(Name = "Отдел")]
        public IList<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        [Display(Name = "Сектор")]
        public IList<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата *")]
        public DateTime Valid_From { get; set; } = DateTime.UtcNow.Date;

        
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата(прознозна)")]
        public DateTime? Valid_To { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [Display(Name = "Категория")]
        public IList<SelectListItem> TaskTypes { get; set; } = new List<SelectListItem>();

        [Display(Name = "Приоритет")]
        public IList<SelectListItem> TaskPrioritys { get; set; } = new List<SelectListItem>();

        [Display(Name = "Отговорник *")]
        public IList<SelectListItem> Assigners { get; set; } = new List<SelectListItem>();

        [Display(Name = "Изпълнител")]
        public IList<SelectListItem> Employees { get; set; } = new List<SelectListItem>();


        [Display(Name = "Лимит на работни часове")]
        public int HoursLimit { get; set; } = 100;



    }
}
