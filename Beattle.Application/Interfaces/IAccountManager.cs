using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beattle.Application.Interfaces
{
    public interface IAccountManager
    {
        Task<Tuple<bool, string[]>> CreateUserAsync(IApplicationUser user, IEnumerable<string> roles, string password);
        Task<Tuple<bool, string[]>> UpdatePasswordAsync(IApplicationUser user, string currentPassword, string newPassword);
        Task<Tuple<bool, string[]>> UpdateRoleAsync(IApplicationRole role, IEnumerable<string> claims);
        Task<Tuple<bool, string[]>> UpdateUserAsync(IApplicationUser user);
        Task<Tuple<bool, string[]>> UpdateUserAsync(IApplicationUser user, IEnumerable<string> roles);
        Task<bool> TestCanDeleteUserAsync(string userId);
        Task<Tuple<bool, string[]>> DeleteUserAsync(IApplicationUser user);
        Task<Tuple<bool, string[]>> DeleteUserAsync(string userId);
        Task<bool> CheckPasswordAsync(IApplicationUser user, string password);
        Task<Tuple<bool, string[]>> ResetPasswordAsync(IApplicationUser user, string newPassword);
        Task<IApplicationUser> GetUserByEmailAsync(string email);
        Task<IApplicationUser> GetUserByIdAsync(string userId);
        Task<IApplicationUser> GetUserByUserNameAsync(string userName);
        Task<Tuple<IApplicationUser, string[]>> GetUserAndRolesAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(IApplicationUser user);
        Task<List<Tuple<IApplicationUser, string[]>>> GetUsersAndRolesAsync(int page, int pageSize);
        Task<Tuple<bool, string[]>> CreateRoleAsync(IApplicationRole role, IEnumerable<string> claims);
        Task<bool> TestCanDeleteRoleAsync(string roleId);
        Task<Tuple<bool, string[]>> DeleteRoleAsync(IApplicationRole role);
        Task<Tuple<bool, string[]>> DeleteRoleAsync(string roleName);
        Task<IApplicationRole> GetRoleByIdAsync(string roleId);
        Task<IApplicationRole> GetRoleByNameAsync(string roleName);
        Task<IApplicationRole> GetRoleLoadRelatedAsync(string roleName);
        Task<List<IApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize);
    }
}
