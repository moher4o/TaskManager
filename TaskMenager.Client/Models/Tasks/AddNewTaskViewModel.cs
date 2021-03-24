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

        [Required(ErrorMessage = "Полето е задължително")]
        [MaxLength(250)]
        [Display(Name = "Име *")]
        public string TaskName { get; set; }

        
        [MaxLength(500)]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Глобална задача(родител)")]
        public IList<SelectListItem> TaskParetns { get; set; } = new List<SelectListItem>();

        public int? ParentTaskId { get; set; }

        public int OwnerId { get; set; }

        [Display(Name = "Дирекция")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public string DirectoratesId { get; set; }

        [Display(Name = "Отдел")]
        public IList<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public string DepartmentsId { get; set; }

        [Display(Name = "Сектор")]
        public IList<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();

        public string SectorsId { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата *")]
        public DateTime Valid_From { get; set; } = DateTime.Now.Date;

        
        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата")]
        public DateTime? Valid_To { get; set; } = DateTime.Now.Date.AddDays(1);

        [Display(Name = "Обхват")]
        public IList<SelectListItem> TaskTypes { get; set; } = new List<SelectListItem>();

        public string TaskTypesId { get; set; }

        [Display(Name = "Приоритет *")]
        public IList<SelectListItem> TaskPrioritys { get; set; } = new List<SelectListItem>();

        public string TaskPriorityId { get; set; }

        //[Display(Name = "Отговорник *")]
        //public IList<SelectListItem> Assigners { get; set; } = new List<SelectListItem>();

        [Display(Name = "Отговорник *")]
        public SelectList AssignersList { get; set; }

        public int AssignerIdInt { get; set; }

        //public string AssignerId { get; set; }

        [Display(Name = "Изпълнител")]
        public IList<SelectListItem> Employees { get; set; } = new List<SelectListItem>();

        [Display(Name = "Subjects")]
        public int[] EmployeesIds { get; set; }

        [Display(Name = "Лимит часове *")]
        public int HoursLimit { get; set; } = 100;



    }
}
