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

        IEnumerable<string> GetSectorsNames();

        IEnumerable<string> GetSectorsNames(int? sectorId);

        IEnumerable<string> GetSectorsNamesByDepartment(int? departmentId);

        IEnumerable<string> GetSectorsNamesByDirectorate(int? directorateId);

    }
}
