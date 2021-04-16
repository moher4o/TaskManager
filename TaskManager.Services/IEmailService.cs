using TaskManager.Services.Models.Email;
using System.Collections.Generic;

namespace TaskManager.Services
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);

        List<EmailMessage> ReceiveEmail(int maxCount = 10);

        //void SendConfirmationEmailBody(EmailTransferServiceModel transfer);
    }
}
