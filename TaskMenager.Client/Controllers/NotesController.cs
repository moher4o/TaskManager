using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskMenager.Client.Models.Notes;
using TaskMenager.Client.Models.Tasks;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class NotesController : BaseController
    {
        //private readonly IDirectorateService directorates;
        //private readonly IDepartmentsService departments;
        //private readonly ISectorsService sectors;
        //private readonly ITaskTypesService tasktypes;
        //private readonly ITaskPrioritysService taskprioritys;
        //private readonly IStatusService statuses;
        private readonly INotesService taskNotes;
        private readonly I2FAConfiguration twoFAConfiguration;
        private readonly IMessageService mobmessage;
        public NotesController(IEmployeesService employees, IHttpContextAccessor httpContextAccessor, ITasksService tasks, INotesService taskNotes, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration, I2FAConfiguration _twoFAConfiguration, IMessageService _mobmessage) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
        {
            //this.statuses = statuses;
            //this.directorates = directorates;
            //this.departments = departments;
            //this.sectors = sectors;
            //this.tasktypes = tasktypes;
            //this.taskprioritys = taskprioritys;
            this.taskNotes = taskNotes;
            this.mobmessage = _mobmessage;
            twoFAConfiguration = _twoFAConfiguration;
        }

        public async Task<IActionResult> TaskNotesList(int taskId)
        {
            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }

            try
            {
                var taskWithNotes = await this.taskNotes.GetTaskNotesAsync(taskId);
                if (taskWithNotes == null)
                {
                    TempData["Error"] = "[Service] грешка. Неуспешно генериране на модела за коментарите по задачата.";
                    return RedirectToAction("Index", "Home");
                }

                return View(taskWithNotes);
            }
            catch (Exception)
            {
                TempData["Error"] = "[Controler] грешка. Неуспешно генериране на модела за коментарите по задачата.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> EditNote(int noteId)
        {
            var model = new EditNoteViewModel();
            var noteOwnerId = await this.taskNotes.GetNoteEmployeeIdAsync(noteId);
            var permisionToEdit = ((currentUser.RoleName == DataConstants.SuperAdmin) || (currentUser.Id == noteOwnerId)) ? true : false;
            if (permisionToEdit)
            {
                model.NoteId = noteId;
                model.NoteText = await this.taskNotes.GetNoteText(noteId);
                if (model.NoteText == "[Service Error]")
                {
                    TempData["Error"] = "[Service error]";
                }
                return PartialView("_EditNoteModalPartial", model);
            }
            else
            {
                return PartialView("_RenameDirectorateModalPartial", model);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(EditNoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var noteOwnerId = await this.taskNotes.GetNoteEmployeeIdAsync(model.NoteId);
                var permisionToEdit = ((currentUser.RoleName == DataConstants.SuperAdmin) || (currentUser.Id == noteOwnerId)) ? true : false;
                if (permisionToEdit)
                {
                    string result = await this.taskNotes.SetNoteText(model.NoteId, model.NoteText);
                    if (result != "success")
                    {
                        TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                    }
                    return PartialView("_EditNoteModalPartial", model);
                }
                {
                    return PartialView("_RenameDirectorateModalPartial", model);
                }
            }
            else
            {
                return PartialView("_EditNoteModalPartial", model);
            }
            
        }


        #region API Calls
        [Authorize(Policy = DataConstants.Employee)]
        [HttpGet]
        public async Task<IActionResult> AddNote(string text, int taskId)
        {
            var taskFromDb = await this.tasks.CheckTaskByIdAsync(taskId);
            if (!taskFromDb)
            {
                return Json(new { success = false, message = $"Няма задача с N:{taskId}" });
            }
            bool result = await this.taskNotes.AddNoteAsync(text, taskId, currentUser.Id);
            if (result)
            {
                var testresult = await this.NotificationAsync(taskId, EmailType.Note);
                if (testresult != "success")
                {
                    TempData["Error"] = testresult;
                }
                await SendMobMessage(text, taskId);
                return Json(new { success = result, message = "Коментара е добавен успешно" });
            }
            {
                return Json(new { success = false, message = "[Service] Коментара не е добавен" });
            }
        }

        private async Task SendMobMessage(string text, int taskId)
        {
            var currentTask = await this.tasks.GetTaskDetails(taskId)
                .ProjectTo<TaskViewModel>()
                .FirstOrDefaultAsync();
            var usersIdList = currentTask.Colleagues
                        .Where(e => e.isDeleted == false && !string.IsNullOrWhiteSpace(e.Email) && e.Notify)
                        .Select(e => e.Id).ToList();
            if (usersIdList.Count > 0)
            {
                int systemAccountId = await this.employees.GetSystemAccountId();
                if (systemAccountId != 0 && systemAccountId != -99999)
                {
                    await this.mobmessage.SendMessage($"Добавен е коментар по задача с N:{taskId}.", text, usersIdList, systemAccountId);
                }
                else
                {
                    TempData["Error"] = "System account Id not set!!!";
                }
                
            }
            
        }

        [Authorize(Policy = DataConstants.Employee)]
        [HttpGet]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var noteOwnerId = await this.taskNotes.GetNoteEmployeeIdAsync(noteId);
            var permisionToEdit = ((currentUser.RoleName == DataConstants.SuperAdmin) || (currentUser.Id == noteOwnerId)) ? true : false;
            if (permisionToEdit)
            {
                bool result = await this.taskNotes.DeleteNoteAsync(noteId);
                if (result)
                {
                    return Json(new { success = result, message = "Коментара е премахнат" });
                }
                else
                {
                    return Json(new { success = result, message = "[Service Error]" });
                }

            }
            else 
            {
                return Json(new { success = false, message = "Липса на права за изтриване на коментара" });
            }

        }


        #endregion

    }
}
