using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRoleAndClaimRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRoleAndClaimRequirement requirement)
        {
            var authfilterContext = context.Resource as AuthorizationFilterContext;
            if(authfilterContext == null)
            {
                return Task.CompletedTask;
            }

            string loggedInAdminId = context.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = authfilterContext.HttpContext.Request.Query["userId"];

            /*if (context.User.IsInRole("AdminRole") && context.User.HasClaim(c => c.Type == "EditRolePolicy" && c.Value == "true")
            && loggedInAdminId.ToLower() != AdminIdBeingEdited.ToLower())
            {
                context.Succeed(requirement);
            }*/

            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
                adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
