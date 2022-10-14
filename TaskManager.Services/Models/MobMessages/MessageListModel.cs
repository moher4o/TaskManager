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
        public DateTime MessageDate { get; set; }

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<MobMessage, MessageListModel>()
               .ForMember(u => u.SenderName, cfg => cfg.MapFrom(r => r.Sender.FullName))
               .ForMember(u => u.MessageText, cfg => cfg.MapFrom(r => r.Message.Text))
               .ForMember(u => u.MessageDate, cfg => cfg.MapFrom(r => r.Message.MessageDate));
        }
    }
}
