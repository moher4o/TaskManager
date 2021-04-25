using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskMenager.Client.Models.Tasks;
using TaskMenager.Client.Models.TasksFiles;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.Employee)]
    public class TasksFilesController : BaseController
    {
        private readonly IManageFilesService files;

        public TasksFilesController(IManageFilesService files, IHttpContextAccessor httpContextAccessor, ITasksService tasks, IEmployeesService employees, IEmailService email, IWebHostEnvironment env) : base(httpContextAccessor, employees, tasks, email, env)
        {
            this.files = files;
        }

        public async Task<IActionResult> TaskFilesList(int taskId, string taskName)
        {
            try
            {
                var currentTask = this.tasks.GetTaskDetails(taskId)
                        .ProjectTo<TaskViewModel>()
                        .FirstOrDefault();
                var assignedEmployees = new List<SelectServiceModel>();
                assignedEmployees.AddRange(currentTask.Colleagues.ToList());
                currentTask.EmployeesIds = assignedEmployees.Where(e => e.isDeleted == false).Select(a => a.Id).ToArray(); //за да изключи премахнатите експерти


                var status = await this.tasks.CheckIfTaskIsClosed(taskId);
                var fileList = new TaskFilesViewModel()
                {
                    Id = taskId,
                    TaskName = taskName != null ? taskName : this.tasks.GetTaskDetails(taskId).Select(t => t.TaskName).FirstOrDefault(),
                    Files = this.files.GetFilesInDirectory(taskId),
                    Status = status ? "closed" : "active"
                };

                if (!(currentTask.OwnerId == currentUser.Id || currentTask.AssignerId == currentUser.Id || currentTask.EmployeesIds.Contains(currentUser.Id)))
                {
                    fileList.Status = "closed";
                }
 
                return View(fileList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"[GetFilesInDirectory] Сървиз грешка! Уведомете администратора. {ex.Message}";
                return RedirectToAction("TaskDetails", "Tasks", new { taskId });
            }
        }

        public async Task<IActionResult> ExportFile(string fileName, int taskId)
        {
            try
            {
                var file = await this.files.ExportFile(taskId, fileName);
                var reg = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Path.GetExtension(fileName).ToLower());
                string contentType = "application/unknown";

                if (reg != null)
                {
                    string registryContentType = reg.GetValue("Content Type") as string;

                    if (!String.IsNullOrWhiteSpace(registryContentType))
                    {
                        contentType = registryContentType;
                    }
                }


                if (file == null)
                {
                    TempData["Error"] = "Грешка при извличането на файла";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return File(file, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"[ExportFile] {ex.Message}";
                return RedirectToAction("Index", "Home");
            }

        }


        #region API Calls
        public IActionResult GetFilesList(int taskId)
        {
            var result = this.files.GetFilesInDirectory(taskId);
            return Json(result);
        }

        public IActionResult DeleteFile(int taskId, string fileName)
        {
            bool result = this.files.DeleteFile(taskId, fileName);
            return Json(result);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFiles(IFormFile file1, int taskId)
        {

            if (file1 != null)
            {
                var result = await this.files.AddFile(taskId, file1);
                return Json(result);
            }
            else
            {
                return Json("Невалиден файл");
            }
        }

        #endregion
    }
}
