using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.MobMessages
{
    public class MessageListModel : IMapFrom<MobMessage>, IHaveCustomMapping
    {
        public int MessageId { get; set; }
        public string MessageText { get; set; }
        public string SenderName { get; set; }

        public string ReceiverName { get; set; }
        public DateTime MessageDate { get; set; }

        public bool Аlignment { get; set; } = false;  //ако получателя е текущия потребител = true

        public void ConfigureMapping(Profile profile)
        {
            int currentEmployeeId = 0;
            profile.CreateMap<MobMessage, MessageListModel>()
               .ForMember(u => u.SenderName, cfg => cfg.MapFrom(r => r.Sender.FullName))
               .ForMember(u => u.ReceiverName, cfg => cfg.MapFrom(r => r.Receiver.FullName))
               .ForMember(u => u.MessageText, cfg => cfg.MapFrom(r => r.Message.Text))
               .ForMember(u => u.Аlignment, cfg => cfg.MapFrom(r => r.ReceiverId == currentEmployeeId))
               .ForMember(u => u.MessageDate, cfg => cfg.MapFrom(r => r.Message.MessageDate));
        }
    }
}
