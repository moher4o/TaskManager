using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TaskManager.Common.DataConstants;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskManager.Services.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using AutoMapper.QueryableExtensions;
using TaskMenager.Client.Models.Tasks;

namespace TaskMenager.Client.Controllers
{

    public class BaseController : Controller
    {
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IEmailService email;
        protected readonly IWebHostEnvironment env;
        protected readonly UserServiceModel currentUser;
        public BaseController(IHttpContextAccessor httpContextAccessor, IEmployeesService employees, ITasksService tasks, IEmailService email, IWebHostEnvironment env)
        {
            this.env = env;
            this.tasks = tasks;
            this.employees = employees;
            this.email = email;
            this._httpContextAccessor = httpContextAccessor;
            currentUser = this.employees.GetUserDataForCooky(_httpContextAccessor?.HttpContext?.User?.Identities.FirstOrDefault().Name.ToLower());
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public async Task<string> NotificationForNoteAsync(int taskId)
        {
            try
            {
                var currentTask = await this.tasks.GetTaskDetails(taskId)
                    .ProjectTo<TaskViewModel>()
                    .FirstOrDefaultAsync();
                if (currentTask != null)
                {
                    //string host = "https://" + _httpContextAccessor.HttpContext.Request.Host.Value + "/TaskManager";
                    string host = string.Concat("https://", _httpContextAccessor.HttpContext.Request.Host.Value, "/TaskManager");
                    var notesLink = host + "/Notes/TaskNotesList?taskId=" + taskId.ToString();
                    string emailForm = string.Format(
                                               NotificationTemplate,
                                               //string.Concat("г-н/г-жо/г-це ", currentTask.AssignerName.Substring(currentTask.AssignerName.LastIndexOf(' ')+1)),
                                               string.Concat("<a href=\"", notesLink, "\">", currentTask.TaskName, "</a>"),
                                               host
                                               );

                    var assigner = await this.employees.GetEmployeeByIdAsync(currentTask.AssignerId);
                    if (string.IsNullOrWhiteSpace(assigner.Email))
                    {
                        return $"{assigner.FullName} няма въведен email адрес";
                    }

                    var emailAddressesToSent = currentTask.Colleagues
                        .Where(e => e.isDeleted == false && !string.IsNullOrWhiteSpace(e.Email))
                        .Select(e => new EmailAddress
                        {
                            Name = e.TextValue,
                            Address = e.Email
                        }).ToList();


                    var assignerEmail = new EmailAddress()
                    {
                        Name = assigner.FullName,
                        Address = assigner.Email
                    };
                    emailAddressesToSent.Add(assignerEmail);

                    var message = new EmailMessage();
                    message.Content = emailForm;
                    message.FromAddresses.Add(new EmailAddress { Name = FirmName, Address = FromEmailString });
                    message.ToAddresses.AddRange(emailAddressesToSent);
                    message.Subject = "Информация за активност по задача";

                    this.email.Send(message);

                }

                return "success";
            }
            catch (Exception ex)
            {
                return $"[SendNotificationForNote] Грешка при създаването на email {ex.Message}";
            }
        }


    }
}
