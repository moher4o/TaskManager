using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models.TaskModels;
using TaskManager.WebApi.Models;


namespace TaskManager.WebApi.Controllers
{
    enum ConnectionType
    {
        GetTasks = 1,
        GetEmployees = 2,
        GetMessages = 3,
        GetNewMessages = 4,
        GetFileList = 5,
        BeginFileUpload = 6,
        GetCompanyMessages = 7,
        GetCurrentUserName = 8,
        SetWorckedHowers = 21,
        SetUserToken = 22,
        SendMessage = 23,
        SetNote = 24,
        TaskSendMessage = 25,
        GetFile = 26,
        UploadChunk = 27,
        EndFileUpload = 28,
        DeleteTaskFile = 29
    }
    public class WorkController : BaseController
    {
        private readonly IManageFilesService files;
        protected readonly IEmployeesService employees;
        protected readonly ITasksService tasks;
        private readonly IDateManagementConfiguration dateConfiguration;
        protected readonly IMessageService mobmessage;
        private readonly INotesService taskNotes;
        private readonly IWebHostEnvironment environment;

        public WorkController(IDateManagementConfiguration _dateConfiguration, IManageFilesService _files, IEmployeesService _employees, ITasksService _tasks, INotesService _taskNotes, IMessageService _mobmessage, IWebHostEnvironment _environment)
        {
            this.files = _files;
            this.tasks = _tasks;
            this.employees = _employees;
            this.dateConfiguration = _dateConfiguration;
            this.mobmessage = _mobmessage;
            taskNotes = _taskNotes;
            environment = _environment ?? throw new ArgumentNullException(nameof(_environment));
        }

        [HttpGet]
        public async Task<ResponceApiModel> Get([FromQuery] string userSecretKey, [FromQuery] DateTime workdate, [FromQuery] int lastMessageId, [FromQuery] int taskId, [FromQuery] string fileName, [FromQuery] int rType)
        {
            var responce = new ResponceApiModel();
            if (rType == (int)ConnectionType.GetTasks)
            {
                return await GetTasksAsync(userSecretKey, workdate, responce);
            }
            else if (rType == (int)ConnectionType.GetEmployees)
            {
                return await GetEmployeesAsync(userSecretKey, responce);
            }
            else if (rType == (int)ConnectionType.GetMessages)
            {
                return await GetMessagesAsync(userSecretKey, responce);
            }
            else if (rType == (int)ConnectionType.GetNewMessages)
            {
                return await GetNewMessagesAsync(userSecretKey, lastMessageId, responce);
            }
            else if (rType == (int)ConnectionType.GetFileList)
            {
                return await GetFileListAsync(userSecretKey, taskId, responce);
            }
            else if (rType == (int)ConnectionType.BeginFileUpload)
            {
                return await BeginFileUpload(userSecretKey, fileName, responce);
            }
            else if (rType == (int)ConnectionType.GetCompanyMessages)
            {
                return await GetTaskMessagesAsync(userSecretKey, taskId, responce);
            }
            else if (rType == (int)ConnectionType.GetCurrentUserName)
            {
                return await GetCurrentUserName(userSecretKey, responce);
            }

            else
            {
                return responce;
            }
        }

        private async Task<ResponceApiModel> GetCurrentUserName(string userSecretKey, ResponceApiModel responce)
        {
            try
            {

                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                var currentUser = await this.employees.GetEmployeeNameByIdAsync(userId);

                if (userId > 0)
                {
                    responce.UserName = currentUser;
                    responce.ApiResponce = "success";
                }
                return responce;
            }
            catch (Exception)
            {
                return responce;
            }

        }

        /// <summary>
        /// Start uploading a new file to the server.
        /// This method will allocate a unique file handle and create an empty file in the temporary upload folder.
        /// </summary>
        /// <param name="fileName">The name of the file to upload. This name will be used in the created file handle.</param>
        /// <returns>The created file handle when the file was successfully allocated. Or an error if a file with that name is already being uploaded.</returns>
        private async Task<ResponceApiModel> BeginFileUpload(string userSecretKey, string fileName, ResponceApiModel responce)
        {
            try
            {
                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                if (userId <= 0)
                {
                    return responce;
                }

                if (string.IsNullOrEmpty(fileName))
                    return responce;

                var filePath = Path.Combine(environment.ContentRootPath, "temp");

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath); //Create the temp upload directory if it doesn't exist yet.

                fileName = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase)); //Remove the extension.
                var tempFileName = $"{fileName} {Guid.NewGuid()}{Path.GetExtension(fileName)}"; //Build the temp filename.

                //Create a new empty file that will be filled later chunk by chunk.
                var fs = new FileStream(Path.Combine(filePath, tempFileName), FileMode.CreateNew);
                fs.Close();
                responce.FilesNameList.Add(tempFileName);
                responce.ApiResponce = "success";
            }
            catch (Exception e)
            {
                //_logger.LogError(e, "Error");
                return responce;
            }

            return responce;
        }

        private async Task<ResponceApiModel> GetFileListAsync(string userSecretKey, int taskId, ResponceApiModel responce)
        {
            try
            {
                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                var currentUser = await this.employees.GetEmployeeByIdAsync(userId);
                var currentTask = await this.tasks.GetTaskDetails(taskId)
                                            .ProjectTo<TaskApiViewModel>()
                                            .FirstOrDefaultAsync();

                if (currentTask != null)
                {
                    var assignedEmployees = currentTask.Colleagues
                                            .Where(e => e.isDeleted == false)
                                            .Select(e => e.Id).ToList();
                    if (currentUser.RoleName == DataConstants.SuperAdmin || currentTask.OwnerId == userId || currentTask.AssignerId == userId || assignedEmployees.Contains(userId))
                    {
                        responce.FilesNameList = this.files.GetFilesInDirectory(taskId);
                        responce.ApiResponce = "success";
                    }
                    return responce;

                }
                else
                {
                    return responce;
                }
            }
            catch (Exception)
            {
                return responce;
            }

        }
        private async Task<ResponceApiModel> GetNewMessagesAsync(string userSecretKey, int lastMessageId, ResponceApiModel responce)
        {
            try
            {

                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                if (userId > 0)
                {
                    responce.UserMessages = await this.mobmessage.GetNewUserMessages(userId, lastMessageId);
                    responce.ApiResponce = "success";
                }
                return responce;
            }
            catch (Exception)
            {
                return responce;
            }

        }
        private async Task<ResponceApiModel> GetMessagesAsync(string userSecretKey, ResponceApiModel responce)
        {
            try
            {
                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                if (userId > 0)
                {
                    responce.UserMessages = await this.mobmessage.GetLast50UserMessages(userId, null);
                    responce.ApiResponce = "success";
                }

                return responce;
            }
            catch (Exception)
            {

                return responce;
            }

        }

        private async Task<ResponceApiModel> GetTaskMessagesAsync(string userSecretKey, int taskId, ResponceApiModel responce)
        {
            try
            {
                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                if (userId > 0)
                {
                    var task = this.tasks.GetTaskDetails(taskId).FirstOrDefault();
                    //var participation = await task.Where(t => t.AssignedExperts.Contains(new Data.Models.EmployeesTasks
                    //{
                    //    EmployeeId = userId,
                    //    TaskId = taskId,
                    //    isDeleted = false
                    //})).FirstOrDefaultAsync();
                    if (task.AssignedExperts.Where(e => e.EmployeeId == userId).FirstOrDefault() != null)
                    {
                        responce.UserMessages = await this.mobmessage.Get50CompanyMessages(userId, taskId);
                        responce.ApiResponce = "success";
                    }
                }

                return responce;
            }
            catch (Exception)
            {

                return responce;
            }

        }


        private async Task<ResponceApiModel> GetEmployeesAsync(string userSecretKey, ResponceApiModel responce)
        {
            var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
            if (!string.IsNullOrWhiteSpace(username))
            {
                var users = await this.employees.GetAllUsers();
                var data = users.Select(u => new UsersListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    JobTitleName = u.JobTitleName,
                    Email = u.Email,
                    DirectorateName = u.DirectorateName,
                    DepartmentName = u.DepartmentName,
                    SectorName = u.SectorName,
                    TelephoneNumber = u.TelephoneNumber,
                    MobileNumber = u.MobileNumber,
                    HasMobileApp = u.HasTokenHash
                }).ToList();

                foreach (var user in data)
                {
                    if (!string.IsNullOrWhiteSpace(user.TelephoneNumber) && Regex.Match(user.TelephoneNumber, @"^\d{4}$").Success)
                    {
                        user.TelephoneNumber = "02949" + user.TelephoneNumber;
                    }
                    else
                    {
                        user.TelephoneNumber = user.TelephoneNumber;
                    }
                }

                responce.Employees = data;
                responce.ApiResponce = "success";
            }
            return responce;
        }

        private async Task<ResponceApiModel> GetTasksAsync(string userSecretKey, DateTime workdate, ResponceApiModel responce)
        {
            var result = new List<TaskApiModel>();
            var nulltask = new TaskApiModel()             //за информация, ако няма параметри
            {
                Id = 1,
                TaskName = "Не са подадени валидни параметри",
                TaskStatusName = "работи се",
                EmployeeHoursToday = 0,
                TaskPriorityName = "нисък",
                ApprovedByAdmninName = "Ангел Вуков",
                ApprovedToday = true,
                ChildrenCount = 0,
                EmployeeHours = 0,
                EndDate = DateTime.Now.Date,
                EndDatePrognose = DateTime.Now.Date,
                FilesCount = 0,
                HoursLimit = 0,
                NotesCount = 0,
                ParentTaskId = 0,
                TaskNoteForToday = "",
                TaskTypeName = DataConstants.TaskTypeSystem
            };


            if (string.IsNullOrWhiteSpace(userSecretKey))
            {
                nulltask.TaskName = "Невалидни параметри - потребителско име";
                result.Add(nulltask);
                responce.Taskove = result;
                return responce;
            }
            else if (workdate.Date < DateTime.Today.AddYears(-30))
            {
                nulltask.TaskName = "Невалидни параметри - дата";
                result.Add(nulltask);
                responce.Taskove = result;
                return responce;
            }

            var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
            if (string.IsNullOrWhiteSpace(username))
            {
                nulltask.TaskName = "Невалидни параметри - Няма такъв потребител или е неактивен";
                result.Add(nulltask);
                responce.Taskove = result;
                return responce;
            }

            var emptasks = await this.employees.GetUserActiveTaskAsync(username, workdate.Date);

            foreach (var itemTask in emptasks.Where(at => at.TaskStatusName == DataConstants.TaskStatusInProgres))
            {
                var item = new TaskApiModel()
                {
                    Id = itemTask.Id,
                    TaskName = itemTask.TaskName,
                    TaskStatusName = itemTask.TaskStatusName,
                    EmployeeHoursToday = itemTask.EmployeeHoursToday,
                    TaskPriorityName = itemTask.TaskPriorityName,
                    ApprovedByAdmninName = itemTask.ApprovedByAdmninName,
                    ApprovedToday = itemTask.ApprovedToday,
                    ChildrenCount = itemTask.ChildrenCount,
                    EmployeeHours = itemTask.EmployeeHours,
                    EndDate = itemTask.EndDate,
                    EndDatePrognose = itemTask.EndDatePrognose,
                    FilesCount = this.files.GetFilesInDirectory(itemTask.Id).Count(),
                    HoursLimit = itemTask.HoursLimit,
                    NotesCount = itemTask.NotesCount,
                    ParentTaskId = itemTask.ParentTaskId,
                    TaskNoteForToday = itemTask.TaskNoteForToday,
                    TaskTypeName = itemTask.TaskTypeName

                };
                result.Add(item);

            }
            responce.Taskove = result;
            responce.ApiResponce = "success";
            return responce;
        }

        [HttpPost]
        //public async Task<IActionResult> Post([FromQuery] string userSecretKey, [FromBody] AuthTaskUpdate requestMob)
        public async Task<IActionResult> Post([FromBody] AuthTaskUpdate requestMob)
        {
            try
            {
                if (requestMob.RType == (int)ConnectionType.SetWorckedHowers)
                {
                    return await SetWorckedHowersAsync(requestMob.UserSecretKey, requestMob);
                }
                else if (requestMob.RType == (int)ConnectionType.SetUserToken)
                {
                    return await SetUserTokenAsync(requestMob.UserSecretKey, requestMob.Token);
                }
                else if (requestMob.RType == (int)ConnectionType.SendMessage)
                {
                    return await SendMessageAsync(requestMob.UserSecretKey, requestMob.Message, requestMob.Receivers);
                }
                else if (requestMob.RType == (int)ConnectionType.SetNote)
                {
                    return await SetTaskNoteAsync(requestMob.UserSecretKey, requestMob);            //добавянена дневен коментар
                }
                else if (requestMob.RType == (int)ConnectionType.TaskSendMessage)
                {
                    return await TaskMessageAsync(requestMob);            //Изпращане на съобщение до участници в задача
                }
                else if (requestMob.RType == (int)ConnectionType.GetFile)
                {
                    return await ExportFileAsync(requestMob);            //файл download
                }
                else if (requestMob.RType == (int)ConnectionType.UploadChunk)
                {
                    return await UploadChunk(requestMob);            //файл 2 upload
                }
                else if (requestMob.RType == (int)ConnectionType.EndFileUpload)
                {
                    return await EndFileUpload(requestMob);            //файл 3 upload
                }
                else if (requestMob.RType == (int)ConnectionType.DeleteTaskFile)
                {
                    return await DeleteTaskFile(requestMob);            //изтриване на файл
                }


                else
                {
                    return BadRequest("Iligal Type");
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> DeleteTaskFile(AuthTaskUpdate requestMob)
        {

            try
            {
                var result = this.files.DeleteTaskFile(requestMob.TaskId, requestMob.FileName);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Upload a part of a media file.
        /// This method takes a part of the media file and appends it to the incomplete file. This method is to be called repeatedly until the upload is complete.
        /// </summary>
        /// <param name="mediaChunk">The chunk of the media file to upload.</param>
        /// <returns>Returns the Ok code if the chunk was uploaded and appended successfully. Or an error when it failed.</returns>
        private async Task<IActionResult> UploadChunk(AuthTaskUpdate requestMob)
        {
            var path = Path.Combine(environment.ContentRootPath, "temp", requestMob.Chunk.FileHandle);
            var fileInfo = new FileInfo(path);
            var start = Convert.ToInt64(requestMob.Chunk.StartAt);

            if (!fileInfo.Exists)
                return NotFound(); //Temp file not found, maybe BeginFileUpload was not called?

            if (fileInfo.Length != start)
                return BadRequest(); //The temp file is not the same length as the starting position of the next chunk, Maybe they are sent out of order?

            try
            {
                using var fs = new FileStream(path, FileMode.Append);

                var bytes = Convert.FromBase64String(requestMob.Chunk.Data);
                fs.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return Ok();
        }
        /// <summary>
        /// Finish a file upload and copy the completed file to the upload folder so it can be streamed or retrieved.
        /// </summary>
        /// <param name="fileHandle">The file handle of the file that the upload is complete for.</param>
        /// <param name="quitUpload">If this is true the file upload will be aborted. The temporary file will be deleted.</param>
        /// <param name="fileSize">The size of the original file that was uploaded. This is used to check if the upload was successful.</param>
        /// <returns>Code Ok if the upload was successfully ended. Code 404 if the file handle was not found. Or code 500 if the file could not be moved or deleted.</returns>
        public async Task<IActionResult> EndFileUpload(AuthTaskUpdate requestMob)
        {
            var result = string.Empty;
            var fileInfo = new FileInfo(Path.Combine(environment.ContentRootPath, "temp", requestMob.FileName));
            if (!fileInfo.Exists)
                return NotFound(); //Temp file not found, maybe BeginFileUpload was not called?
            try
            {
                if (requestMob.QuitUpload)
                    fileInfo.Delete(); //Upload is being aborted, so the temp file is no longer needed.
                else
                {
                    if (fileInfo.Length != requestMob.FileSize)
                        return Conflict(); //The local file does not have the same size as the file that was uploaded. This could indicate the upload was not completed properly.

                    result = await this.files.AddFile(requestMob.TaskId, fileInfo, requestMob.Message);  //в requestMob.Message е оригиналното име на файла
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(e, "Error");
                return BadRequest(ex.Message);
            }
            return Ok(result);
        }
        private async Task<IActionResult> ExportFileAsync(AuthTaskUpdate requestMob)
        {
            try
            {
                var userId = await this.employees.GetUserIdBySKAsync(requestMob.UserSecretKey);
                var currentUser = await this.employees.GetEmployeeByIdAsync(userId);
                var currentTask = await this.tasks.GetTaskDetails(requestMob.TaskId)
                                            .ProjectTo<TaskApiViewModel>()
                                            .FirstOrDefaultAsync();

                if (currentTask != null)
                {
                    var assignedEmployees = currentTask.Colleagues
                                            .Where(e => e.isDeleted == false)
                                            .Select(e => e.Id).ToList();
                    if (currentUser.RoleName == DataConstants.SuperAdmin || (currentUser.RoleName == DataConstants.DirectorateAdmin && currentTask.DirectorateId == currentUser.DirectorateId) || (currentUser.RoleName == DataConstants.DepartmentAdmin && currentTask.DepartmentId == currentUser.DepartmentId) || (currentUser.RoleName == DataConstants.SectorAdmin && currentTask.SectorId == currentUser.SectorId) || currentTask.OwnerId == userId || currentTask.AssignerId == userId || assignedEmployees.Contains(userId))
                    {
                        var file = await this.files.ExportFile(requestMob.TaskId, requestMob.FileName);
                        var reg = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Path.GetExtension(requestMob.FileName).ToLower());
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
                            return BadRequest();
                        }
                        else
                        {
                            return File(file, contentType, requestMob.FileName);
                        }
                    }
                    return BadRequest();

                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        //private async Task<IActionResult> TaskMessageAsync(AuthTaskUpdate requestMob)
        //{
        //    int fromUserId = await this.employees.GetUserIdBySKAsync(requestMob.UserSecretKey);
        //    string senderName = await this.employees.GetEmployeeNameByIdAsync(fromUserId);

        //    var currentTask = await this.tasks.GetTaskDetails(requestMob.TaskId)
        //                                .ProjectTo<TaskApiViewModel>()
        //                                .FirstOrDefaultAsync();
        //    if (currentTask != null)
        //    {
        //        var receivers = currentTask.Colleagues
        //                                .Where(e => e.isDeleted == false)
        //                                .Select(e => e.Id).ToList();
        //        if (receivers.Count > 0 && !string.IsNullOrWhiteSpace(requestMob.Message) && !string.IsNullOrWhiteSpace(senderName))
        //        {

        //            var sendResult = await this.mobmessage.SendMessage($"{senderName} :", requestMob.Message, receivers, fromUserId);
        //            if (sendResult)
        //            {
        //                return Ok();
        //            }
        //            else
        //            {
        //                return BadRequest("No valid users!");
        //            }

        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        private async Task<IActionResult> TaskMessageAsync(AuthTaskUpdate requestMob)
        {
            try
            {
                int fromUserId = await this.employees.GetUserIdBySKAsync(requestMob.UserSecretKey);
                string senderName = await this.employees.GetEmployeeNameByIdAsync(fromUserId);

                var currentTask = await this.tasks.GetTaskDetails(requestMob.TaskId)
                                            .ProjectTo<TaskApiViewModel>()
                                            .FirstOrDefaultAsync();
                if (currentTask != null)
                {
                    var receivers = currentTask.Colleagues
                                            .Where(e => e.isDeleted == false)
                                            .Select(e => e.Id).ToList();
                    if (receivers.Count > 0 && !string.IsNullOrWhiteSpace(requestMob.Message) && !string.IsNullOrWhiteSpace(senderName))
                    {

                        var sendResult = await this.mobmessage.SendMessage($"{senderName} :", requestMob.Message, receivers, currentTask.Id, fromUserId);
                        if (sendResult)
                        {
                            return Ok();
                        }
                        else
                        {
                            return BadRequest();
                        }

                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }


        private async Task<IActionResult> SetTaskNoteAsync(string userSecretKey, AuthTaskUpdate requestMob)    //AddDateNote в TaskController
        {
            try
            {
                var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
                var user = this.employees.GetUserDataForCooky(username);

                var taskFromDb = await this.tasks.CheckTaskByIdAsync(requestMob.TaskId);
                if (!taskFromDb)
                {
                    return BadRequest("Database not updated");
                }
                var model = new AddNoteToTaskServiceModel()
                {
                    TaskId = requestMob.TaskId,
                    EmployeeId = user.Id,
                    Text = requestMob.Note,
                    WorkDate = requestMob.WorkDate.Date
                };
                bool result = await this.tasks.SetTaskEmpNoteForDateAsync(model);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Database not updated");
                }
            }
            catch (Exception)
            {
                return BadRequest("Database not updated");
            }
        }

        private async Task<IActionResult> SendMessageAsync(string userSecretKey, string message, ICollection<int> receivers)
        {
            int fromUserId = await this.employees.GetUserIdBySKAsync(userSecretKey);
            string senderName = await this.employees.GetEmployeeNameByIdAsync(fromUserId);
            if (receivers.Count > 0 && !string.IsNullOrWhiteSpace(message) && !string.IsNullOrWhiteSpace(senderName))
            {

                var sendResult = await this.mobmessage.SendMessage($"{senderName} :", message, receivers, fromUserId);
                //sendResult = this.mobmessage.MessTest($"{senderName} :", message, receivers, fromUserId);
                if (sendResult)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("No valid users!");
                }

            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<IActionResult> SetUserTokenAsync(string userSecretKey, string token)
        {
            try
            {
                string result = String.Empty;
                var userId = await this.employees.GetUserIdBySKAsync(userSecretKey);
                if (userId != 0)
                {
                    result = await this.employees.AddTokenHash(userId, token);
                    if (result == "success")
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("Database not updated");
                    }
                }
                else
                {
                    return BadRequest("Invalid user");
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }


        private async Task<IActionResult> SetWorckedHowersAsync(string userSecretKey, AuthTaskUpdate requestMob)
        {
            try
            {
                var username = await this.employees.GetUserNameBySKAsync(userSecretKey);
                var user = this.employees.GetUserDataForCooky(username);
                var systemTaskList = await this.tasks.GetSystemTasksAsync();
                //var inTime = requestMob.workDate.Date < DateTime.Now.Date.AddDays(-7) ? false : true;

                string result = String.Empty;
                if (systemTaskList.Any(st => st.Id == requestMob.TaskId))
                {
                    var systemTaskName = systemTaskList.Where(st => st.Id == requestMob.TaskId).Select(st => st.TextValue).FirstOrDefault();
                    if (systemTaskName == "Отпуски")
                    {
                        result = await this.SetDateSystemTasks(user.Id, requestMob.WorkDate.Date, true, false);
                    }
                    else if (systemTaskName == "Болнични")
                    {
                        result = await this.SetDateSystemTasks(user.Id, requestMob.WorkDate.Date, false, true);
                    }
                }
                else
                {
                    var inTime = this.CheckDate(requestMob.WorkDate.Date);
                    var workedHours = new TaskWorkedHoursServiceModel()
                    {
                        EmployeeId = user.Id,
                        TaskId = requestMob.TaskId,
                        HoursSpend = requestMob.HoursSpend,
                        WorkDate = requestMob.WorkDate.Date,
                        RegistrationDate = DateTime.Now.Date,
                        InTimeRecord = inTime
                    };
                    var removedSystemTask = await RemoveSystemTasks(user.Id, requestMob.WorkDate.Date);
                    if (removedSystemTask)
                    {
                        result = await this.tasks.SetWorkedHoursAsync(workedHours);
                    }
                    else
                    {
                        result = "error";
                    }
                }

                if (result == "success")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Database not updated");
                }
            }
            catch (Exception)
            {
                return BadRequest("Database not updated");
            }

        }

        [HttpPut]
        public async Task<IActionResult> ClearSystemTasks([FromBody] AuthTaskUpdate requestMob)
        {
            try
            {
                var username = await this.employees.GetUserNameBySKAsync(requestMob.UserSecretKey);
                var user = this.employees.GetUserDataForCooky(username);
                if (user == null)
                {
                    return BadRequest("Database not updated");
                }
                bool result = await this.RemoveSystemTasks(user.Id, requestMob.WorkDate.Date);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Database not updated");
                }
            }
            catch (Exception)
            {
                return BadRequest("Database not updated");
            }
        }

        private async Task<string> SetDateSystemTasks(int userId, DateTime workDate, bool isholiday = false, bool isill = false)
        {
            try
            {
                var result = string.Empty;
                var message = string.Empty;
                if (isholiday || isill)
                {
                    var dateTaskList = await this.employees.GetAllUserTaskAsync(userId, workDate.Date);
                    var inTime = CheckDate(workDate.Date);
                    foreach (var itemTask in dateTaskList)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = itemTask.Id,
                            HoursSpend = 0,
                            WorkDate = workDate.Date,
                        };

                        await this.tasks.SetWorkedHoursWithDeletedAsync(workedHours);
                    }
                    if (isholiday)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = await this.tasks.GetSystemTaskIdByNameAsync("Отпуски"),
                            HoursSpend = 8,
                            WorkDate = workDate.Date,
                            RegistrationDate = DateTime.Now.Date,
                            InTimeRecord = inTime,
                            Approved = false
                        };
                        result = await this.tasks.SetWorkedHoursAsync(workedHours);
                        message = "Отпускът е отразен в системата";
                    }
                    else if (isill)
                    {
                        var workedHours = new TaskWorkedHoursServiceModel()
                        {
                            EmployeeId = userId,
                            TaskId = await this.tasks.GetSystemTaskIdByNameAsync("Болнични"),
                            HoursSpend = 8,
                            WorkDate = workDate.Date,
                            RegistrationDate = DateTime.Now.Date,
                            InTimeRecord = inTime,
                            Approved = false
                        };
                        result = await this.tasks.SetWorkedHoursAsync(workedHours);
                        message = "Болничния е отразен в системата";
                    }

                }
                return result;
            }
            catch (Exception)
            {
                return "exception";
            }
        }

        private async Task<bool> RemoveSystemTasks(int userId, DateTime workDate)
        {
            try
            {
                bool result = await this.tasks.RemoveSystemTaskForDate(userId, workDate);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private bool CheckDate(DateTime workDate)   //проверява дали датата за която ще се прави отчет е в текущия отчетен период
        {
            var daysAfterOtchet = new List<int>();
            var ot4etday = this.dateConfiguration.ReportDate;
            var todayDate = DateTime.Now.Date;
            var todayDayOfWeek = ((int)todayDate.DayOfWeek);
            var diference = todayDate.Date - workDate.Date;
            //int diferenceValue = todayDayOfWeek > ot4etday ? 8 : 7;

            if (diference.TotalDays < 0)
            {
                return true;
            }

            if (diference.TotalDays <= 7)
            {
                if (diference.TotalDays < 7)
                {
                    for (int i = ot4etday + 1; i <= ((todayDayOfWeek < (ot4etday + 1)) ? (todayDayOfWeek + 7) : todayDayOfWeek); i++)
                    {
                        daysAfterOtchet.Add(i > 6 ? i - 7 : i);
                    }
                }
                if (todayDayOfWeek == ot4etday + 1 || (todayDayOfWeek == 0 && ot4etday == 7))    //за да може в деня след отчета да се попълва за миналата седмица (петък е примерен)
                {
                    for (int i = 1; i <= 7; i++)
                    {
                        if (!daysAfterOtchet.Contains(i))
                        {
                            daysAfterOtchet.Add(i);
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            if (daysAfterOtchet.Contains(((int)workDate.DayOfWeek)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
