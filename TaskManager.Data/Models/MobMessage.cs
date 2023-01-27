using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TaskManager.Data.Models
{
    public class MobMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public Employee Sender { get; set; }
        public int ReceiverId { get; set; }
        public Employee Receiver { get; set; }

        public int MessageId { get; set; }
        public MobMessageText Message { get; set; }

        public int TaskId { get; set; }
        public bool isReceived { get; set; } = false;
    }
}
