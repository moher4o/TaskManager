using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services.Models.TaskModels;

namespace TaskMenager.Client.Models.Tasks
{
    public class TaskChildrenViewModel
    {
        public string TaskName { get; set; }

        public List<TaskChildrensServiceModel> ChildrenTasks { get; set; } = new List<TaskChildrensServiceModel>();
    }
}
