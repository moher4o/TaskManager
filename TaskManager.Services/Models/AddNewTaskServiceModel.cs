using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models
{
    public class AddNewTaskServiceModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? SectorId { get; set; }

        public int? DepartmentId{ get; set; }

        public int? DirectorateId { get; set; }

        public DateTime RegCreated { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public int? ParentTaskId { get; set; }

        public int OwnerId { get; set; }

        public int AssignerId { get; set; }

        public int[] EmployeesIds { get; set; }

        public int StatusId { get; set; }

        public int TypeId { get; set; }

        public int PriorityId { get; set; }

        public int HoursLimit { get; set; } = 100;

        public bool isDeleted { get; set; } = false;

    }
}
