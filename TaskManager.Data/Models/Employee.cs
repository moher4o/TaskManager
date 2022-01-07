using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        public int? JobTitleId { get; set; }

        public JobTitle JobTitle { get; set; }

        public int? SectorId { get; set; }

        public Sector Sector { get; set; }

        public int? DepartmentId { get; set; }

        public Department Department { get; set; }

        public int? DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }


        // [Required(ErrorMessage = "Telephone Number Required")
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public string TelephoneNumber {get; set;}

        public string MobileNumber { get; set; }

        [Required]
        [StringLength(50)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string DaeuAccaunt { get; set; }

        public bool isDeleted { get; set; } = false;

        public bool isActive { get; set; } = true;

        public bool Notify { get; set; } = true;

        public int? RepresentativeId { get; set; }

        public Employee Representative { get; set; }

        public bool MessageReaded { get; set; } = false;

        public bool TwoFAActiv { get; set; } = false;

        public virtual ICollection<Employee> UsersRepresentative { get; set; } = new List<Employee>();

        public virtual ICollection<Task> TasksCreator { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksAssigner { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksCloser { get; set; } = new List<Task>();

        public virtual ICollection<Task> TasksDeleted { get; set; } = new List<Task>();

        public virtual ICollection<EmployeesTasks> Tasks { get; set; } = new List<EmployeesTasks>();

        public virtual ICollection<WorkedHours> WorkedHoursByTask { get; set; } = new List<WorkedHours>();

        public virtual ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();

        public virtual ICollection<WorkedHours> ApprovedDateReports { get; set; } = new List<WorkedHours>();

    }
}
