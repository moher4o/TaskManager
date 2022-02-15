using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Users
{
    public class TwoFAViewModel
    {
        [Display(Name = "Акаунт(email):")]
        public string Account { get; set; }

        [Display(Name = "Автентикационен код за ръчно въвеждане(за у-ва без QR Скенер):")]
        public string ManualEntryKey { get; set; }

        [Display(Name = "QR код: ")]
        public string QrCodeSetupImageUrl { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage = "Кода е задължителен")]
        [Display(Name = "Код от мобилното у-во:")]
        public string UserInputCode { get; set; }

        public string TwoFAExplainLink { get; set; }

        public bool TwoFAActiv { get; set; } = false;

    }
}
