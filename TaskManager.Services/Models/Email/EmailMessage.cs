using System.Collections.Generic;

namespace TaskManager.Services.Models.Email
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
            Atachments = new List<EmailAtachment>();
        }

        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public ICollection<EmailAtachment> Atachments { get; set; }
    }
}
