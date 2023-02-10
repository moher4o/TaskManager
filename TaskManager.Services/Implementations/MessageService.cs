using AutoMapper.QueryableExtensions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Services.Models;
using TaskManager.Services.Models.MobMessages;

namespace TaskManager.Services.Implementations
{
    public class MessageService : IMessageService
    {
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;
        private readonly TasksDbContext db;
        public MessageService(TasksDbContext _db, IEmployeesService _employees, ITasksService _tasks)
        {
            this.employees = _employees;
            this.tasks = _tasks;
            this.db = _db;
        }

        public async Task<List<MessageListModel>> GetLast50UserMessages(int userId, int? colleagueId)
        {
            var messages = new List<MessageListModel>();
            try
            {
                if (colleagueId.HasValue)
                {
                    messages = await this.db.MessagesParticipants
                        .Where(sr => (sr.ReceiverId == userId && sr.SenderId == colleagueId) || (sr.ReceiverId == colleagueId && sr.SenderId == userId))
                        .OrderByDescending(sr => sr.Message.MessageDate)
                        .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                        .Take(80)
                        .ToListAsync();
                }
                else
                {
                    var tasksIds = await this.tasks.GetAllTasks(userId).Select(t => t.Id).ToListAsync();
                    messages = await this.db.MessagesParticipants
                         .Where(sr => sr.ReceiverId == userId || sr.SenderId == userId || tasksIds.Contains(sr.TaskId))
                         .OrderByDescending(sr => sr.Message.MessageDate)
                         .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                         .Take(80)
                         .ToListAsync();
                    messages = messages                                     //Distinct  на съобщенията по Id, за да не се повтарят
                               .GroupBy(x => x.MessageId)
                               .Select(x => x.FirstOrDefault())
                               .TakeLast(80)
                               .ToList();

                }

                return messages;
            }
            catch (Exception)
            {
                return messages;
            }

        }

        public async Task<List<MessageListModel>> Get50CompanyMessages(int userId, int taskId)
        {
            var messages = new List<MessageListModel>();
            try
            {
                messages = await this.db.MessagesParticipants
                     .Where(sr => sr.TaskId == taskId)
                     .OrderByDescending(sr => sr.Message.MessageDate)
                     //.TakeLast(100)
                     .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                     .Take(80)
                     .ToListAsync();

                //messages = messages                                     //Distinct  на съобщенията по Id, за да не се повтарят
                //                   .GroupBy(x => x.MessageId)
                //                   .Select(x => x.FirstOrDefault())
                //                   .TakeLast(100)
                //                   .ToList();

                return messages;
            }
            catch (Exception)
            {
                return messages;
            }
        }
        public async Task<List<MessageListModel>> GetNewCompMessages(int userId, int taskId, int lastMessageId)
        {
            var messages = new List<MessageListModel>();
            try
            {
                messages = await this.db.MessagesParticipants
                     .Where(sr => sr.TaskId == taskId && sr.MessageId > lastMessageId)
                     .OrderByDescending(sr => sr.Message.MessageDate)
                     .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                     .Take(100)
                     .ToListAsync();

                //messages = messages                                     //Distinct  на съобщенията по Id, за да не се повтарят
                //                   .GroupBy(x => x.MessageId)
                //                   .Select(x => x.FirstOrDefault())
                //                   .ToList();

                return messages;
            }
            catch (Exception)
            {
                return messages;
            }
        }

        public async Task<List<MessageListModel>> GetNewUserMessages(int userId, int? colleagueId, int lastMessageId)
        {
            var messages = new List<MessageListModel>();
            try
            {
                if (colleagueId.HasValue)
                {
                    if (lastMessageId < await this.db.MessagesParticipants.Where(mp => (mp.SenderId == userId && mp.ReceiverId == colleagueId) || (mp.SenderId == colleagueId && mp.ReceiverId == userId)).Select(m => m.Id).LastOrDefaultAsync())
                    {
                        messages = await this.db.MessagesParticipants
                             .Where(mp => (mp.SenderId == userId && mp.ReceiverId == colleagueId) || (mp.SenderId == colleagueId && mp.ReceiverId == userId) && mp.MessageId > lastMessageId)
                             .OrderByDescending(sr => sr.Message.MessageDate)
                             .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                             //.Take(50)    // не е удачно
                             .ToListAsync();
                    }
                }
                else
                {
                    var tasksIds = await this.tasks.GetAllTasks(userId).Select(t => t.Id).ToListAsync();
                    messages = await this.db.MessagesParticipants
                             .Where(sr => (sr.ReceiverId == userId || sr.SenderId == userId || tasksIds.Contains(sr.TaskId)) && sr.MessageId > lastMessageId)
                             .OrderByDescending(sr => sr.Message.MessageDate)
                             .ProjectTo<MessageListModel>(new { currentEmployeeId = userId })
                             .Take(80)    // не е удачно
                             .ToListAsync();
                    messages = messages                                     //Distinct  на съобщенията по Id, за да не се повтарят
                       .GroupBy(x => x.MessageId)
                       .Select(x => x.FirstOrDefault())
                       .TakeLast(80)
                       .ToList();

                    //if (lastMessageId < messages.Select(m => m.MessageId).LastOrDefault())
                    //{
                    //}
                }

                return messages;
            }
            catch (Exception)
            {
                return messages;
            }
        }
        public async Task<int> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int taskId, int fromUserId)
        {
            try
            {
                var messageDb = new MobMessageText()
                {
                    Text = messageText,
                    MessageDate = DateTime.Now
                };

                messageDb = await SendFirebaseMessage(messageTitle, messageText, receivers, taskId, fromUserId, messageDb);
                await this.db.Messages.AddAsync(messageDb);
                await this.db.SaveChangesAsync();
                return messageDb.Id;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private async Task<MobMessageText> SendFirebaseMessage(string messageTitle, string messageText, ICollection<int> receivers, int taskId, int fromUserId, MobMessageText messageDb)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("private_key.json")
                });
            }

            if (messageText.Length > 299)
            {
                messageText = messageText.Substring(0, 299) + "...";
            }
            var receiversTokens = new List<string>();
            foreach (var userId in receivers)
            {
                if (userId != fromUserId || taskId <= 0)     //второто условие пропуска тестовите съобщения лично до себе си
                {
                    var registrationToken = await employees.GetMobileToken(userId);
                    if (taskId <= 0)        // добявя се всеки получател, само ако това не е съобщение до задача
                    {
                        if (registrationToken != "error" && !string.IsNullOrWhiteSpace(registrationToken))
                        {
                            receiversTokens.Add(registrationToken);
                            messageDb.SendReceivers.Add(new MobMessage()
                            {
                                ReceiverId = userId,
                                SenderId = fromUserId,
                                TaskId = taskId,
                                isReceived = false
                            });

                        }
                        else
                        {
                            messageDb.SendReceivers.Add(new MobMessage()
                            {
                                ReceiverId = userId,
                                SenderId = fromUserId,
                                TaskId = taskId,
                                isReceived = false
                            });
                        }
                    }
                }
            }
            if (taskId > 0)   //ако е съобщение до задача, се добавя само един ред в MobMessage
            {
                messageDb.SendReceivers.Add(new MobMessage()
                {
                    ReceiverId = receivers.FirstOrDefault(),
                    SenderId = fromUserId,
                    TaskId = taskId,
                    isReceived = true
                });
            }
            if (receiversTokens.Count > 0)
            {
                var message = new MulticastMessage()
                {
                    Tokens = receiversTokens,
                    Notification = new Notification()
                    {
                        Title = messageTitle,
                        Body = messageText
                    },

                    Android = new AndroidConfig()
                    {
                        TimeToLive = TimeSpan.FromHours(4),
                        Notification = new AndroidNotification()
                        {
                            Icon = "logo",
                            Color = "#f45342"

                        },
                        Priority = FirebaseAdmin.Messaging.Priority.Normal,
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

                if (response.SuccessCount > 0 && taskId <= 0)        //дали получателя е уведомен чрез FirebaseMessaging. Това не означава , че не е получил съобщението (api)
                {
                    foreach (var item in messageDb.SendReceivers)
                    {
                        item.isReceived = true;
                    }
                }
            }
            return messageDb;
        }

        public async Task<int> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId)
        {
            return await this.SendMessage(messageTitle, messageText, receivers, -1, fromUserId);
        }

        //public async Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId)
        //{
        //    try
        //    {
        //        var messageDb = new MobMessageText()
        //        {
        //            Text = messageText,
        //            MessageDate = DateTime.Now
        //        };

        //        if (FirebaseApp.DefaultInstance == null)
        //        {
        //            FirebaseApp.Create(new AppOptions()
        //            {
        //                Credential = GoogleCredential.FromFile("private_key.json")
        //            });
        //        }
        //        if (messageText.Length > 299)
        //        {
        //            messageText = messageText.Substring(0, 299) + "...";
        //        }
        //        var receiversTokens = new List<string>();
        //        foreach (var userId in receivers)
        //        {
        //            var registrationToken = await employees.GetMobileToken(userId);
        //            if (registrationToken != "error" && !string.IsNullOrWhiteSpace(registrationToken))
        //            {
        //                receiversTokens.Add(registrationToken);
        //                messageDb.SendReceivers.Add(new MobMessage()
        //                {
        //                    ReceiverId = userId,
        //                    SenderId = fromUserId,
        //                    TaskId = -1
        //                });
        //            }

        //        }
        //        if (receiversTokens.Count > 0)
        //        {
        //            var message = new MulticastMessage()
        //            {
        //                Tokens = receiversTokens,
        //                Notification = new Notification()
        //                {
        //                    Title = messageTitle,
        //                    Body = messageText
        //                },

        //                Android = new AndroidConfig()
        //                {
        //                    TimeToLive = TimeSpan.FromHours(2),
        //                    Notification = new AndroidNotification()
        //                    {
        //                        Icon = "logo",
        //                        Color = "#f45342"

        //                    },
        //                    Priority = FirebaseAdmin.Messaging.Priority.Normal,
        //                }
        //            };

        //            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

        //            if (response.SuccessCount > 0)
        //            {
        //                foreach (var item in messageDb.SendReceivers)
        //                {
        //                    item.isReceived = true;
        //                }
        //                await this.db.Messages.AddAsync(messageDb);
        //                await this.db.SaveChangesAsync();
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        //public async Task<bool> SendMessage(string messageText, int toUserId, int fromUserId)
        //{
        //    try
        //    {
        //    if (FirebaseApp.DefaultInstance == null)
        //    {
        //        FirebaseApp.Create(new AppOptions()
        //        {
        //            Credential = GoogleCredential.FromFile("private_key.json")
        //        });
        //    }
        //    if (messageText.Length > 299)
        //    {
        //        messageText = messageText.Substring(0, 299) + "...";
        //    }
        //    string senderName = await this.employees.GetEmployeeNameByIdAsync(fromUserId);
        //    var registrationToken = await employees.GetMobileToken(toUserId);
        //    if (registrationToken != "error")
        //    {
        //        var message = new Message()
        //        {
        //            Token = registrationToken,
        //            Notification = new Notification()
        //            {
        //                Title = $"{senderName} :",
        //                Body = messageText
        //            },
        //            Android = new AndroidConfig()
        //            {
        //                TimeToLive = TimeSpan.FromHours(2),
        //                Notification = new AndroidNotification()
        //                {
        //                    Icon = "stock_ticker_update",
        //                    Color = "#f45342"
        //                }
        //            }

        //        };

        //        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        //        // Response is a message ID string.
        //        if (!string.IsNullOrWhiteSpace(response))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    return false;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers)
        //{
        //    if (FirebaseApp.DefaultInstance == null)
        //    {
        //        FirebaseApp.Create(new AppOptions()
        //        {
        //            Credential = GoogleCredential.FromFile("private_key.json")
        //        });
        //    }
        //    if (messageText.Length > 299)
        //    {
        //        messageText = messageText.Substring(0, 299) + "...";
        //    }

        //    var receiversTokens = new List<string>();
        //    foreach (var userId in receivers)
        //    {
        //        var registrationToken = await employees.GetMobileToken(userId);
        //        if (registrationToken != "error")
        //        {
        //            receiversTokens.Add(registrationToken);
        //        }

        //    }
        //    // This registration token comes from the client FCM SDKs.
        //    //var registrationToken = await employees.GetMobileToken(toUserId);
        //    if (receiversTokens.Count > 0)
        //    {
        //        var message = new MulticastMessage()
        //        {
        //            Tokens = receiversTokens,
        //            Notification = new Notification()
        //            {
        //                Title = messageTitle,
        //                Body = messageText
        //            },

        //            Android = new AndroidConfig()
        //            {
        //                TimeToLive = TimeSpan.FromHours(2),
        //                Notification = new AndroidNotification()
        //                {
        //                    Icon = "stock_ticker_update",
        //                    Color = "#f45342"
        //                }
        //            }
        //        };

        //        var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

        //        if (response.SuccessCount > 0)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }

        //    return false;
        //}

    }
}
