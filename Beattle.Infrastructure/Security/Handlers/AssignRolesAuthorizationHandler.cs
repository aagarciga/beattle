using Beattle.Identity;
using Beattle.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Beattle.Infrastructure.Security.Handlers
{
    public class AssignRolesAuthorizationHandler : AuthorizationHandler<AssignRolesAuthorizationRequirement, Tuple<string[], string[]>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AssignRolesAuthorizationRequirement requirement, Tuple<string[], string[]> newAndCurrentRoles)
        {
            if (!RolesHasChanges(newAndCurrentRoles.Item1, newAndCurrentRoles.Item2))
            {
                context.Succeed(requirement);
            }
            else if (context.User.HasClaim(ApplicationClaimType.Authorization, AuthorizationManager.AssignRoles))
            {
                // If user has ViewRoles authorization, then it can assign any roles
                if (context.User.HasClaim(ApplicationClaimType.Authorization, AuthorizationManager.ViewRoles)) 
                    context.Succeed(requirement);

                // Else user can only assign roles they're part of
                else if (IsUserInAllAddedRoles(context.User, newAndCurrentRoles.Item1, newAndCurrentRoles.Item2)) 
                    context.Succeed(requirement);
            }


            return Task.CompletedTask;
        }

        /// <summary>
        /// Comapares if exist any variation between the current roles set and the new one
        /// </summary>
        /// <param name="newRoles"></param>
        /// <param name="currentRoles"></param>
        /// <returns>True if any change</returns>
        private bool RolesHasChanges(string[] newRoles, string[] currentRoles)
        {
            if (newRoles == null)
                newRoles = new string[] { };

            if (currentRoles == null)
                currentRoles = new string[] { };


            bool roleAdded = newRoles.Except(currentRoles).Any();
            bool roleRemoved = currentRoles.Except(newRoles).Any();

            return roleAdded || roleRemoved;
        }

        /// <summary>
        /// Verify if the <paramref name="contextUser"/> is in all added roles
        /// </summary>
        /// <param name="contextUser"></param>
        /// <param name="newRoles"></param>
        /// <param name="currentRoles"></param>
        /// <returns></returns>
        private bool IsUserInAllAddedRoles(ClaimsPrincipal contextUser, string[] newRoles, string[] currentRoles)
        {
            if (newRoles == null)
                newRoles = new string[] { };

            if (currentRoles == null)
                currentRoles = new string[] { };


            var addedRoles = newRoles.Except(currentRoles);

            return addedRoles.All(role => contextUser.IsInRole(role));
        }
    }
}
