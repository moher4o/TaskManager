using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Users
{
    public class UserRegisterViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Име*:")]
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Display(Name = "Служебна поща*:")]
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; }

        public int? JobTitleId { get; set; }

        [Display(Name = "Длъжност*:")]
        public IList<SelectListItem> JobTitles { get; set; } = new List<SelectListItem>();

        public int? SectorId { get; set; }

        [Display(Name = "Сектор")]
        public IList<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();

        public int? DepartmentId { get; set; }

        [Display(Name = "Отдел")]
        public IList<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public int? DirectorateId { get; set; }

        [Display(Name = "Дирекция:")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        [Display(Name = "Телефонен номер*:")]
        [Required(ErrorMessage = "Телефонния номер е задължителен")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Въведения телефонен номер не е валиден.")]
        public string TelephoneNumber { get; set; }

        [Display(Name = "Мобилен номер:")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Въведения мобилен номер не е валиден.")]
        public string MobileNumber { get; set; }

        [Display(Name = "Логин акаунт:")]
        [Required]
        [StringLength(50)]
        public string DaeuAccaunt { get; set; }

        public bool isActive { get; set; }
    }
}
