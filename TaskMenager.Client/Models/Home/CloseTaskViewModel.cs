using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Home
{
    public class CloseTaskViewModel
    {
        [Required]
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        [MaxLength(500)]
        [Required(ErrorMessage = "Полето е задължително")]
        [Display(Name = "Коментар:")]
        public string EndNote { get; set; }
    }
}
