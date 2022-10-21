using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models.MobMessages;

namespace TaskManager.Services
{
    public interface IMessageService
    {
        Task<List<MessageListModel>> GetLast50UserMessages(int userId, int? senderId);
        Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);
        Task<List<MessageListModel>> GetNewUserMessages(int userId, int lastMessageId);

        //Task<bool> SendMessage(string messageText, int toUserId, int fromUserId);

        //Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);

        //bool MessTest(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);
    }
}
