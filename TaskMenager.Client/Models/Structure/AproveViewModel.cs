using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Structure
{
    public class AproveViewModel
    {
        [Required]
        public int UnitId { get; set; }

        [MaxLength(200)]
        public string UnitName { get; set; }

        [Required(ErrorMessage = "Полето е задължително")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата, до която да се одобрят отчетите *")]
        public DateTime AproveDate { get; set; } = DateTime.Now.Date.AddDays(-1);
    }
}
