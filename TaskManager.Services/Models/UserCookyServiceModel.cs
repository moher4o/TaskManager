using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models
{
    [Serializable]
    public class UserCookyServiceModel : IMapFrom<Employee>
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public int RoleId { get; set; }

        public string DaeuAccaunt { get; set; }

    }
}
