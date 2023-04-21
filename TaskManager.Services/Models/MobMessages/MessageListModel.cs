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
        public string Text { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int TaskId { get; set; }
        public string User { get; set; }
        public string ReceiverName { get; set; }
        public DateTime MessageDate { get; set; }

        public bool Аlignment { get; set; } = false;  //ако изпращача е текущия потребител = true

        public void ConfigureMapping(Profile profile)
        {
            int currentEmployeeId = 0;
            profile.CreateMap<MobMessage, MessageListModel>()
               .ForMember(u => u.User, cfg => cfg.MapFrom(r => r.Sender.FullName))
               .ForMember(u => u.ReceiverName, cfg => cfg.MapFrom(r => r.TaskId <= 0 ? r.Receiver.FullName : r.TaskId.ToString()))
               .ForMember(u => u.Text, cfg => cfg.MapFrom(r => r.Message.Text))
               .ForMember(u => u.Аlignment, cfg => cfg.MapFrom(r => r.SenderId == currentEmployeeId))
               .ForMember(u => u.MessageDate, cfg => cfg.MapFrom(r => r.Message.MessageDate));
        }
    }
}
