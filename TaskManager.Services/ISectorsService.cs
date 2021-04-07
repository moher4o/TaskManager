using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface ISectorsService
    {
        Task<string> AddSectorsCollection(List<AddNewSectorServiceModel> sectors);

        IEnumerable<SelectServiceModel> GetSectorsNames();

        IEnumerable<SelectServiceModel> GetSectorsNames(int? sectorId);

        Task<List<AddNewSectorServiceModel>> GetSectorsAsync(bool deleted = false);

        Task<IEnumerable<SelectServiceModel>> GetSectorsNamesByDepartment(int? departmentId);

        Task<IEnumerable<SelectServiceModel>> GetSectorsNamesByDirectorate(int? directorateId);
        Task<string> CreateSectorAsync(int directoratesId, int depId, string sectorName);
        Task<string> MarkSectorDeleted(int secId);
        Task<string> MarkSectorActiveAsync(int secId);
    }
}
