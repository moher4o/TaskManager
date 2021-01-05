using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Task
    {
        [Key]
        public string TaskId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string SectorId { get; set; }

        public Sector Sector { get; set; }

        public string DepartmentId { get; set; }

        public Department Department { get; set; }

        public string DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

        public DateTime RegCreated { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? EndDatePrognose { get; set; }

        public string ParentTaskId { get; set; }

        public Task ParentTask { get; set; }

        public int OwnerId { get; set; }
        public Employee Owner { get; set; }

        public int AssignerId { get; set; }
        public Employee Assigner { get; set; }

        public int StatusId { get; set; }
        public TasksStatus TaskStatus { get; set; }

        public int TypeId { get; set; }
        public TasksType TaskType { get; set; }

        public int PriorityId { get; set; }
        public Priority TaskPriority { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<EmployeesTasks> AssignedExperts { get; set; } = new List<EmployeesTasks>();

        public ICollection<WorkedHours> WorkedHours { get; set; } = new List<WorkedHours>();

        public ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();


    }
}
