using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.TaskModels
{
    public class TaskChildrensServiceModel : IMapFrom<Task>, IHaveCustomMapping
    {
        public int Id { get; set; }
        public string TaskName { get; set; }

        public bool isDeleted { get; set; } = false;

        public string TaskTypeName { get; set; }
        public string TaskStatusName { get; set; }

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<Task, TaskChildrensServiceModel>()
                   .ForMember(u => u.TaskStatusName, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                   .ForMember(u => u.TaskTypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName));
       
        }
    }
}
