using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public interface IStatusService
    {
        Task<int> GetStatusIdByNameAsync(string statusName);
    }
}
