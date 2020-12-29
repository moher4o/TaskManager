using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Directorate
    {
        [Key]
        public string DirectorateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public ICollection<Department> Departments { get; set; } = new List<Department>();

        public ICollection<Sector> Sectors { get; set; } = new List<Sector>();

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public bool isDeleted { get; set; } = false;

    }
}
