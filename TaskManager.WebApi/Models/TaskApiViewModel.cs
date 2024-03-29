﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common.Mapping;
using TaskManager.Services.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.WebApi.Models
{
    public class TaskApiViewModel : IMapFrom<TaskInfoServiceModel>, IHaveCustomMapping
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
        [Display(Name = "Крайна дата: ")]
        public DateTime? EndDatePrognose { get; set; }

        public int? ParentTaskId { get; set; }

        [Display(Name = "Подзадача на: ")]
        public string ParentTaskName { get; set; }

        public int OwnerId { get; set; }

        [Display(Name = "Създадена от: ")]
        public string OwnerName { get; set; }

        public int AssignerId { get; set; }

        [Display(Name = "Отговорник: ")]
        public string AssignerName { get; set; }

        [Display(Name = "Затворена от: ")]
        public string CloserName { get; set; }

        [Display(Name = "Изтрита от: ")]
        public string DeleterName { get; set; }


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

        [Display(Name = "Дирекция: ")]
        public string DirectorateName { get; set; }

        [Display(Name = "Отдел: ")]
        public string DepartmentName { get; set; }

        [Display(Name = "Сектор: ")]
        public string SectorName { get; set; }

        [Display(Name = "Участници: ")]
        public IList<SelectServiceModel> Colleagues { get; set; } = new List<SelectServiceModel>();

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<TaskInfoServiceModel, TaskApiViewModel>()
                   .ForMember(u => u.Colleagues, cfg => cfg.MapFrom(s => s.AssignedExperts
                                                           .OrderBy(e => e.Employee.FullName)
                                                           .Select(e => new SelectServiceModel
                                                           {
                                                               Notify = e.Employee.Notify,
                                                               Email = e.Employee.Email,
                                                               TextValue = e.Employee.FullName,
                                                               Id = e.Employee.Id,
                                                               isDeleted = e.isDeleted,
                                                               DepartmentName = e.Employee.Department.DepartmentName,
                                                               DirectorateName = e.Employee.Directorate.DirectorateName,
                                                               SectorName = e.Employee.Sector.SectorName,
                                                               TokenHash = e.Employee.TokenHash
                                                           })
                                                           .ToList()));
        }
    }
}
