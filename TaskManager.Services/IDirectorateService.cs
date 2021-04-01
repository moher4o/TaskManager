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

        //IEnumerable<SelectServiceModel> GetDirectoratesNames();

        IEnumerable<SelectServiceModel> GetDirectoratesNames(int? directorateId);

        Task<List<AddNewDirectorateServiceModel>> GetDirectoratesAsync(bool deleted = false);

        Task<string> MarkDirectorateDeleted(int dirId);
    }
}
