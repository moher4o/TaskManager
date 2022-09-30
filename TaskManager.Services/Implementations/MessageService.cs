﻿using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Services.Implementations
{
    public class MessageService : IMessageService
    {
        protected readonly IEmployeesService employees;
        public MessageService(IEmployeesService _employees)
        {
            this.employees = _employees;
        }

        //public bool MessTest(string messageTitle, string messageText, ICollection<int> receivers, int fromUserId)
        //{
        //    return true;
        //}

        public async Task<bool> SendMessage(string messageTitle, string messageText, ICollection<int> receivers)
        {
            try
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
                    var registrationToken = await employees.GetMobileToken(userId);
                    if (registrationToken != "error" && !string.IsNullOrWhiteSpace(registrationToken))
                    {
                        receiversTokens.Add(registrationToken);
                    }

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
                            TimeToLive = TimeSpan.FromHours(2),
                            Notification = new AndroidNotification()
                            {
                                Icon = "logo",
                                Color = "#f45342"
                                
                            },
                            Priority = Priority.Normal,
                        }
                    };

                    var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

                    if (response.SuccessCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> SendMessage(string messageText, int toUserId, int fromUserId)
        {
            try
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
            string senderName = await this.employees.GetEmployeeNameByIdAsync(fromUserId);
            var registrationToken = await employees.GetMobileToken(toUserId);
            if (registrationToken != "error")
            {
                var message = new Message()
                {
                    Token = registrationToken,
                    Notification = new Notification()
                    {
                        Title = $"{senderName} :",
                        Body = messageText
                    },
                    Android = new AndroidConfig()
                    {
                        TimeToLive = TimeSpan.FromHours(2),
                        Notification = new AndroidNotification()
                        {
                            Icon = "stock_ticker_update",
                            Color = "#f45342"
                        }
                    }

                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                if (!string.IsNullOrWhiteSpace(response))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

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
