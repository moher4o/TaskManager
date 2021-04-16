
using System;

namespace TaskManager.Services.Models
{
    [Serializable]
    public class SelectServiceModel
    {
        public string TextValue { get; set; }

        public int Id { get; set; }

        public bool isDeleted { get; set; } = false;

        public string DirectorateName { get; set; }

        public string DepartmentName { get; set; }

        public string SectorName { get; set; }

        public string Email { get; set; }
    }
}
