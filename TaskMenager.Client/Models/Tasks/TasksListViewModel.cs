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

        public int? SectorId { get; set; }

        public int? DepartmentId { get; set; }

        public int? DirectorateId { get; set; }

        public string Status { get; set; }

        public string TypeName { get; set; }

        public string TypeId { get; set; }

        public string TaskAssigner { get; set; }

        public int AssignerId { get; set; }

        public int AssignedExpertsCount { get; set; } = 0;

        public string ParentTaskId { get; set; }

        public int FilesCount { get; set; }

        public int ChildrenCount { get; set; }

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<Task, TasksListViewModel>()
                  .ForMember(u => u.Status, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                  .ForMember(u => u.TypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName))
                  .ForMember(u => u.TaskAssigner, cfg => cfg.MapFrom(s => s.Assigner.FullName))
                  .ForMember(u => u.DirectorateName, cfg => cfg.MapFrom(s => s.Directorate.DirectorateName))
                  .ForMember(u => u.DepartmentName, cfg => cfg.MapFrom(s => s.Department.DepartmentName))
                  .ForMember(u => u.SectorName, cfg => cfg.MapFrom(s => s.Sector.SectorName))
                  .ForMember(u => u.ParentTaskId, cfg => cfg.MapFrom(s => s.ParentTaskId.HasValue ? s.ParentTaskId.ToString() : "-1"))
                  .ForMember(u => u.ChildrenCount, cfg => cfg.MapFrom(s => s.TaskChildrens.Count()))
                  .ForMember(u => u.AssignedExpertsCount, cfg => cfg.MapFrom(s => s.AssignedExperts.Where(ae => ae.isDeleted == false).Count()));
        }
    }
}
