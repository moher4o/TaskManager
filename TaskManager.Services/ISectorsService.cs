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

        Task<IEnumerable<SelectServiceModel>> GetSectorsNamesByDepartment(int? departmentId);

        IEnumerable<SelectServiceModel> GetSectorsNamesByDirectorate(int? directorateId);

    }
}
