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
        [Display(Name = "Номер на задачата")]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name = "Име *")]
        public string TaskName { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        public int? ParentTaskId { get; set; }

        public int OwnerId { get; set; }

        [Display(Name = "Дирекция *")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public string DirectoratesId { get; set; }

        [Display(Name = "Отдел")]
        public IList<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public string DepartmentsId { get; set; }

        [Display(Name = "Сектор")]
        public IList<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();

        public string SectorsId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата *")]
        public DateTime Valid_From { get; set; } = DateTime.UtcNow.Date;

        
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата(прознозна)")]
        public DateTime? Valid_To { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [Display(Name = "Категория")]
        public IList<SelectListItem> TaskTypes { get; set; } = new List<SelectListItem>();

        public string TaskTypesId { get; set; }

        [Display(Name = "Приоритет *")]
        public IList<SelectListItem> TaskPrioritys { get; set; } = new List<SelectListItem>();

        public string TaskPriorityId { get; set; }

        [Display(Name = "Отговорник *")]
        public IList<SelectListItem> Assigners { get; set; } = new List<SelectListItem>();

        public string AssignerId { get; set; }

        [Display(Name = "Изпълнител")]
        public IList<SelectListItem> Employees { get; set; } = new List<SelectListItem>();

        [Display(Name = "Subjects")]
        public int[] EmployeesIds { get; set; }

        [Display(Name = "Лимит на работни часове *")]
        public int HoursLimit { get; set; } = 100;



    }
}
