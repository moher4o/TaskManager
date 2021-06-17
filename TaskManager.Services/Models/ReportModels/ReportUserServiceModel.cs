using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.ReportModels
{
    public class ReportUserServiceModel : IMapFrom<Employee>
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public int? SectorId { get; set; }

        public int? DepartmentId { get; set; }

        public int? DirectorateId { get; set; }

        public int? TaskWorkedHours { get; set; }

        public string UserNotesForPeriod { get; set; }

    }
}
