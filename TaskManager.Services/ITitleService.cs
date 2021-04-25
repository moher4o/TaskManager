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

        Task<List<SelectServiceModel>> GetJobTitlesAsync(bool deleted = false);

        Task<string> MarkTitleDeleted(int jobId);

        Task<string> MarkTitleActiveAsync(int jobId);

        Task<string> CreateTitleAsync(string titleName);

        Task<string> RenameTitleAsync(int jobId, string titleName);
    }
}
