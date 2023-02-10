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

        Task<int> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int taskId, int fromUserId);
        Task<int> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);
        Task<List<MessageListModel>> GetNewUserMessages(int userId, int? colleagueId, int lastMessageId);

        Task<List<MessageListModel>> Get50CompanyMessages(int userId, int taskId);

        Task<List<MessageListModel>> GetNewCompMessages(int userId, int taskId, int lastMessageId);


        //Task<bool> SendMessage(string messageText, int toUserId, int fromUserId);

        //Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);

        //bool MessTest(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId);
    }
}
