using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Common.Mapping;
using TaskManager.Services.Models;

namespace TaskManager.WebApi.Models
{
    public class UsersListViewModel : IMapFrom<UserServiceModel>
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string TelephoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public string SectorName { get; set; }

        public string DepartmentName { get; set; }

        public string DirectorateName { get; set; }

        public string JobTitleName { get; set; }

        public string RoleName { get; set; }

        public string Status { get; set; }

    }
}
