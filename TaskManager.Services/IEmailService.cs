using TaskManager.Services.Models.Email;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public interface IEmailService
    {
        Task<bool> Send(EmailMessage emailMessage);

        List<EmailMessage> ReceiveEmail(int maxCount = 10);

        //void SendConfirmationEmailBody(EmailTransferServiceModel transfer);
    }
}
