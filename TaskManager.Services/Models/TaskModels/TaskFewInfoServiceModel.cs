using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.TaskModels
{
    public class TaskFewInfoServiceModel : IMapFrom<Task>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ParentTaskId { get; set; }

        public string TaskStatusName { get; set; }

        public string TaskPriorityName { get; set; }

        public string TaskTypeName { get; set; }

        public int HoursLimit { get; set; }

        public int NotesCount { get; set; }

        public int EmployeeHours { get; set; }

        public int EmployeeHoursToday { get; set; }

        public int FilesCount { get; set; }

        public string TaskNoteForToday { get; set; }

        IEnumerable<SelectServiceModel> Colleagues { get; set; } = new List<SelectServiceModel>();

        

        public void ConfigureMapping(Profile profile)
        {
            int currentEmployeeId = 0;
            DateTime workDate = DateTime.Now.Date;
            profile.CreateMap<Task, TaskFewInfoServiceModel>()
                   .ForMember(u => u.TaskStatusName, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                   .ForMember(u => u.TaskTypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName))
                   .ForMember(u => u.TaskPriorityName, cfg => cfg.MapFrom(s => s.TaskPriority.PriorityName))
                   .ForMember(u => u.Colleagues, cfg => cfg.MapFrom(s => s.AssignedExperts
                                                           .OrderBy(e => e.Employee.FullName)
                                                           .Select(e => new  SelectServiceModel {
                                                                   TextValue = e.Employee.FullName,
                                                                   Id = e.Employee.Id
                                                                })
                                                           .ToList()))
                   .ForMember(u => u.NotesCount, cfg => cfg.MapFrom(s => s.Notes
                                                                                .Where(n => n.isDeleted == false)
                                                                                .Count()))
                   .ForMember(u => u.EmployeeHoursToday, cfg => cfg.MapFrom(s => s.WorkedHours
                                                                                .Where(d => d.WorkDate.Date == workDate.Date && d.EmployeeId == currentEmployeeId)
                                                                                .Sum(hr => hr.HoursSpend)))
                   .ForMember(u => u.EmployeeHours, cfg => cfg.MapFrom(s => s.WorkedHours
                                                                                .Where(hr => hr.EmployeeId == currentEmployeeId)
                                                                                .Sum(hr => hr.HoursSpend)))
                   .ForMember(u => u.TaskNoteForToday, cfg => cfg.MapFrom(s => s.WorkedHours
                                                                                .Where(d => d.WorkDate.Date == workDate.Date && d.EmployeeId == currentEmployeeId)
                                                                                .Select(hr => hr.Text)
                                                                                .FirstOrDefault()));
        }
    }
}
