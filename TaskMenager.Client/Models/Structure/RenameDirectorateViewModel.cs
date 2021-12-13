using System;
using System.ComponentModel.DataAnnotations;

namespace TaskMenager.Client.Models.Structure
{
    public class RenameDirectorateViewModel
    {
        [Required]
        public int DirId { get; set; }

        [MaxLength(200)]
        [Required(ErrorMessage = "Полето е задължително")]
        public string DirectorateName { get; set; }

    }
}
