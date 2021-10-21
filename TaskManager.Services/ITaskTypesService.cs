using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface ITaskTypesService
    {
        IEnumerable<SelectServiceModel> GetTaskTypesNames();

        int GetTaskTypeIdByNameAsync(string typeName);
    }
}
