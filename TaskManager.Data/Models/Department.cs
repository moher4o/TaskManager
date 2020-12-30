using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Department
    {
        [Key]
        public string DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

        public ICollection<Sector> Sectors { get; set; } = new List<Sector>();

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public ICollection<Task> Tasks { get; set; } = new List<Task>();

        public bool isDeleted { get; set; } = false;

    }
}
