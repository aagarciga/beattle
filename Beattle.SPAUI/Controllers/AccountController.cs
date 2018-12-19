using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Beattle.Application.Interfaces;
using Beattle.Identity;
using Beattle.Infrastructure.Security;
using Beattle.SPAUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation;

namespace Beattle.SPAUI.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    //[ApiController]
    public class AccountController : Controller
    {
        private const string GetUserByIdActionName = "GetUserById";
        private const string GetRoleByIdActionName = "GetRoleById";

        private readonly IAccountManager accountManager;
        private readonly IAuthorizationService authorizationService;

        public AccountController( IAccountManager accountManager, IAuthorizationService authorizationService)
        {
            this.accountManager = accountManager;
            this.authorizationService = authorizationService;
        }

        [HttpGet("users/me")]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await GetUserByUserName(User.Identity.Name);
        }

        [HttpGet("users/username/{userName}")]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            IApplicationUser applicationUser = await accountManager.GetUserByUserNameAsync(userName);
            if (!(await authorizationService.AuthorizeAsync(User, applicationUser?.Id ?? "", AccountManagementOperations.Read)).Succeeded)
                return new ChallengeResult();
            if (applicationUser == null)
                return NotFound(userName);
            return await GetUserById(applicationUser.Id);
        }

        [HttpGet("users/{id}", Name = GetUserByIdActionName)]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!(await authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Read)).Succeeded)
                return new ChallengeResult();

            UserViewModel userViewModel = await FetchUserViewModel(id);

            if (userViewModel != null)
                return Ok(userViewModel);
            return NotFound(id);
        }

        [HttpGet("users")]
        [Authorize(Policies.ViewUsers)]
        [ProducesResponseType(200, Type = typeof(List<UserViewModel>))]
        public async Task<IActionResult> GetUsers()
        {
            return await GetUsers(-1, -1);
        }

        [HttpGet("users/{pageNumber:int}/{pageSize:int}")]
        [Authorize(Policies.ViewUsers)]
        [ProducesResponseType(200, Type = typeof(List<UserViewModel>))]
        public async Task<IActionResult> GetUsers(int pageNumber, int pageSize)
        {
            List<Tuple<IApplicationUser, string[]>> usersAndRoles = await accountManager.GetUsersAndRolesAsync(pageNumber, pageSize);

            List<UserViewModel> userViewModels = new List<UserViewModel>();

            foreach (var item in usersAndRoles)
            {
                UserViewModel userViewModel = Mapper.Map<UserViewModel>(item.Item1);
                userViewModel.Roles = item.Item2;

                userViewModels.Add(userViewModel);
            }

            return Ok(userViewModels);
        }

        [HttpPut("users/me")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditViewModel userEditViewModel)
        {
            return await UpdateUser(Security.GetUserId(User), userEditViewModel);
        }

        [HttpPut("users/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserEditViewModel userEditViewModel)
        {
            IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(id);
            string[] currentRoles = applicationUser != null ? (await accountManager.GetUserRolesAsync(applicationUser)).ToArray() : null;

            Task<AuthorizationResult> manageUsersPolicy = authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Update);
            Task<AuthorizationResult> assignRolePolicy = authorizationService.AuthorizeAsync(User, Tuple.Create(userEditViewModel.Roles, currentRoles), Policies.AssignAllowedRoles);
            
            if ((await Task.WhenAll(manageUsersPolicy, assignRolePolicy)).Any(result => !result.Succeeded))
                return new ChallengeResult();
            
            if (ModelState.IsValid)
            {
                if (userEditViewModel == null)
                    return BadRequest($"{nameof(userEditViewModel)} cannot be null");

                if (!string.IsNullOrWhiteSpace(userEditViewModel.Id) && id != userEditViewModel.Id)
                    return BadRequest("Conflicting user id in parameter and model data");

                if (applicationUser == null)
                    return NotFound(id);

                if (Security.GetUserId(User) == id && string.IsNullOrWhiteSpace(userEditViewModel.CurrentPassword))
                {
                    if (!string.IsNullOrWhiteSpace(userEditViewModel.NewPassword))
                        return BadRequest("Current password is required when changing your own password");

                    if (applicationUser.UserName != userEditViewModel.UserName)
                        return BadRequest("Current password is required when changing your own username");
                }

                bool isValid = true;

                if (Security.GetUserId(User) == id && (applicationUser.UserName != userEditViewModel.UserName || !string.IsNullOrWhiteSpace(userEditViewModel.NewPassword)))
                {
                    if (!await accountManager.CheckPasswordAsync(applicationUser, userEditViewModel.CurrentPassword))
                    {
                        isValid = false;
                        AddErrors(new string[] { "The username/password couple is invalid." });
                    }
                }

                if (isValid)
                {
                    Mapper.Map<UserViewModel, ApplicationUser>(userEditViewModel, applicationUser as ApplicationUser);

                    var result = await accountManager.UpdateUserAsync(applicationUser, userEditViewModel.Roles);
                    if (result.Item1)
                    {
                        if (!string.IsNullOrWhiteSpace(userEditViewModel.NewPassword))
                        {
                            if (!string.IsNullOrWhiteSpace(userEditViewModel.CurrentPassword))
                                result = await accountManager.UpdatePasswordAsync(applicationUser, userEditViewModel.CurrentPassword, userEditViewModel.NewPassword);
                            else
                                result = await accountManager.ResetPasswordAsync(applicationUser, userEditViewModel.NewPassword);
                        }

                        if (result.Item1)
                            return NoContent();
                    }

                    AddErrors(result.Item2);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPatch("users/me")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] JsonPatchDocument<UserPatchViewModel> patch)
        {
            return await UpdateUser(Security.GetUserId(this.User), patch);
        }

        [HttpPatch("users/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] JsonPatchDocument<UserPatchViewModel> patch)
        {
            if (!(await authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Update)).Succeeded)
                return new ChallengeResult();

            if (ModelState.IsValid)
            {
                if (patch == null)
                    return BadRequest($"{nameof(patch)} cannot be null");

                IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(id);

                if (applicationUser == null)
                    return NotFound(id);

                UserPatchViewModel userPatchViewModel = Mapper.Map<UserPatchViewModel>(applicationUser);
                patch.ApplyTo(userPatchViewModel, ModelState);

                if (ModelState.IsValid)
                {
                    Mapper.Map(userPatchViewModel, applicationUser as ApplicationUser);

                    var result = await accountManager.UpdateUserAsync(applicationUser);
                    if (result.Item1)
                        return NoContent();

                    AddErrors(result.Item2);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("users")]
        [Authorize(Policies.ManageUsers)]
        [ProducesResponseType(201, Type = typeof(UserViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Register([FromBody] UserEditViewModel userEditViewModel)
        {
            if (!(await authorizationService.AuthorizeAsync(User, Tuple.Create(userEditViewModel.Roles, new string[] { }), Policies.AssignAllowedRoles)).Succeeded)
                return new ChallengeResult();

            if (ModelState.IsValid)
            {
                if (userEditViewModel == null)
                    return BadRequest($"{nameof(userEditViewModel)} cannot be null");

                IApplicationUser applicationUser = Mapper.Map<ApplicationUser>(userEditViewModel);

                var result = await accountManager.CreateUserAsync(applicationUser, userEditViewModel.Roles, userEditViewModel.NewPassword);
                if (result.Item1)
                {
                    UserViewModel userViewModel = await FetchUserViewModel(applicationUser.Id);
                    return CreatedAtAction(GetUserByIdActionName, new { id = userViewModel.Id }, userViewModel);
                }

                AddErrors(result.Item2);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("users/{id}")]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!(await authorizationService.AuthorizeAsync(User, id, AccountManagementOperations.Delete)).Succeeded)
                return new ChallengeResult();

            if (!await accountManager.TestCanDeleteUserAsync(id))
                return BadRequest("User cannot be deleted. Delete all orders associated with this user and try again");

            UserViewModel userViewModel = null;
            IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(id);

            if (applicationUser != null)
                userViewModel = await FetchUserViewModel(applicationUser.Id);

            if (userViewModel == null)
                return NotFound(id);

            var result = await accountManager.DeleteUserAsync(applicationUser);
            if (!result.Item1)
                throw new Exception("The following errors occurred whilst deleting user: " + string.Join(", ", result.Item2));
            
            return Ok(userViewModel);
        }

        [HttpPut("users/unblock/{id}")]
        [Authorize(Policies.ManageUsers)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnblockUser(string id)
        {
            IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(id);

            if (applicationUser == null)
                return NotFound(id);

            (applicationUser as ApplicationUser).LockoutEnd = null;
            var result = await accountManager.UpdateUserAsync(applicationUser);
            if (!result.Item1)
                throw new Exception("The following errors occurred whilst unblocking user: " + string.Join(", ", result.Item2));
            
            return NoContent();
        }

        [HttpGet("users/me/preferences")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UserPreferences()
        {
            var userId = Security.GetUserId(User);
            IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(userId);

            if (applicationUser != null)
                return Ok(applicationUser.Configuration);
            else
                return NotFound(userId);
        }

        [HttpPut("users/me/preferences")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UserPreferences([FromBody] string data)
        {
            string userId = Security.GetUserId(User);
            IApplicationUser applicationUser = await accountManager.GetUserByIdAsync(userId);

            if (applicationUser == null)
                return NotFound(userId);

            applicationUser.Configuration = data;
            var result = await accountManager.UpdateUserAsync(applicationUser);
            if (!result.Item1)
                throw new Exception("The following errors occurred whilst updating User Configurations: " + string.Join(", ", result.Item2));
            
            return NoContent();
        }

        [HttpGet("roles/name/{name}")]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            if (!(await authorizationService.AuthorizeAsync(User, name, Policies.ViewRoleByRoleName)).Succeeded)
                return new ChallengeResult();

            RoleViewModel roleViewModel = await FetchRoleViewModel(name);

            if (roleViewModel == null)
                return NotFound(name);
            return Ok(roleViewModel);
        }

        [HttpGet("roles/{id}", Name = GetRoleByIdActionName)]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var appRole = await accountManager.GetRoleByIdAsync(id);

            if (!(await authorizationService.AuthorizeAsync(this.User, appRole?.Name ?? "", Policies.ViewRoleByRoleName)).Succeeded)
                return new ChallengeResult();

            if (appRole == null)
                return NotFound(id);

            return await GetRoleByName(appRole.Name);
        }

        [HttpGet("roles")]
        [Authorize(Policies.ViewRoles)]
        [ProducesResponseType(200, Type = typeof(List<RoleViewModel>))]
        public async Task<IActionResult> GetRoles()
        {
            return await GetRoles(-1, -1);
        }

        [HttpGet("roles/{pageNumber:int}/{pageSize:int}")]
        [Authorize(Policies.ViewRoles)]
        [ProducesResponseType(200, Type = typeof(List<RoleViewModel>))]
        public async Task<IActionResult> GetRoles(int pageNumber, int pageSize)
        {
            List<IApplicationRole> roles = await accountManager.GetRolesLoadRelatedAsync(pageNumber, pageSize);
            return Ok(Mapper.Map<List<RoleViewModel>>(roles));
        }

        [HttpPut("roles/{id}")]
        [Authorize(Policies.ManageRoles)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleViewModel role)
        {
            if (ModelState.IsValid)
            {
                if (role == null)
                    return BadRequest($"{nameof(role)} cannot be null");

                if (!string.IsNullOrWhiteSpace(role.Id) && id != role.Id)
                    return BadRequest("Conflicting role id in parameter and model data");

                IApplicationRole applicationRole = await accountManager.GetRoleByIdAsync(id);

                if (applicationRole == null)
                    return NotFound(id);

                Mapper.Map(role, applicationRole as ApplicationRole);

                Tuple<bool, string[]> result = await accountManager.UpdateRoleAsync(applicationRole, role.Permissions?.Select(p => p.Value).ToArray());
                if (result.Item1)
                    return NoContent();

                AddErrors(result.Item2);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("roles")]
        [Authorize(Policies.ManageRoles)]
        [ProducesResponseType(201, Type = typeof(RoleViewModel))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateRole([FromBody] RoleViewModel role)
        {
            if (ModelState.IsValid)
            {
                if (role == null)
                    return BadRequest($"{nameof(role)} cannot be null");

                IApplicationRole applicationRole = Mapper.Map<ApplicationRole>(role);

                Tuple<bool, string[]> result = await accountManager.CreateRoleAsync(applicationRole, role.Permissions?.Select(p => p.Value).ToArray());
                if (result.Item1)
                {
                    RoleViewModel roleViewModel = await FetchRoleViewModel(applicationRole.Name);
                    return CreatedAtAction(GetRoleByIdActionName, new { id = roleViewModel.Id }, roleViewModel);
                }

                AddErrors(result.Item2);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("roles/{id}")]
        [Authorize(Policies.ManageRoles)]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (!await accountManager.TestCanDeleteRoleAsync(id))
                return BadRequest("Role cannot be deleted. Remove all users from this role and try again");

            RoleViewModel roleViewModel = null;
            IApplicationRole applicationRole = await accountManager.GetRoleByIdAsync(id);

            if (applicationRole != null)
                roleViewModel = await FetchRoleViewModel(applicationRole.Name);

            if (roleViewModel == null)
                return NotFound(id);

            Tuple<bool, string[]> result = await accountManager.DeleteRoleAsync(applicationRole);
            if (!result.Item1)
                throw new Exception("The following errors occurred whilst deleting role: " + string.Join(", ", result.Item2));

            return Ok(roleViewModel);
        }

        [HttpGet("permissions")]
        [Authorize(Policies.ViewRoles)]
        [ProducesResponseType(200, Type = typeof(List<AuthorizationViewModel>))]
        public IActionResult GetAllAuthorizations()
        {
            //TODO: Change permissions for authorizations
            return Ok(Mapper.Map<List<AuthorizationViewModel>>(AuthorizationManager.Authorizations));
        }

        private async Task<UserViewModel> FetchUserViewModel(string userId)
        {
            var userAndRoles = await accountManager.GetUserAndRolesAsync(userId);
            if (userAndRoles == null)
                return null;

            UserViewModel userViewModel = Mapper.Map<UserViewModel>(userAndRoles.Item1);
            userViewModel.Roles = userAndRoles.Item2;

            return userViewModel;
        }
        private async Task<RoleViewModel> FetchRoleViewModel(string roleName)
        {
            IApplicationRole role = await accountManager.GetRoleLoadRelatedAsync(roleName);
            if (role != null)
                return Mapper.Map<RoleViewModel>(role);
            return null;
        }
        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }
}