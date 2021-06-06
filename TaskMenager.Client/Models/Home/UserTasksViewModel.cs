using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Services.Models.TaskModels;
using static TaskManager.Common.DataConstants;

namespace TaskMenager.Client.Models.Home
{
    public class UserTasksViewModel
    {
        public int userId { get; set; }

        public int totalHoursPerDay { get; set; } = 0;

        public int totalHoursPerDayMax { get; set; } = TotalHoursPerDay;

        public DateTime workDate { get; set; } = DateTime.Now.Date;

        public IEnumerable<TaskFewInfoServiceModel> ActiveTasks { get; set; } = new List<TaskFewInfoServiceModel>();

        public IEnumerable<TaskFewInfoServiceModel> AssignerTasks { get; set; } = new List<TaskFewInfoServiceModel>();

        public IEnumerable<TaskFewInfoServiceModel> CreatedTasks { get; set; } = new List<TaskFewInfoServiceModel>();

    }
}
