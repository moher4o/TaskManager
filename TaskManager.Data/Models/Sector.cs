using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Sector
    {
        [Key]
        public string SectorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

        public string DepartmentId { get; set; }

        public Department Department { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public bool isDeleted { get; set; } = false;

    }
}
