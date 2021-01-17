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

        public ClaimsTransformer(IEmployeesService employees, IRolesService roles)
        {
            this.employees = employees;
            this.roles = roles;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var id = ((ClaimsIdentity)principal.Identity);

            var ci = new ClaimsIdentity(id.Claims, id.AuthenticationType, id.NameClaimType, id.RoleClaimType);

            var logedUser = principal.Identities.FirstOrDefault().Name.ToLower();

            var currentEmployee = this.employees.GetUserDataForCooky(logedUser);

            var roleName = await this.roles.UserRoleNameByRoleIdAsync(currentEmployee.RoleId);

            ci.AddClaim(new Claim("fullName", currentEmployee.FullName));

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

            var cp = new ClaimsPrincipal(ci);

            //return Task.FromResult(cp);
            return cp;
        }
    }
}
