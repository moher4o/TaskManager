namespace TaskManager.Services.Implementations 
{ 
    public class EmailConfiguration : IEmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }

        public bool EnableSsl { get; set; } = false;

        public string PopServer { get; set; }
        public int PopPort { get; set; }
        public string PopUsername { get; set; }
        public string PopPassword { get; set; }

        public bool SendMails { get; set; } = false;

        public string FromEmailString { get; set; }
        public string HostName { get; set; }
    }
}
