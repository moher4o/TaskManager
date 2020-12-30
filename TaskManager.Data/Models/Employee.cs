using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TaskManager.Data.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

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

        public string MobileNumber { get; set; }

        [Required]
        [StringLength(50)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string DaeuAccaunt { get; set; }




    }
}
