using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessage(int userId, string messagetitle, string messageToUser);
    }
}
