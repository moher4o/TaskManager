using AutoMapper;
using System;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models.MobMessages
{
    public class MessageServiceModel : IMapFrom<MobMessage>, IHaveCustomMapping
    {
        public int Id { get; set; }
        public int SenderId { get; set; }

        public string SenderName { get; set; }

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public int MessageId { get; set; }

        public string MessageText { get; set; }

        public DateTime MessageDate { get; set; }

        public bool isReceived { get; set; }

        public void ConfigureMapping(Profile profile)
        {
            
            profile.CreateMap<MobMessage, MessageServiceModel>()
               .ForMember(u => u.SenderName, cfg => cfg.MapFrom(r => r.Sender.FullName))
               .ForMember(u => u.ReceiverName, cfg => cfg.MapFrom(r => r.Receiver.FullName))
               .ForMember(u => u.MessageText, cfg => cfg.MapFrom(r => r.Message.Text))
               .ForMember(u => u.MessageDate, cfg => cfg.MapFrom(r => r.Message.MessageDate));
        }
    }
}
