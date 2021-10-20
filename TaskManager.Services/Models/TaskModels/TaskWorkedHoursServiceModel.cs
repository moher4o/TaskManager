using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.TaskModels
{
    public class TaskWorkedHoursServiceModel : IMapFrom<WorkedHours>, IHaveCustomMapping
    {
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public DateTime WorkDate { get; set; }

        public int HoursSpend { get; set; }

        [MaxLength(500)]
        public string Text { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool InTimeRecord { get; set; } = true;

        public bool isDeleted { get; set; } = false;

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<WorkedHours, TaskWorkedHoursServiceModel>()
                              .ForMember(u => u.TaskName, cfg => cfg.MapFrom(s => s.Task.TaskName))
                              .ForMember(u => u.EmployeeName, cfg => cfg.MapFrom(s => s.Employee.FullName));
        }
    }
}
