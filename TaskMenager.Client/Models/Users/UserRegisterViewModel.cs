using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common.Mapping;
using TaskManager.Services.Models;

namespace TaskMenager.Client.Models.Users
{
    public class UserRegisterViewModel : IMapFrom<UserServiceModel>
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

        public int? RoleId { get; set; }

        [Display(Name = "Роля*:")]
        public IList<SelectListItem> RolesNames { get; set; } = new List<SelectListItem>();

        public int? SectorId { get; set; }

        [Display(Name = "Сектор")]
        public IList<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();

        public int? DepartmentId { get; set; }

        [Display(Name = "Отдел")]
        public IList<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        public int? DirectorateId { get; set; }

        [Display(Name = "Дирекция:")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public int? RepresentativeId { get; set; }

        [Display(Name = "Представител")]
        public SelectList RepresentativeList { get; set; }

        [Display(Name = "Телефонен номер(02 949)*:")]
        [Required(ErrorMessage = "Телефонния номер е задължителен, ако имате 2 тел. номера ги въведете във формат xxxx;xxxx")]
        //[RegularExpression(@"^\(?([0-9]{4})\)?$", ErrorMessage = "Въведения телефонен номер не е валиден.")]
        public string TelephoneNumber { get; set; }

        [Display(Name = "Мобилен номер:")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Въведения мобилен номер не е валиден.")]
        public string MobileNumber { get; set; }

        [Display(Name = "Акаунт:")]
        [StringLength(50)]
        public string DaeuAccaunt { get; set; }

        [Display(Name = "Код за мобилното приложение:")]
        public string SecretKeyHash { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

        [Display(Name = "email нотификации")]
        public bool Notify { get; set; }

        [Display(Name = "Двуфакторна автентикация")]
        public bool TwoFAActive { get; set; }
    }
}
