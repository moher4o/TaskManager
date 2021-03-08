using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.TaskModels
{
    public class TaskInfoServiceModel : TaskServiceModel, IHaveCustomMapping
    {
        public string TaskStatusName { get; set; }

        public string TaskTypeName { get; set; }

        public string TaskPriorityName { get; set; }

        public string ParentTaskName { get; set; }

        public string DirectorateName { get; set; }

        public string DepartmentName { get; set; }

        public string SectorName { get; set; }

        public string OwnerName { get; set; }

        public string AssignerName { get; set; }

        public string CloserName { get; set; }

        public string DeleterName { get; set; }


        public ICollection<EmployeesTasks> AssignedExperts { get; set; } = new List<EmployeesTasks>();
        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<Task, TaskInfoServiceModel>()
                   .ForMember(u => u.AssignerName, cfg => cfg.MapFrom(s => string.Concat(s.Assigner.JobTitle.TitleName, " ", s.Assigner.FullName)))
                   .ForMember(u => u.OwnerName, cfg => cfg.MapFrom(s => string.Concat(s.Owner.JobTitle.TitleName, " ", s.Owner.FullName)))
                   .ForMember(u => u.CloserName, cfg => cfg.MapFrom(s => string.Concat(s.CloseUser.JobTitle.TitleName, " ", s.CloseUser.FullName)))
                   .ForMember(u => u.DeleterName, cfg => cfg.MapFrom(s => string.Concat(s.DeletedByUser.JobTitle.TitleName, " ", s.DeletedByUser.FullName)))
                   .ForMember(u => u.DirectorateName, cfg => cfg.MapFrom(s => s.Directorate.DirectorateName))
                   .ForMember(u => u.DepartmentName, cfg => cfg.MapFrom(s => s.Department.DepartmentName))
                   .ForMember(u => u.SectorName, cfg => cfg.MapFrom(s => s.Sector.SectorName))
                   .ForMember(u => u.ParentTaskName, cfg => cfg.MapFrom(s => s.ParentTask.TaskName))
                   .ForMember(u => u.TaskStatusName, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                   .ForMember(u => u.TaskTypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName))
                   .ForMember(u => u.TaskPriorityName, cfg => cfg.MapFrom(s => s.TaskPriority.PriorityName));

        }
    }
}
