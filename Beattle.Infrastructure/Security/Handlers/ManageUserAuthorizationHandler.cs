using System;
using System.Collections.Generic;
using System.Text;
using Beattle.Identity;
using System.Threading.Tasks;
using Beattle.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Beattle.Infrastructure.Security.Handlers
{
    public class ManageUserAuthorizationHandler : AuthorizationHandler<UserAccountAuthorizationRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAccountAuthorizationRequirement requirement, string targetUserId)
        {
            if (context.User == null ||
                (requirement.OperationName != AccountManagementOperations.CreateOperationName &&
                 requirement.OperationName != AccountManagementOperations.UpdateOperationName &&
                 requirement.OperationName != AccountManagementOperations.DeleteOperationName))
                return Task.CompletedTask;
            if (context.User.HasClaim(ApplicationClaimType.Authorization, AuthorizationManager.ManageUsers) 
                || Security.IsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

}
