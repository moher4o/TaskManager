using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class NotesController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly ITaskTypesService tasktypes;
        private readonly ITaskPrioritysService taskprioritys;
        private readonly IStatusService statuses;
        private readonly INotesService taskNotes;
        public NotesController(IDirectorateService directorates, IEmployeesService employees, IDepartmentsService departments, ISectorsService sectors, ITaskTypesService tasktypes, ITaskPrioritysService taskprioritys, IHttpContextAccessor httpContextAccessor, IStatusService statuses, ITasksService tasks, INotesService taskNotes, IEmailService email, IWebHostEnvironment env) : base(httpContextAccessor, employees, tasks, email, env)
        {
            this.statuses = statuses;
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.tasktypes = tasktypes;
            this.taskprioritys = taskprioritys;
            this.taskNotes = taskNotes;
        }

        public async Task<IActionResult> TaskNotesList(int taskId)
        {
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
                await this.NotificationAsync(taskId, EmailType.Note);
                return Json(new { success = result, message = "Коментара е добавен успешно" });
            }
            {
                return Json(new { success = false, message = "[Service] Коментара не е добавен" });
            }
            
        }

        #endregion

    }
}
