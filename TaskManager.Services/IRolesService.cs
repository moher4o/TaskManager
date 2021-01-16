﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Services.Models;

namespace TaskManager.Services
{
   public interface IRolesService
   {
        Task<string> AddRolesCollection(List<AddNewRoleServiceModel> roles);

        Task<string> CreateRolesAsync();

        int RolesCount();

        Task<string> UserRoleNameByRoleIdAsync(int roleId);
    }
}
