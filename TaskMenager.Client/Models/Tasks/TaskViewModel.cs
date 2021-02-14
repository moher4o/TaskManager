using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskMenager.Client.Models.Tasks
{
    public class TaskViewModel : IMapFrom<TaskInfoServiceModel>, IHaveCustomMapping
    {
        [Display(Name = "Номер на задачата: ")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [MaxLength(250)]
        [Display(Name = "Име: ")]
        public string TaskName { get; set; }

        
        [MaxLength(500)]
        [Display(Name = "Описание: ")]
        public string Description { get; set; }

        [Display(Name = "Финален коментар: ")]
        public string EndNote { get; set; }

        public int? SectorId { get; set; }

        public int? DepartmentId { get; set; }

        public int? DirectorateId { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Създадена на: ")]
        public DateTime RegCreated { get; set; }

     
        [DataType(DataType.Date)]
        [Display(Name = "Начална дата: ")]
        public DateTime StartDate { get; set; }

        
        [DataType(DataType.Date)]
        [Display(Name = "Приключена на: ")]
        public DateTime? EndDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Крайна дата(прогнозна): ")]
        public DateTime? EndDatePrognose { get; set; }

        public int? ParentTaskId { get; set; }

        public int OwnerId { get; set; }

        [Display(Name = "Създадена от: ")]
        public string OwnerName { get; set; }

        public int AssignerId { get; set; }

        [Display(Name = "Отговорник: ")]
        public string AssignerName { get; set; }

        public int[] EmployeesIds { get; set; }

        public int StatusId { get; set; }

        public int TypeId { get; set; }

        public int PriorityId { get; set; }

        [Display(Name = "Лимит часове: ")]
        public int HoursLimit { get; set; } = 100;

        public bool isDeleted { get; set; } = false;

        [Display(Name = "Тип: ")]
        public string TaskTypeName { get; set; }

        [Display(Name = "Статус: ")]
        public string TaskStatusName { get; set; }

        [Display(Name = "Приоритет: ")]
        public string TaskPriorityName { get; set; }

        [Display(Name = "Основна Задача: ")]
        public string ParentTaskName { get; set; }

        [Display(Name = "Дирекция: ")] 
        public string DirectorateName { get; set; }

        [Display(Name = "Отдел: ")]
        public string DepartmentName { get; set; }

        [Display(Name = "Сектор: ")]
        public string SectorName { get; set; }

        [Display(Name = "Участници: ")]
        public IList<SelectListItem> Colleagues { get; set; } = new List<SelectListItem>();

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<TaskInfoServiceModel, TaskViewModel>()
                   .ForMember(u => u.Colleagues, cfg => cfg.MapFrom(s => s.AssignedExperts
                                                           .OrderBy(e => e.Employee.FullName)
                                                           .Select(e => new SelectListItem
                                                           {
                                                               Text = string.Concat(e.Employee.JobTitle.TitleName, " ", e.Employee.FullName),
                                                               Value = e.Employee.Id.ToString()
                                                           })
                                                           .ToList()));
        }
    }
}
