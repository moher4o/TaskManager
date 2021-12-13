using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Structure
{
    public class CreateDepViewModel
    {
        public int DepId { get; set; }

        [MaxLength(200)]
        [Required(ErrorMessage = "Полето е задължително")]
        public string DepartmentName { get; set; }

        [Display(Name = "Дирекция")]
        public IList<SelectListItem> Directorates { get; set; } = new List<SelectListItem>();

        public int DirectoratesId { get; set; }

    }
}
