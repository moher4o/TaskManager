using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskMenager.Client.Models.Tasks
{
    public class TasksListViewModel : IMapFrom<TaskManager.Data.Models.Task>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public string DirectorateName { get; set; }
        public string DepartmentName { get; set; }
        public string SectorName { get; set; }

        public string Status { get; set; }

        public string TypeName { get; set; }

        public string TaskAssigner { get; set; }

        public int AssignedExpertsCount { get; set; } = 0;

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<Task, TasksListViewModel>()
                  .ForMember(u => u.Status, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                  .ForMember(u => u.TypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName))
                  .ForMember(u => u.TaskAssigner, cfg => cfg.MapFrom(s => s.Assigner.FullName))
                  .ForMember(u => u.DirectorateName, cfg => cfg.MapFrom(s => s.Directorate.DirectorateName))
                  .ForMember(u => u.DepartmentName, cfg => cfg.MapFrom(s => s.Department.DepartmentName))
                  .ForMember(u => u.SectorName, cfg => cfg.MapFrom(s => s.Sector.SectorName))
                  .ForMember(u => u.AssignedExpertsCount, cfg => cfg.MapFrom(s => s.AssignedExperts.Count()));
        }
    }
}
