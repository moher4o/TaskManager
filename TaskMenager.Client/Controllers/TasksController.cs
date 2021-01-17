using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services;
using TaskMenager.Client.Models.Tasks;
using static TaskManager.Common.DataConstants;

namespace TaskMenager.Client.Controllers
{
    public class TasksController : Controller
    {
        private readonly IDirectorateService directorates;
        public TasksController(IDirectorateService directorates)
        {
            this.directorates = directorates;
        }

        public IActionResult CreateNewTask()
        {
            var newRelation = new AddNewTaskViewModel();

            newRelation = TasksModelPrepareForView(newRelation);

            return View(newRelation);
        }

        private AddNewTaskViewModel TasksModelPrepareForView(AddNewTaskViewModel newRelation)
        {
            newRelation.Directorates = this.directorates.GetDirectoratesNames()
                                               .Select(a => new SelectListItem
                                               {
                                                   Text = a,
                                                   Value = a
                                               })
                                               .ToList();
            newRelation.Directorates.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = ChooseValue,
                Selected = true
            });
            newRelation.Departments.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = ChooseValue,
                Selected = true
            });
            newRelation.Sectors.Insert(0, new SelectListItem
            {
                Text = ChooseValue,
                Value = ChooseValue,
                Selected = true
            });
            //TODO.....
            return newRelation;
        }
    }
}
