using TaskManager.Services.Models;
using MailKit.Net.Smtp;
using MimeKit;
using static TaskManager.Common.DataConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskManager.Services.Models.Email;
using System.Security.Authentication;
using MailKit.Security;
using System.Threading.Tasks;

namespace TaskManager.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Send(EmailMessage emailMessage)
        {
            if (_emailConfiguration.SendMails)
            {
                var message = new MimeMessage();
                message.To.Add(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)).FirstOrDefault());
                emailMessage.ToAddresses.RemoveAt(0);
                message.Bcc.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

                message.Subject = emailMessage.Subject;
                //We will say we are sending HTML. But there are options for plaintext etc. 
                //message.Body = new TextPart(TextFormat.Html)
                //{
                //    Text = emailMessage.Content
                //};

                message.Body = MessageBody(emailMessage);
                //SaslMechanismNtlmIntegrated sasl = new SaslMechanismNtlmIntegrated();
                //Be careful that the SmtpClient class is the one from Mailkit not the framework!
                using (var emailClient = new SmtpClient())
                {
                    emailClient.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    if (_emailConfiguration.EnableSsl)
                    {
                        await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                        await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                    }
                    else
                    {
                        await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);
                    }

                    ////The last parameter here is to use SSL (Which you should!)
                    ////emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);


                    //await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);     //1 вариант без автентикация

                    ////emailClient.Connect(_emailConfiguration.SmtpServer, 587, SecureSocketOptions.StartTls);   //2 вариант с автентикация
                    ////emailClient.Authenticate(sasl);

                    ////Remove any OAuth functionality as we won't be using it. 
                    ////emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    ////await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                    await emailClient.SendAsync(message);

                    await emailClient.DisconnectAsync(true);
                }
            }
            return true;
        }

        public MimeEntity MessageBody(EmailMessage emailMessage)
        {

            var builder = new BodyBuilder();

            builder.HtmlBody = emailMessage.Content;

            if (emailMessage.Atachments.Count != 0)
            {

                foreach (var atachment in emailMessage.Atachments)
                {
                    builder.Attachments.Add(atachment.Name, atachment.Content);
                }
                // Now we just need to set the message body and we're done

            }

            return builder.ToMessageBody();


        }

        //public void SendConfirmationEmailBody(EmailTransferServiceModel transfer)
        //{
        //    string emailForm = null;
        //    if (transfer.TransferType.IsAirportTransfer)
        //    {
        //        emailForm = string.Format(
        //                                 ConfirmationEmailFormatAirport,
        //                                 transfer.RegistrationDate.ToString("dd/MM/yyyy"),
        //                                 transfer.KlientName,
        //                                 transfer.TransferType.Title,
        //                                 transfer.Subject,
        //                                 transfer.DestinationName,
        //                                 transfer.ArrivalDate.Date.ToString("dd/MM/yyyy"),
        //                                 transfer.ArrivalDate.TimeOfDay,
        //                                 transfer.ArrivalAirportName,
        //                                 transfer.ArrivalAirlineName,
        //                                 transfer.ArrivalFlightNumber,
        //                                 transfer.ArrivalPassengers,
        //                                 transfer.BabyPassengers,
        //                                 transfer.HoldBags,
        //                                 transfer.SkiBags,
        //                                 transfer.SnowboardBags,
        //                                 transfer.Id
        //                                 );

        //    }
        //    else
        //    {
        //        emailForm = string.Format(
        //                                 ConfirmationEmailFormatNotAirport,
        //                                 transfer.RegistrationDate.ToString("dd/MM/yyyy"),
        //                                 transfer.KlientName,
        //                                 transfer.TransferType.Title,
        //                                 transfer.Subject,
        //                                 transfer.DestinationName,
        //                                 transfer.ArrivalDate.Date.ToString("dd/MM/yyyy"),
        //                                 transfer.ArrivalDate.TimeOfDay,
        //                                 transfer.PickupLocation,
        //                                 transfer.ArrivalPassengers,
        //                                 transfer.BabyPassengers,
        //                                 transfer.HoldBags,
        //                                 transfer.SkiBags,
        //                                 transfer.SnowboardBags,
        //                                 transfer.Id
        //                                 );

        //    }

        //    if (!transfer.TransferType.IsOneWay) //if transfer has return part
        //    {
        //        emailForm = emailForm + string.Format(
        //                                 ConfirmationEmailReturn,
        //                                 transfer.DepartureDate.Date.ToString("dd/MM/yyyy"),
        //                                 transfer.DepartureDate.TimeOfDay,
        //                                 transfer.DepartureAirportName,
        //                                 transfer.DepartureAirlineName,
        //                                 transfer.DepartureFlightNumber,
        //                                 transfer.ReturnPassengers
        //                                 );
        //    }

        //    emailForm = emailForm + string.Format(
        //                              ConfirmationEmailAditionalInfo,
        //                              transfer.AdditionalInformation
        //                              );


        //    var message = new EmailMessage();
        //    message.Content = emailForm;
        //    message.FromAddresses.Add(new EmailAddress { Name = "Book&Travel", Address = "bookandtravel@gmail.com" });
        //    message.ToAddresses.Add(new EmailAddress { Name = $"{transfer.KlientName}", Address = $"{transfer.Email}" });
        //    message.Subject = "Transfer confirmation";
        //    this.Send(message);

        //}
    }
}
