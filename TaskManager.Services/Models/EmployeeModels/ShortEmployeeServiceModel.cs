using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;
using TaskManager.Services.Models.ReportModels;

namespace TaskManager.Services.Models.EmployeeModels
{
    public class ShortEmployeeServiceModel : IMapFrom<Employee>, IHaveCustomMapping
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public virtual ICollection<EmployeeWorkedHoursByDateServiceModel> WorkedHoursByTaskByPeriod { get; set; } = new List<EmployeeWorkedHoursByDateServiceModel>();

        public void ConfigureMapping(Profile profile)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-1);
            DateTime endDate = DateTime.Now.Date;
            profile.CreateMap<Employee, ShortEmployeeServiceModel>()
                   .ForMember(u => u.WorkedHoursByTaskByPeriod, cfg => cfg.MapFrom(s => s.WorkedHoursByTask
                                            .OrderBy(wh => wh.WorkDate)
                                            .Where(wh => !wh.isDeleted && wh.WorkDate.Date >= startDate.Date && wh.WorkDate.Date <= endDate.Date)
                                            .Select(wh => new EmployeeWorkedHoursByDateServiceModel
                                            {
                                                TaskId = wh.TaskId,
                                                TaskName = wh.Task.TaskName,
                                                HoursSpend = wh.HoursSpend,
                                                Text = wh.Text,
                                                WorkDate = wh.WorkDate,
                                                RegistrationDate = wh.RegistrationDate,
                                                InTimeRecord = wh.InTimeRecord,
                                                ApprovedRecord = wh.Approved,
                                                ApprovedAdminName = wh.Approved ? wh.ApprovedByAdmnin.FullName : string.Empty
                                            })
                                            .ToList()));
        }

    }
}
