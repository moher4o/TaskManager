using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface ITitleService
    {
        Task<string> AddTitlesCollection(List<AddNewJobTitlesServiceModel> jobTypes);

        IEnumerable<SelectServiceModel> GetJobTitlesNames();
    }
}
