using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.WebApi.Models
{
    public class ResponceApiModel
    {
        public List<TaskApiModel> Taskove { get; set; } = new List<TaskApiModel>();

        public List<UsersListViewModel> Employees { get; set; } = new List<UsersListViewModel>();
    }
}

