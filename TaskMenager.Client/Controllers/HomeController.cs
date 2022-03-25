using Google.Authenticator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Services.Models;
using TaskMenager.Client.Infrastructure.Extensions;
using TaskMenager.Client.Models;
using TaskMenager.Client.Models.Home;
using static TaskManager.Common.DataConstants;


namespace TaskMenager.Client.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IManageFilesService files;
        private readonly IApprovalConfiguration approvalConfiguration;
        private readonly I2FAConfiguration twoFAConfiguration;

        public HomeController(ILogger<HomeController> logger, IEmployeesService employees, IManageFilesService files, ITasksService tasks, IHttpContextAccessor httpContextAccessor, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration, IApprovalConfiguration _approvalConfiguration, I2FAConfiguration _twoFAConfiguration) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
        {
            this.files = files;
            this.approvalConfiguration = _approvalConfiguration;
            twoFAConfiguration = _twoFAConfiguration;
         }


        public IActionResult Index()
        {
            if (this.User.Claims.Any(cl => cl.Type == "DbUpdated"))
            {
                TempData["Error"] = this.User.Claims.Where(cl => cl.Type == "DbUpdated").Select(cl => cl.Value).FirstOrDefault();
                return RedirectToAction("NotAuthorized", "Base");
            }


            if (this.User.Claims.Any(cl => cl.Value == "Guest"))
            {
                return RedirectToAction("WellCome", "Users");
            }

            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }



            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult NotImplemented()
        {
            return View();
        }

        //public IActionResult SecondAuthentication()
        //{
        //    var userToAuthenticate = currentUser.Email;
        //    var twoFactorAuthenticator = new TwoFactorAuthenticator();
        //    var TwoFactorSecretCode = twoFAConfiguration.TwoFactorSecretCode;
        //    var accountSecretKey = $"{TwoFactorSecretCode}-{userToAuthenticate}";
        //    var setupCode = twoFactorAuthenticator.GenerateSetupCode("TaskManager", userToAuthenticate,
        //        Encoding.ASCII.GetBytes(accountSecretKey));
        //    var twoFAModel = new TwoFAViewModel()
        //    {
        //        Account = currentUser.Email,
        //        ManualEntryKey = setupCode.ManualEntryKey,
        //        QrCodeSetupImageUrl = setupCode.QrCodeSetupImageUrl
        //    };

        //    return View(twoFAModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult SecondAuthentication(TwoFAViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var userToAuthenticate = currentUser.Email;
        //    var TwoFactorSecretCode = twoFAConfiguration.TwoFactorSecretCode;
        //    var accountSecretKey = $"{TwoFactorSecretCode}-{userToAuthenticate}";
        //    var twoFactorAuthenticator = new TwoFactorAuthenticator();
        //    var result = twoFactorAuthenticator
        //        .ValidateTwoFactorPIN(accountSecretKey, model.UserInputCode);
        //    if (result)
        //    {
        //        TempData["Success"] = "Кода е приет!";
        //        //HttpContext.User.Identities.FirstOrDefault().AddClaim(new Claim("your claim", "your field"));
        //        HttpContext.Response.Cookies.Append("Test_cookie", "yo");
        //        // HttpContext.Request.Headers.Add("2fa", "true");
        //        return RedirectToAction(nameof(Index));
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Невалиден код!";
        //        return RedirectToAction(nameof(SecondAuthentication));
        //    }
           
        //}

        [HttpGet]
        public async Task<PartialViewResult> GetDateTasks(int? userId, DateTime? workDate)
        {
            int identityId = userId.HasValue ? userId.Value : currentUser.Id;
            DateTime dateToProcess = workDate.HasValue ? workDate.Value.Date : DateTime.Now.Date;

            var currentEmployee = new UserTasksViewModel()
            {
                userId = identityId,
                workDate = dateToProcess.Date,
                ActiveTasks =await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date),
                ReportApproval = this.approvalConfiguration.ReportApproval
                //AssignerTasks = await this.employees.GetUserAssignerTaskAsync(currentUser.Id)
            };
            foreach (var task in currentEmployee.ActiveTasks.Where(at => at.TaskTypeName != TaskTypeSystem).ToList())
            {
                task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
            }

            currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Where(at => at.TaskTypeName != TaskTypeSystem).Sum(at => at.EmployeeHoursToday);

            return PartialView("~/Views/Home/_ShowTasksPartial.cshtml", currentEmployee);
        }

        public async Task<JsonResult> GetDateTasksJsonAsync()
        {
            int identityId = 1;
            DateTime dateToProcess =  DateTime.Now.Date;
            var emptasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date);

            return Json(new { emptasks });
        }

        #region API Calls
        //[Authorize(Policy = Employee)]
        //[HttpGet]
        //public async Task<IActionResult> GetDateTasksAsync(int? userId, DateTime? workDate)
        //{
        //    int identityId = userId.HasValue ? userId.Value : currentUser.Id;
        //    DateTime dateToProcess = workDate.HasValue ? workDate.Value.Date : DateTime.Now.Date;

        //    var currentEmployee = new UserTasksViewModel()
        //    {
        //        userId = identityId,
        //        workDate = dateToProcess.Date,
        //        ActiveTasks = await this.employees.GetUserActiveTaskAsync(identityId, dateToProcess.Date),
        //    };
        //    foreach (var task in currentEmployee.ActiveTasks)
        //    {
        //        task.FilesCount = this.files.GetFilesInDirectory(task.Id).Count();
        //    }

        //    currentEmployee.totalHoursPerDay = currentEmployee.ActiveTasks.Sum(at => at.EmployeeHoursToday);
        //    return Json(new { currentEmployee });
        //}
        #endregion

    }
}
