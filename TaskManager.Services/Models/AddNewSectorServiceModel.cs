using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Services.Models
{
    public class AddNewSectorServiceModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DirectorateId { get; set; }

        public int DepartmentId { get; set; }

        public bool isDeleted { get; set; } = false;

    }
}
