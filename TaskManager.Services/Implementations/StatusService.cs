using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data;

namespace TaskManager.Services.Implementations
{
    public class StatusService : IStatusService
    {
        private readonly TasksDbContext db;
        public StatusService(TasksDbContext db)
        {
            this.db = db;
        }

        public async Task<int> GetStatusIdByNameAsync(string statusName)
        {
            return await this.db.TasksStatuses
                .Where(ts => ts.StatusName == statusName)
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();
        }
    }
}
