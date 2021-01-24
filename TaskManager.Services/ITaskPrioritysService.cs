using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface ITaskPrioritysService
    {
        IEnumerable<SelectServiceModel> GetTaskPrioritysNames();
    }
}
