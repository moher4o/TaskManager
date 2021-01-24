using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface ITaskTypesService
    {
        IEnumerable<SelectServiceModel> GetTaskTypesNames();
    }
}
