using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class JobTitle
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TitleName { get; set; }

        public bool isDeleted { get; set; } = false;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
