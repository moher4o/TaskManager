using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models.Email
{
    public class EmailAtachment
    {
        public string Name { get; set; }

        public Byte[] Content { get; set; }

    }
}
