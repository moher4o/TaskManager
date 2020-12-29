using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public string JobTitleId { get; set; }

        public JobTitle JobTitle { get; set; }

        public string SectorId { get; set; }

        public Sector Sector { get; set; }

        public string DepartmentId { get; set; }

        public Department Department { get; set; }

        public string DirectorateId { get; set; }

        public Directorate Directorate { get; set; }

       // [Required(ErrorMessage = "Telephone Number Required")
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public string TelephoneNumber {get; set;}




}
}
