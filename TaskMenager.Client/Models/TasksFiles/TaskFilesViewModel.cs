using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Models.TasksFiles
{
    public class TaskFilesViewModel
    {
        public int Id { get; set; }

        public string TaskName { get; set; }

        public string Status { get; set; }

        public IEnumerable<string> Files { get; set; } = new List<string>();

    }
}
