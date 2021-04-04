using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;
using TaskMenager.Client.Models.Structure;

namespace TaskMenager.Client.Controllers
{
    [Authorize(Policy = DataConstants.SuperAdmin)]
    public class StructureController : BaseController
    {
        private readonly IDirectorateService directorates;
        private readonly IDepartmentsService departments;
        private readonly ISectorsService sectors;


        public StructureController(IDirectorateService directorates, IDepartmentsService departments, ISectorsService sectors, IHttpContextAccessor httpContextAccessor, ITasksService tasks, IEmployeesService employees) : base(httpContextAccessor, employees, tasks)
        {
            this.directorates = directorates;
            this.departments = departments;
            this.sectors = sectors;
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
            return View();
        }

        #region API Calls
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
        public async Task<IActionResult> RestoreDirectorate(int dirId)
        {
            string check = await this.directorates.MarkDirectorateActiveAsync(dirId);
            if (check != "success")
            {
                return Json(new { success = false, message = check });
            }
            return Json(new { success = true, message = "Дирекцията е възстановена" });
        }

        #endregion
    }
}
