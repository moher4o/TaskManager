using FirebaseAdmin;
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
            //FirebaseApp.Create(new AppOptions()
            //{
            //    Credential = GoogleCredential.FromFile("private_key.json")
            //});
        }

        public async Task<bool> SendMessage(int userId, string messagetitle, string messageToUser)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("private_key.json")
                });
            }
            // This registration token comes from the client FCM SDKs.
            var registrationToken = await employees.GetMobileToken(userId);
            if (registrationToken != "error")
            {


                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Data = new Dictionary<string, string>()
                {
                    { "myData", "1337" },
                },
                    Token = registrationToken,
                    //Topic = "all",
                    Notification = new Notification()
                    {
                        Title = messagetitle,
                        Body = messageToUser
                    }
                };

                // Send a message to the device corresponding to the provided
                // registration token.

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

    }
}
