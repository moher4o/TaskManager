using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Services;

namespace TaskMenager.Client.Infrastructure.Extensions
{

    public class ClaimsTransformer : IClaimsTransformation
    {

        private readonly IEmployeesService employees;
        private readonly IRolesService roles;
        private readonly ITasksService tasks;

        public ClaimsTransformer(IEmployeesService employees, IRolesService roles, ITasksService tasks)
        {
            this.employees = employees;
            this.roles = roles;
            this.tasks = tasks;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var id = ((ClaimsIdentity)principal.Identity);

            var ci = new ClaimsIdentity(id.Claims, id.AuthenticationType, id.NameClaimType, id.RoleClaimType);

            var result = await RolesChecker();
            if (result != "rolesOK")
            {
                ci.AddClaim(new Claim("DbUpdated", result));
            }

            var logedUser = principal.Identities.FirstOrDefault().Name.ToLower();

            var currentEmployee = this.employees.GetUserDataForCooky(logedUser);

            if (currentEmployee != null)
            {
                var roleName = await this.roles.UserRoleNameByRoleIdAsync(currentEmployee.RoleId);

                ci.AddClaim(new Claim("fullName", currentEmployee.FullName));
                ci.AddClaim(new Claim("userId", currentEmployee.Id.ToString()));

                if (roleName != DataConstants.Employee)
                {
                    ci.AddClaim(new Claim("permission", "Admin"));

                    if (roleName == DataConstants.SectorAdmin)
                    {
                        ci.AddClaim(new Claim("permissionType", DataConstants.SectorAdmin));
                    }
                    else if (roleName == DataConstants.DepartmentAdmin)
                    {
                        ci.AddClaim(new Claim("permissionType", DataConstants.DepartmentAdmin));
                    }
                    else if (roleName == DataConstants.DirectorateAdmin)
                    {
                        ci.AddClaim(new Claim("permissionType", DataConstants.DirectorateAdmin));
                    }
                    else if (roleName == DataConstants.SuperAdmin)
                    {
                        ci.AddClaim(new Claim("permissionType", DataConstants.SuperAdmin));
                    }
                }
                else
                {
                    ci.AddClaim(new Claim("permission", DataConstants.Employee));
                    ci.AddClaim(new Claim("permissionType", DataConstants.Employee));
                }
            }
            var cp = new ClaimsPrincipal(ci);

            //return Task.FromResult(cp);
            return cp;
        }

        private async Task<string> RolesChecker()
        {
            string result = string.Empty;
            string resultLocal = string.Empty;
            if (this.roles.RolesCount() != DataConstants.RolesCount)
            {
                result = await this.roles.CreateRolesAsync();
                if (!result.Equals("success"))
                {
                    result = "Грешка след опит за инициализиране на ролите : " + result + " ";
                }
                else
                {
                    result = "Заредени ролите";
                }


                if (this.tasks.TasksStatusCount() != DataConstants.TasksStatusCount)
                {
                    resultLocal = await this.tasks.CreateTasksStatusesAsync();
                    if (!resultLocal.Equals("success"))
                    {
                        result = result + "<  >" + "Грешка след опит за инициализиране на статусите на задачите : " + resultLocal;
                    }
                    else
                    {
                        result  = result + "<  >" + "Заредени статусите на задачите";
                    }

                }
                if (this.tasks.TasksPrioritysCount() != DataConstants.TasksPriorityCount)
                {
                    resultLocal = await this.tasks.CreateTasksPrioritiesAsync();
                    if (!resultLocal.Equals("success"))
                    {
                        result = result + "<  >" + "Грешка след опит за инициализиране на приоритетите на задачите : " + resultLocal;
                    }
                    else
                    {
                        result = result + "<  >" + "Заредени приоритетите на задачите";
                    }

                }
                if (this.tasks.TasksTypesCount() != DataConstants.TasksTypesCount)
                {
                    resultLocal = await this.tasks.CreateTasksTypesAsync();
                    if (!resultLocal.Equals("success"))
                    {
                        result = result + "<  >" + "Грешка след опит за инициализиране на типовете задачи : " + resultLocal;
                    }
                    else
                    {
                        result = result + "<  >" + "Заредени типовете задачи";
                    }

                }
                return result;

            }
            return "rolesOK";
        }
    }
}
