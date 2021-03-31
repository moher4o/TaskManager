using Microsoft.AspNetCore.Authorization;
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

        //[HttpGet]
       // public async Task<IActionResult> DeleteDirectorate(int dirId)
        //{
        //    var taskFromDb = await this.directorates.CheckDirectorateByIdAsync(dirId);
        //    if (!taskFromDb)
        //    {
        //        return Json(new { success = false, message = "Грешка при изтриване" });
        //    }
        //    bool result = await this.directorates.MarkTaskDeletedAsync(dirId, currentUser.Id);
        //    return Json(new { success = result, message = "Задачата е изтрита" });
       // }

        #endregion
    }
}
