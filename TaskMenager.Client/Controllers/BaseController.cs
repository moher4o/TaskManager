﻿using Microsoft.AspNetCore.Authorization;
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
    public enum EmailType
    {
        Note,
        Create,
        Close
    }

    public class BaseController : Controller
    {
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IEmailService email;
        protected readonly IWebHostEnvironment env;
        protected readonly UserServiceModel currentUser;
        private readonly IEmailConfiguration emailConfiguration;
        public BaseController(IHttpContextAccessor httpContextAccessor, IEmployeesService employees, ITasksService tasks, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration)
        {
            this.env = env;
            this.tasks = tasks;
            this.employees = employees;
            this.email = email;
            this._httpContextAccessor = httpContextAccessor;
            currentUser = this.employees.GetUserDataForCooky(_httpContextAccessor?.HttpContext?.User?.Identities.FirstOrDefault().Name.ToLower());
            this.emailConfiguration = _emailConfiguration;
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public async Task<string> NotificationAsync(int taskId, EmailType _emailType)
        {
            try
            {
                var currentTask = await this.tasks.GetTaskDetails(taskId)
                    .ProjectTo<TaskViewModel>()
                    .FirstOrDefaultAsync();
                if (currentTask != null)
                {
                    string host = string.Concat(emailConfiguration.HostName, "/TaskManager");
                    //string host = string.Concat("https://", _httpContextAccessor.HttpContext.Request.Host.Value, "/TaskManager");
                    var userSetingsLink = "/Users/EditUser?userId=" + currentUser.Id.ToString();
                    userSetingsLink = string.Concat("<a href=\"",host, userSetingsLink, "\">", "Отписване", "</a>");
                    string logo = host + "/gif/logo.gif";
                    var taskLink = host + "/Tasks/TaskDetails?taskId=" + taskId.ToString();
                    var emailForm = string.Empty;
                    if (_emailType == EmailType.Note)
                    {
                        
                        var notesLink = host + "/Notes/TaskNotesList?taskId=" + taskId.ToString();
                        emailForm = string.Format(
                                                   NotificationTemplate,
                                                   string.Concat("<a href=\"", notesLink, "\">", currentTask.TaskName, "</a>"),
                                                   host,
                                                   logo,
                                                   userSetingsLink
                                                   );
                    }
                    else if (_emailType == EmailType.Create)
                    {
                        
                        emailForm = string.Format(
                                                   CreateTemplate,
                                                   string.Concat("<a href=\"", taskLink, "\">", currentTask.TaskName, "</a>"),
                                                   host,
                                                   logo,
                                                   userSetingsLink
                                                   );
                    }
                    else if (_emailType == EmailType.Close)
                    {
                        emailForm = string.Format(
                                                   CloseTemplate,
                                                   string.Concat("<a href=\"", taskLink, "\">", currentTask.TaskName, "</a>"),
                                                   host,
                                                   logo,
                                                   userSetingsLink
                                                   );
                    }


                    var emailAddressesToSent = currentTask.Colleagues
                            .Where(e => e.isDeleted == false && !string.IsNullOrWhiteSpace(e.Email) && e.Notify)
                            .Select(e => new EmailAddress
                            {
                                Name = e.TextValue,
                                Address = e.Email
                            }).ToList();

                    var assigner = await this.employees.GetEmployeeByIdAsync(currentTask.AssignerId);
                    if (!string.IsNullOrWhiteSpace(assigner.Email) && assigner.Notify)
                    {
                        var assignerEmail = new EmailAddress()
                        {
                            Name = assigner.FullName,
                            Address = assigner.Email
                        };
                        var index = emailAddressesToSent.FindIndex(i => i.Address == assignerEmail.Address);
                        if (index > -1)
                        {
                            emailAddressesToSent.RemoveAt(index);
                            emailAddressesToSent.Insert(0, assignerEmail);
                        }                                                                                               //адреса на ръководителя се поставя/премества на първо място
                        else
                        {
                            emailAddressesToSent.Insert(0, assignerEmail);    //съобщението се изпраща до този (първия) потребител. Останалите са в BCC
                        }
                    }

                    if (emailAddressesToSent.Count > 0)
                    {
                        var message = new EmailMessage();
                        message.Content = emailForm;
                        message.FromAddresses.Add(new EmailAddress { Name = FirmName, Address = emailConfiguration.FromEmailString });
                        message.ToAddresses.AddRange(emailAddressesToSent);
                        message.Subject = "Информация за активност по задача";
                        await this.email.Send(message);
                    }

                }

                return "success";
            }
            catch (Exception ex)
            {
                return $"[SendNotificationForNote] Грешка при създаването на email {ex.Message}";
            }
        }

        public async Task<string> NotificationAsync(int taskId, List<int> oldColeagues)
        {
            try
            {
                var currentTask = await this.tasks.GetTaskDetails(taskId)
                    .ProjectTo<TaskViewModel>()
                    .FirstOrDefaultAsync();
                if (currentTask != null)
                {
                    string host = string.Concat(emailConfiguration.HostName, "/TaskManager");
                    //string host = string.Concat("https://", _httpContextAccessor.HttpContext.Request.Host.Value);
                    var userSetingsLink = "/Users/EditUser?userId=" + currentUser.Id.ToString();
                    userSetingsLink = string.Concat("<a href=\"", host, userSetingsLink, "\">", "Отписване", "</a>");
                    string logo = host + "/gif/logo.gif";

                    var emailForm = string.Empty;
                        var taskLink = host + "/Tasks/TaskDetails?taskId=" + taskId.ToString();
                        emailForm = string.Format(
                                                   AddColeaguesTemplate,
                                                   string.Concat("<a href=\"", taskLink, "\">", currentTask.TaskName, "</a>"),
                                                   host,
                                                   logo,
                                                   userSetingsLink
                                                   );
                    var emailAddressesToSent = currentTask.Colleagues
                            .Where(e => e.isDeleted == false && !string.IsNullOrWhiteSpace(e.Email) && e.Notify)
                            .Select(e => new EmailAddress
                            {
                                EmpId = e.Id,
                                Name = e.TextValue,
                                Address = e.Email
                            }).ToList();

                    var assigner = await this.employees.GetEmployeeByIdAsync(currentTask.AssignerId);
                    if (!string.IsNullOrWhiteSpace(assigner.Email) && assigner.Notify)
                    {
                        var assignerEmail = new EmailAddress()
                        {
                            EmpId = assigner.Id,
                            Name = assigner.FullName,
                            Address = assigner.Email
                        };
                        var index = emailAddressesToSent.FindIndex(i => i.Address == assignerEmail.Address);
                        if (index > -1)
                        {
                            emailAddressesToSent.RemoveAt(index);
                            emailAddressesToSent.Insert(0, assignerEmail);
                        }                                                                                               //адреса на ръководителя се поставя/премества на първо място
                        else
                        {
                            emailAddressesToSent.Insert(0, assignerEmail);   //съобщението се изпраща до този (първия) потребител. Останалите са в BCC
                        }
                    }

                    //проверка дали има нови колеги
                    emailAddressesToSent = emailAddressesToSent.Except(emailAddressesToSent.Where(s => oldColeagues.Contains(s.EmpId)).ToArray()).ToList();

                    if (emailAddressesToSent.Count > 0)
                    {
                        var message = new EmailMessage();
                        message.Content = emailForm;
                        message.FromAddresses.Add(new EmailAddress { Name = FirmName, Address = emailConfiguration.FromEmailString });
                        message.ToAddresses.AddRange(emailAddressesToSent);
                        message.Subject = "Информация за активност по задача";
                        await this.email.Send(message);
                    }

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
