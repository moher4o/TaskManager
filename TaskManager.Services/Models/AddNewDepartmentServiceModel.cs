using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models
{
    public class AddNewDepartmentServiceModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DirectorateId { get; set; }

        public bool isDeleted { get; set; } = false;

    }
}
