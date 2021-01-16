﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
    public interface IDepartmentsService
    {
        Task<string> AddDepartmentsCollection(List<AddNewDepartmentServiceModel> departments);
    }
}
