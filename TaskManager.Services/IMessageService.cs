using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);

        //Task<bool> SendMessage(string messageText, int toUserId, int fromUserId);

        //Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);

        //bool MessTest(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);
    }
}
