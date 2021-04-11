using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;
using TaskManager.Services.Models.ReportModels;

namespace TaskManager.Services.Models.EmployeeModels
{
    public class EmployeeServiceModel : IMapFrom<Employee>, IHaveCustomMapping
    {
        public int Id { get; set; }

        [Display(Name = "Име:")]
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Display(Name = "Служебна поща:")]
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        public int? JobTitleId { get; set; }

        public string JobTitleName { get; set; }

        public int? SectorId { get; set; }

        public string SectorName { get; set; }

        public int? DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public int? DirectorateId { get; set; }

        public string DirectorateName { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }


        // [Required(ErrorMessage = "Telephone Number Required")
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public string TelephoneNumber { get; set; }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public string MobileNumber { get; set; }

        [Display(Name = "Логин акаунт:")]
        [Required]
        [StringLength(50)]
        public string DaeuAccaunt { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

        public virtual ICollection<Task> TasksCreator { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksAssigner { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksCloser { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksDeleted { get; set; } = new List<Task>();

        public virtual ICollection<EmployeesTasks> Tasks { get; set; } = new List<EmployeesTasks>();

        //public virtual ICollection<WorkedHours> WorkedHoursByTask { get; set; } = new List<WorkedHours>();

        public virtual ICollection<EmployeeWorkedHoursByDateServiceModel> WorkedHoursByTaskByPeriod { get; set; } = new List<EmployeeWorkedHoursByDateServiceModel>();

        public virtual ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();

        public void ConfigureMapping(Profile profile)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-1);
            DateTime endDate = DateTime.Now.Date;
            profile.CreateMap<Employee, EmployeeServiceModel>()
                   .ForMember(u => u.RoleName, cfg => cfg.MapFrom(s => s.Role.Name))
                   .ForMember(u => u.DirectorateName, cfg => cfg.MapFrom(s => s.Directorate.DirectorateName))
                   .ForMember(u => u.DepartmentName, cfg => cfg.MapFrom(s => s.Department.DepartmentName))
                   .ForMember(u => u.WorkedHoursByTaskByPeriod, cfg => cfg.MapFrom(s => s.WorkedHoursByTask
                                            .OrderBy(wh => wh.WorkDate)               
                                            .Where(wh => !wh.isDeleted && wh.WorkDate.Date >= startDate.Date && wh.WorkDate.Date <= endDate.Date)
                                            .Select(wh => new EmployeeWorkedHoursByDateServiceModel
                                            {
                                                TaskId = wh.TaskId,
                                                TaskName = wh.Task.TaskName,
                                                HoursSpend = wh.HoursSpend,
                                                Text = wh.Text,
                                                WorkDate = wh.WorkDate
                                            })
                                            .ToList() ))
                   .ForMember(u => u.SectorName, cfg => cfg.MapFrom(s => s.Sector.SectorName));
        }
    }
}
