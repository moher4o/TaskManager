using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.Notes
{
    public class EditNoteViewModel
    {
        [Required]
        public int NoteId { get; set; }

        [MaxLength(200)]
        [Required(ErrorMessage = "Полето е задължително")]
        public string NoteText { get; set; }
    }
}
