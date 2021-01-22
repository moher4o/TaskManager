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

        public string SectorName { get; set; }

        public string DepartmentName{ get; set; }

        public string DirectorateName { get; set; }

        public DateTime RegCreated { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public int? ParentTaskId { get; set; }

        public int OwnerId { get; set; }

        public int AssignerId { get; set; }

        public string StatusValue { get; set; }

        public string TypeValue { get; set; }

        public string PriorityValue { get; set; }

        public bool isDeleted { get; set; } = false;

    }
}
