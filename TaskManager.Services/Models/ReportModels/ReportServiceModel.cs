using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;
using TaskManager.Services.Models.TaskModels;

namespace TaskManager.Services.Models.ReportModels
{
    public class ReportServiceModel : IMapFrom<Task>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public string Description { get; set; }

        public string EndNote { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ParentTaskId { get; set; }

        public string TaskStatusName { get; set; }

        public string TaskTypeName { get; set; }

        public int HoursLimit { get; set; }

        public List<ReportUserServiceModel> Colleagues { get; set; } = new List<ReportUserServiceModel>();

        public void ConfigureMapping(Profile profile)
        {
            int[] employeesIds = new int[1];
            DateTime startDate = DateTime.Now.Date.AddDays(-1);
            DateTime endDate = DateTime.Now.Date;
            bool onlyApprovedHours = false;
            profile.CreateMap<Task, ReportServiceModel>()
                   .ForMember(u => u.TaskStatusName, cfg => cfg.MapFrom(s => s.TaskStatus.StatusName))
                   .ForMember(u => u.TaskTypeName, cfg => cfg.MapFrom(s => s.TaskType.TypeName))
                   .ForMember(u => u.Colleagues, cfg => cfg.MapFrom(s => s.AssignedExperts
                                                           .Where(ex => employeesIds.Contains(ex.EmployeeId))
                                                           .OrderBy(e => e.Employee.FullName)
                                                           .Select(e => new ReportUserServiceModel
                                                           {
                                                               Id = e.EmployeeId,
                                                               FullName = e.Employee.FullName,
                                                               DirectorateId = e.Employee.DirectorateId,
                                                               DepartmentId = e.Employee.DepartmentId,
                                                               SectorId = e.Employee.SectorId,
                                                               InTimeRecord = e.Employee.WorkedHoursByTask
                                                               .Where(t => t.TaskId == s.Id && !t.isDeleted && t.WorkDate.Date >= startDate.Date && t.WorkDate.Date <= endDate.Date).All(ir => ir.InTimeRecord == true),
                                                               TaskWorkedHours = e.Employee.WorkedHoursByTask
                                                               .Where(t => t.TaskId == s.Id && !t.isDeleted && t.WorkDate.Date >= startDate.Date && t.WorkDate.Date <= endDate.Date && (onlyApprovedHours ? t.Approved == true : true)).Sum(wh => wh.HoursSpend),
                                                               UserNotesForPeriod = string.Join(Environment.NewLine, e.Employee.WorkedHoursByTask
                                                               .Where(t => t.TaskId == s.Id && !t.isDeleted && t.WorkDate.Date >= startDate.Date && t.WorkDate.Date <= endDate.Date && !string.IsNullOrEmpty(t.Text)).Select(wh => string.Concat(wh.WorkDate.ToString("dd/MM/yyyy")," - ", wh.Text) ).ToArray())
                                                           })
                                                           .ToList()));
        }

    }
}


//UserNotesForPeriod = string.Join(" | ", e.Employee.WorkedHoursByTask
//.Where(t => t.TaskId == s.Id && !t.isDeleted && t.WorkDate.Date >= startDate.Date && t.WorkDate.Date <= endDate.Date && !string.IsNullOrEmpty(t.Text)).Select(wh => wh.Text).ToArray())
