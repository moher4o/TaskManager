using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Structure
{
    public class TitleViewModel
    {
        [Required]
        public int TitleId { get; set; }

        [MaxLength(200)]
        [Required(ErrorMessage = "Полето е задължително")]
        public string TitleName { get; set; }
    }
}
