using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface IDirectorateService
    {
        Task<string> AddDirectoratesCollection(List<AddNewDirectorateServiceModel> directorates);

        IEnumerable<string> GetDirectoratesNames();

        IEnumerable<string> GetDirectoratesNames(int? directorateId);
    }
}
