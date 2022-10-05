using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class MobMessageText
    {
        public int Id { get; set; }

        [Required]
        public DateTime MessageDate { get; set; }

        [Required]
        [MaxLength(350)]
        public string Text { get; set; }

        public virtual ICollection<MobMessage> SendReceivers { get; set; } = new List<MobMessage>();
    }
}
