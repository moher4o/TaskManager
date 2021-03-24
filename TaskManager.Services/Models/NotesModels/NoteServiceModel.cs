using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.NotesModels
{
    public class NoteServiceModel : IMapFrom<TaskNote>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public DateTime NoteDate { get; set; } = new DateTime();

        [MaxLength(500)]
        public string Text { get; set; }

        public bool isDeleted { get; set; } = false;

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<TaskNote, NoteServiceModel>()
                   .ForMember(u => u.EmployeeName, cfg => cfg.MapFrom(s => s.Employee.FullName))
                   .ForMember(u => u.TaskName, cfg => cfg.MapFrom(s => s.Task.TaskName));
        }
    }
}
