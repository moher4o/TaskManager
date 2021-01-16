using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
