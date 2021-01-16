using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models
{
    public class AddNewEmployeeServiceModel
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string JobTitle { get; set; }

        public string Sector { get; set; }

        public string Department { get; set; }

        public string Directorate { get; set; }

        public string Role { get; set; }

        // [Required(ErrorMessage = "Telephone Number Required")
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public string TelephoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public string DaeuAccaunt { get; set; }

        public bool isDeleted { get; set; } = false;
    }
}
