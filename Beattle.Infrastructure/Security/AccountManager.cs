using AspNet.Security.OpenIdConnect.Primitives;
using Beattle.Application.Interfaces;
using Beattle.Identity;
using Beattle.Persistence.PostgreSQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Beattle.Infrastructure.Security
{
    public class AccountManager : IAccountManager
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        public AccountManager(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IHttpContextAccessor httpContextAccessor)
        {
            
            this.dbContext = dbContext;
            //TODO: refactor the way of getting the CurrentUserId
            this.dbContext.CurrentUserId = httpContextAccessor.HttpContext?.User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value?.Trim();
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<bool> CheckPasswordAsync(IApplicationUser user, string password)
        {
            if (!await userManager.CheckPasswordAsync(user as ApplicationUser, password))
            {

                if (! userManager.SupportsUserLockout)
                {
                    // Alex: Increments the access failed count for the user as an asynchronous operation.
                    //  If the failed access account is greater than or equal to the configured maximum
                    //  number of attempts, the user will be locked out for the configured lockout time
                    //  span.
                    await userManager.AccessFailedAsync(user as ApplicationUser);
                }
                return false;
            }
            return true;
        }

        public async Task<Tuple<bool, string[]>> CreateRoleAsync(IApplicationRole role, IEnumerable<string> claims)
        {
            if (claims == null)
            {
                claims = new string[] { };
            }                

            string[] invalidClaims = claims.Where(claim => AuthorizationManager.GetByValue(claim) == null).ToArray();
            if (invalidClaims.Any())
                return Tuple.Create(false, new[] { "The following claim types are invalid: " + string.Join(", ", invalidClaims) });


            var result = await roleManager.CreateAsync(role as ApplicationRole);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(error => error.Description).ToArray());


            role = await roleManager.FindByNameAsync(role.Name);

            foreach (string claim in claims.Distinct())
            {
                result = await roleManager.AddClaimAsync(role as ApplicationRole, new Claim(ApplicationClaimType.Authorization, AuthorizationManager.GetByValue(claim)));

                if (!result.Succeeded)
                {
                    await DeleteRoleAsync(role);
                    return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }
            }

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> CreateUserAsync(IApplicationUser user, IEnumerable<string> roles, string password)
        {
            var result = await userManager.CreateAsync(user as ApplicationUser, password);
            if (!result.Succeeded)
            {
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
            }

            user = await userManager.FindByNameAsync(user.Name);

            try
            {
                result = await userManager.AddToRolesAsync(user as ApplicationUser, roles.Distinct());
            }
            catch
            {
                await DeleteUserAsync(user);
                throw;
            }

            if (!result.Succeeded)
            {
                await DeleteUserAsync(user);
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
            }

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> DeleteRoleAsync(IApplicationRole role)
        {
            var result = await roleManager.DeleteAsync(role as ApplicationRole);
            return Tuple.Create(result.Succeeded, result.Errors.Select(error => error.Description).ToArray());
        }

        public async Task<Tuple<bool, string[]>> DeleteRoleAsync(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role != null)
            {
                return await DeleteRoleAsync(role);
            }               

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> DeleteUserAsync(IApplicationUser user)
        {
            var result = await userManager.DeleteAsync(user as ApplicationUser);
            return Tuple.Create(result.Succeeded, result.Errors.Select(error => error.Description).ToArray());
        }

        public async Task<Tuple<bool, string[]>> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                return await DeleteUserAsync(user);
            }              

            return Tuple.Create(true, new string[] { });
        }

        public async Task<IApplicationRole> GetRoleByIdAsync(string roleId)
        {
            return await roleManager.FindByIdAsync(roleId);
        }

        public async Task<IApplicationRole> GetRoleByNameAsync(string roleName)
        {
            return await roleManager.FindByNameAsync(roleName);
        }

        public async Task<IApplicationRole> GetRoleLoadRelatedAsync(string roleName)
        {
            var role = await dbContext.Roles
                .Include(r => r.Claims)
                .Include(r => r.Users)
                .Where(r => r.Name == roleName)
                .FirstOrDefaultAsync();

            return role;
        }

        public async Task<List<IApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize)
        {
            IQueryable<IApplicationRole> rolesQuery = dbContext.Roles
                .Include(r => r.Claims)
                .Include(r => r.Users)
                .OrderBy(r => r.Name);

            if (page != -1)
            {
                rolesQuery = rolesQuery.Skip((page - 1) * pageSize);
            }


            if (pageSize != -1)
            {
                rolesQuery = rolesQuery.Take(pageSize);
            }


            var roles = await rolesQuery.ToListAsync();

            return roles;
        }

        public async Task<Tuple<IApplicationUser, string[]>> GetUserAndRolesAsync(string userId)
        {
            IApplicationUser user = await dbContext.Users
                .Include(u => u.Roles)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            List<string> userRoleIds = user.Roles.Select(r => r.RoleId).ToList();

            string[] roles = await dbContext.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToArrayAsync();

            return Tuple.Create(user, roles);
        }

        public async Task<IApplicationUser> GetUserByEmailAsync(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<IApplicationUser> GetUserByIdAsync(string userId)
        {
            return await userManager.FindByIdAsync(userId);
        }

        public async Task<IApplicationUser> GetUserByUserNameAsync(string userName)
        {
            return await userManager.FindByNameAsync(userName);
        }

        public async Task<IList<string>> GetUserRolesAsync(IApplicationUser user)
        {
            return await userManager.GetRolesAsync(user as ApplicationUser);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"> -1 for all pages</param>
        /// <param name="pageSize"> -1 for all</param>
        /// <returns></returns>
        public async Task<List<Tuple<IApplicationUser, string[]>>> GetUsersAndRolesAsync(int page, int pageSize)
        {
            IQueryable<IApplicationUser> usersQuery = dbContext.Users
                .Include(u => u.Roles)
                .OrderBy(u => u.UserName);

            if (page != -1)
                usersQuery = usersQuery.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                usersQuery = usersQuery.Take(pageSize);

            var users = await usersQuery.ToListAsync();

            var userRoleIds = users.SelectMany(u => u.Roles.Select(r => r.RoleId)).ToList();

            var roles = await dbContext.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .ToArrayAsync();

            return users.Select(u => Tuple.Create(u,
                roles.Where(r => u.Roles.Select(ur => ur.RoleId).Contains(r.Id)).Select(r => r.Name).ToArray()))
                .ToList();
        }

        public async Task<Tuple<bool, string[]>> ResetPasswordAsync(IApplicationUser user, string newPassword)
        {
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user as ApplicationUser);

            var result = await userManager.ResetPasswordAsync(user as ApplicationUser, resetToken, newPassword);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());

            return Tuple.Create(true, new string[] { });
        }

        public async Task<bool> TestCanDeleteRoleAsync(string roleId)
        {
            return !await dbContext.UserRoles.Where(r => r.RoleId == roleId).AnyAsync();
        }

        public async Task<bool> TestCanDeleteUserAsync(string userId)
        {
            // TODO: Check for related user data. Below example with User Orders
            //if (await dbContext.Orders.Where(o => o.CashierId == userId).AnyAsync())
                //return false;

            //canDelete = !await ; //Do other tests...

            return true;
        }

        public async Task<Tuple<bool, string[]>> UpdatePasswordAsync(IApplicationUser user, string currentPassword, string newPassword)
        {
            var result = await userManager.ChangePasswordAsync(user as ApplicationUser, currentPassword, newPassword);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> UpdateRoleAsync(IApplicationRole role, IEnumerable<string> claims)
        {
            if (claims != null)
            {
                string[] invalidClaims = claims.Where(claim => AuthorizationManager.GetByValue(claim) == null).ToArray();
                if (invalidClaims.Any())
                    return Tuple.Create(false, new[] { "The following claim types are invalid: " + string.Join(", ", invalidClaims) });
            }

            var result = await roleManager.UpdateAsync(role as ApplicationRole);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            if (claims != null)
            {
                var roleClaims = (await roleManager.GetClaimsAsync(role as ApplicationRole)).Where(c => c.Type == ApplicationClaimType.Authorization);
                var roleClaimValues = roleClaims.Select(c => c.Value).ToArray();

                var claimsToRemove = roleClaimValues.Except(claims).ToArray();
                var claimsToAdd = claims.Except(roleClaimValues).Distinct().ToArray();

                if (claimsToRemove.Any())
                {
                    foreach (string claim in claimsToRemove)
                    {
                        result = await roleManager.RemoveClaimAsync(role as ApplicationRole, roleClaims.Where(c => c.Value == claim).FirstOrDefault());
                        if (!result.Succeeded)
                            return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                    }
                }

                if (claimsToAdd.Any())
                {
                    foreach (string claim in claimsToAdd)
                    {
                        result = await roleManager.AddClaimAsync(role as ApplicationRole, new Claim(ApplicationClaimType.Authorization, AuthorizationManager.GetByValue(claim)));
                        if (!result.Succeeded)
                            return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                    }
                }
            }

            return Tuple.Create(true, new string[] { });
        }

        public async Task<Tuple<bool, string[]>> UpdateUserAsync(IApplicationUser user)
        {
            return await UpdateUserAsync(user, null);
        }

        public async Task<Tuple<bool, string[]>> UpdateUserAsync(IApplicationUser user, IEnumerable<string> roles)
        {
            var result = await userManager.UpdateAsync(user as ApplicationUser);
            if (!result.Succeeded)
                return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());


            if (roles != null)
            {
                var userRoles = await userManager.GetRolesAsync(user as ApplicationUser);

                var rolesToRemove = userRoles.Except(roles).ToArray();
                var rolesToAdd = roles.Except(userRoles).Distinct().ToArray();

                if (rolesToRemove.Any())
                {
                    result = await userManager.RemoveFromRolesAsync(user as ApplicationUser, rolesToRemove);
                    if (!result.Succeeded)
                        return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }

                if (rolesToAdd.Any())
                {
                    result = await userManager.AddToRolesAsync(user as ApplicationUser, rolesToAdd);
                    if (!result.Succeeded)
                        return Tuple.Create(false, result.Errors.Select(e => e.Description).ToArray());
                }
            }

            return Tuple.Create(true, new string[] { });
        }
    }
}
