namespace TaskManager.Services
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }

        string PopServer { get; }
        int PopPort { get; }
        string PopUsername { get; }
        string PopPassword { get; }
        bool SendMails { get; set; }

        bool StoreFiles { get; set; }

        long TaskDirectorySize { get; }
    }
}
