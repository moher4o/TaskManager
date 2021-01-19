using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services
{
    public interface ITaskPrioritysService
    {
        IEnumerable<string> GetTaskPrioritysNames();
    }
}
