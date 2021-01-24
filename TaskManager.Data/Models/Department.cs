using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Department
    {
        
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string DepartmentName { get; set; }

        public int DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

        public virtual ICollection<Sector> Sectors { get; set; } = new List<Sector>();

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

        public bool isDeleted { get; set; } = false;

    }
}
