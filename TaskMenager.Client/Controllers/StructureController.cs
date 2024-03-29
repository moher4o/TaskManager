﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskMenager.Client.Models.Structure;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = "Admin")]
    public class StructureController : BaseController
    {
        private readonly IApprovalConfiguration approvalConfiguration;
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;
        private readonly ITitleService titles;
        private readonly I2FAConfiguration twoFAConfiguration;


        public StructureController(ITitleService titles, IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IHttpContextAccessor httpContextAccessor, ITasksService tasks, IEmployeesService employees, IEmailService email, IWebHostEnvironment env, IEmailConfiguration _emailConfiguration, IApprovalConfiguration _approvalConfiguration, I2FAConfiguration _twoFAConfiguration) : base(httpContextAccessor, employees, tasks, email, env, _emailConfiguration)
        {
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
            this.titles = titles;
            this.approvalConfiguration = _approvalConfiguration;
            twoFAConfiguration = _twoFAConfiguration;
        }

        public IActionResult RenameTitle(int jobId)
        {
            var model = new TitleViewModel();

            model.TitleId = jobId;
            model.TitleName = this.titles.GetJobTitlesNames().Where(j => j.Id == jobId).Select(j => j.TextValue).FirstOrDefault();

            return PartialView("_RenameTitleModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameTitle(TitleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await this.titles.RenameTitleAsync(model.TitleId, model.TitleName);
                if (result == "success")
                {
                    TempData["Success"] = $"Името е успешно променено на \"{model.TitleName}\"";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_RenameTitleModalPartial", model);
        }

        public IActionResult CreateTitle()
        {
            var model = new TitleViewModel();

            model.TitleId = -999;

            return PartialView("_CreateTitleModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTitle(TitleViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.titles.CreateTitleAsync(model.TitleName);
                if (result == "success")
                {
                    TempData["Success"] = $"Длъжност \"{model.TitleName}\" е създадена успешно";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_CreateTitleModalPartial", model);
        }

        public IActionResult JobtitlesList()
        {
            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }

            return View();
        }


        public IActionResult CreateSector()
        {
            var model = new SectorViewModel();
            var dirList = this.directorates.GetDirectoratesNames(null);
            model.Directorates = dirList
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                       Value = a.Id.ToString(),
                                       Selected = true
                                   })
                                   .ToList();
            model.DirectoratesId = dirList.Select(d => d.Id).FirstOrDefault();

            var depList = this.departments.GetDepartmentsNamesByDirectorate(model.DirectoratesId);
            model.Departments = depList
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                       Value = a.Id.ToString(),
                                       Selected = true
                                   })
                                   .ToList();
            model.DepartmentsId = depList.Select(d => d.Id).FirstOrDefault();


            return PartialView("_CreateTotalNewSectorModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSector(SectorViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SecId > 0)
                {
                   string result = await this.sectors.EditSectorAsync(model.SecId, model.DirectoratesId, model.DepartmentsId, model.SectorName);
                    if (result == "success")
                    {
                        TempData["Success"] = $"Сектор \"{model.SectorName}\" е успешно редактиран";
                    }
                    else
                    {
                        TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                    }

                }
                else
                {
                    await CreateSectorAsync(model.DirectoratesId, model.DepartmentsId, model.SectorName);
                }
                
            }
            return PartialView("_CreateTotalNewSectorModalPartial", model);
        }

        public IActionResult CreateDepartment()
        {
            var model = new CreateDepViewModel();
            var dirList = this.directorates.GetDirectoratesNames(null);
            model.Directorates = dirList
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                       Value = a.Id.ToString(),
                                       Selected = true
                                   })
                                   .ToList();
            model.DirectoratesId = dirList.Select(d => d.Id).FirstOrDefault();

            return PartialView("_CreateDepartmentModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(CreateDepViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.departments.CreateDepartmentAsync(model.DirectoratesId, model.DepartmentName);
                if (result == "success")
                {
                    TempData["Success"] = $"Отдел \"{model.DepartmentName}\" е създаден успешно";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_CreateDepartmentModalPartial", model);

        }

        public async Task<IActionResult> AddSector(int depId)
        {
            var departmentHost = await this.departments.GetDepartmentAsync(depId);
            var model = new CreateDepViewModel()
            {
                DepId = departmentHost.Id,
                DirectoratesId = departmentHost.DirectorateId
            };
            return PartialView("_CreateSectorModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSector(CreateDepViewModel model)
        {
            if (ModelState.IsValid)
            {
                await CreateSectorAsync(model.DirectoratesId,model.DepId,model.DepartmentName);
            }
            return PartialView("_CreateSectorModalPartial", model);
        }

        private async Task CreateSectorAsync(int dirId, int depId, string sectorName)
        {
            string result = await this.sectors.CreateSectorAsync(dirId, depId, sectorName);
            if (result == "success")
            {
                TempData["Success"] = $"Сектор \"{sectorName}\" е създаден успешно";
            }
            else
            {
                TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
            }
        }

        public async Task<IActionResult> EditSector(int secId)
        {
            var sectorToEdit = await this.sectors.GetSectorAsync(secId);
            var model = new SectorViewModel()
            {

                SecId = sectorToEdit.Id,
                SectorName = sectorToEdit.Name,
                DirectoratesId = sectorToEdit.DirectorateId,
                Directorates = this.directorates.GetDirectoratesNames(null)
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                       Value = a.Id.ToString(),
                                       Selected = a.Id == sectorToEdit.DirectorateId
                                   })
                                   .ToList(),
                DepartmentsId = sectorToEdit.DepartmentId,
                Departments = this.departments.GetDepartmentsNamesByDirectorate(sectorToEdit.DirectorateId)
                                                   .Select(a => new SelectListItem
                                                   {
                                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                                       Value = a.Id.ToString(),
                                                       Selected = a.Id == sectorToEdit.DepartmentId
                                                   })
                                   .ToList()
            };
            return PartialView("_CreateTotalNewSectorModalPartial", model);
        }



        public async Task<IActionResult> EditDepartment(int depId)
        {
            var departmentToEdit = await this.departments.GetDepartmentAsync(depId);
            var model = new CreateDepViewModel()
            {
                DepId = departmentToEdit.Id,
                DepartmentName = departmentToEdit.Name,
                DirectoratesId = departmentToEdit.DirectorateId,
                Directorates = this.directorates.GetDirectoratesNames(null)
                                   .Select(a => new SelectListItem
                                   {
                                       Text = a.TextValue.Length <= 65 ? a.TextValue : a.TextValue.Substring(0, 61) + "...",
                                       Value = a.Id.ToString(),
                                       Selected = a.Id == departmentToEdit.DirectorateId
                                   })
                                   .ToList()
            };
            return PartialView("_EditDepartmentModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(CreateDepViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.departments.EditDepartmentDetails(model.DepId, model.DirectoratesId, model.DepartmentName);
                if (result == "success")
                {
                    TempData["Success"] = $"Отдел \"{model.DepartmentName}\" е успешно редактиран";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_EditDepartmentModalPartial", model);
        }


        public IActionResult CreateDirectorate()
        {
            var model = new RenameDirectorateViewModel();

            model.DirId = -999;

            return PartialView("_CreateDirectorateModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDirectorate(RenameDirectorateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.directorates.CreateDirectorateAsync(model.DirectorateName);
                if (result == "success")
                {
                    TempData["Success"] = $"Дирекция \"{model.DirectorateName}\" е създадена успешно";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_CreateDirectorateModalPartial", model);
        }

        public IActionResult AproveSecReport(int secId)
        {
            var model = new AproveViewModel();

            model.UnitId = secId;
            model.UnitName = this.sectors.GetSectorsNames(secId).Select(s => s.TextValue).FirstOrDefault();

            return PartialView("_AproveSecReportModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AproveSecReport(AproveViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.sectors.AproveSecReportsAsync(model.UnitId, model.AproveDate, currentUser.Id);
                if (result == "success")
                {
                    TempData["Success"] = $"Отчетите до дата {model.AproveDate.Date.ToString("dd/MM/yyyy")} са одобрени";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_AproveSecReportModalPartial", model);
        }

        public IActionResult AproveDepReport(int depId)
        {
            var model = new AproveViewModel();

            model.UnitId = depId;
            model.UnitName = this.departments.GetDepartmentsNames(depId).Select(d =>d.TextValue).FirstOrDefault();

            return PartialView("_AproveDepReportModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AproveDepReport(AproveViewModel model)
        {
            if (ModelState.IsValid)
            {
                string result = await this.departments.AproveDepReportsAsync(model.UnitId, model.AproveDate, currentUser.Id);
                if (result == "success")
                {
                    TempData["Success"] = $"Отчетите до дата {model.AproveDate.Date.ToString("dd/MM/yyyy")} са одобрени";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_AproveDepReportModalPartial", model);
        }

        public IActionResult AproveDirReport(int dirId)
        {
            var model = new AproveViewModel();

            model.UnitId = dirId;
            model.UnitName = this.directorates.GetDirectoratesNames(dirId).Select(d => d.TextValue).FirstOrDefault();

            return PartialView("_AproveDirReportModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AproveDirReport(AproveViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await this.directorates.AproveDirReportsAsync(model.UnitId, model.AproveDate, currentUser.Id);
                if (result == "success")
                {
                    TempData["Success"] = $"Отчетите до дата {model.AproveDate.Date.ToString("dd/MM/yyyy")} са одобрени";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_AproveDirReportModalPartial", model);
        }


        public IActionResult RenameDirectorate(int dirId)
        {
            var model = new RenameDirectorateViewModel();

            model.DirId = dirId;
            model.DirectorateName = this.directorates.GetDirectoratesNames(dirId).Select(d => d.TextValue).FirstOrDefault();

            return PartialView("_RenameDirectorateModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameDirectorate(RenameDirectorateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await this.directorates.RenameDirectorateAsync(model.DirId, model.DirectorateName);
                if (result == "success")
                {
                    TempData["Success"] = $"Името е успешно променено на \"{model.DirectorateName}\"";
                }
                else
                {
                    TempData["Error"] = $"[Service error] Уведомете администратора. {result}";
                }

            }
            return PartialView("_RenameDirectorateModalPartial", model);
        }



        public IActionResult DirectoratesList()
        {
            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }

            return View();
        }

        public IActionResult DepartmentsList()
        {
            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }

            return View();
        }

        public IActionResult SectorsList()
        {
            if (this.User.Claims.Any(cl => cl.Type == "2FA" && cl.Value == "false") && twoFAConfiguration.TwoFAMandatory)
            {
                return RedirectToAction("SecondAuthentication", "Users");
            }

            return View();
        }

        public async Task<IActionResult> AddUsersToSystemTasks()
        {
            var result = await this.employees.AddAllToSystemTasksAsync();
            if (!result)
            {
                TempData["Error"] = $"[employee service error] AddAllToSystemTasksAsync";
            }
            else
            {
                TempData["Success"] = "Потребителите са добавени към системните задачи";
            }

            return RedirectToAction("UsersList", "Users");
        }

        public async Task<IActionResult> RemoveEmptyEmails()
        {
            var result = await this.employees.GenerateEmailWhenEmpty();
            if (result != "success")
            {
                TempData["Error"] = $"[Service error] {result}";
            }
            else
            {
                TempData["Success"] = "Всички празни имейли са генерирани автоматично";
            }

            return RedirectToAction("UsersList", "Users");
        }

        public async Task<IActionResult> RemoveEmptySecretKey()
        {
            var result = await this.employees.GenerateSecretKeyWhenEmpty();
            if (result != "success")
            {
                TempData["Error"] = $"[Service error] {result}";
            }
            else
            {
                TempData["Success"] = "Всички празни секрети са генерирани автоматично";
            }

            return RedirectToAction("UsersList", "Users");
        }

        public async Task<IActionResult> AddSystemAccount()
        {
            var result = await this.employees.AddSystemAccountMethod();
            if (!result)
            {
                TempData["Error"] = $"[Service error]";
            }
            else
            {
                TempData["Success"] = "Успешно добавен системен акаунт";
            }

            return RedirectToAction("UsersList", "Users");
        }
        //AddSystemAccount

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> getAllJobs(bool deleted = false)
        {
            var data = await this.titles.GetJobTitlesAsync(deleted);
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTitle(int jobId)
        {
            var check = await this.titles.MarkTitleDeleted(jobId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Длъжността е изтрита" });
        }

        [HttpGet]
        public async Task<IActionResult> RestoreTitle(int jobId)
        {
            string check = await this.titles.MarkTitleActiveAsync(jobId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Длъжността е възстановена" });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllSectors(bool deleted = false)
        {
            var data = await this.sectors.GetSectorsAsync(deleted);
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments(bool deleted = false)
        {
            var data = await this.departments.GetDepartmentsAsync(deleted);
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDirectorates(bool deleted = false)
        {
            var data = await this.directorates.GetDirectoratesAsync(deleted);
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteDirectorate(int dirId)
        {
            var check = await this.directorates.MarkDirectorateDeleted(dirId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Дирекцията е изтрита" });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteDepartment(int depId)
        {
            var check = await this.departments.MarkDepartmentDeleted(depId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Отдела е изтрит" });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSector(int secId)
        {
            string check = await this.sectors.MarkSectorDeleted(secId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Сектора е изтрит" });
        }


        [HttpGet]
        public async Task<IActionResult> RestoreDirectorate(int dirId)
        {
            string check = await this.directorates.MarkDirectorateActiveAsync(dirId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Дирекцията е възстановена" });
        }

        [HttpGet]
        public async Task<IActionResult> RestoreDepartment(int depId)
        {
            string check = await this.departments.MarkDepartmentActiveAsync(depId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Отдела е възстановен" });
        }

        [HttpGet]
        public async Task<IActionResult> RestoreSector(int secId)
        {
            string check = await this.sectors.MarkSectorActiveAsync(secId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Сектора е възстановен" });
        }

        [Authorize(Policy = DataConstants.Employee)]
        [HttpGet]
        public IActionResult GetUserRole()
        {
            return Json(currentUser);
        }
        [Authorize(Policy = DataConstants.Employee)]
        [HttpGet]
        public IActionResult GetAprovalStatus()
        {
            return Json(this.approvalConfiguration.ReportApproval);
        }
        
        #endregion
    }
}
