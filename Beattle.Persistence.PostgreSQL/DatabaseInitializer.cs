using Beattle.Application.Interfaces;
using Beattle.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beattle.Persistence.PostgreSQL
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IAccountManager accountManager;

        public DatabaseInitializer(ApplicationDbContext dbContext, IAccountManager accountManager)
        {
            applicationDbContext = dbContext;
            this.accountManager = accountManager;
        }

        public async Task SeedAsync(string[] adminRoleClaims)
        {
            await applicationDbContext.Database.MigrateAsync().ConfigureAwait(false);

            if (!await applicationDbContext.Users.AnyAsync())
            {
                // TODO: Log : "Generating inbuilt accounts"

                const string adminRoleName = "administrator";
                const string userRoleName = "user";

                await EnsureRoleAsync(adminRoleName, "Default administrator", adminRoleClaims);
                await EnsureRoleAsync(userRoleName, "Default user", new string[] { });

                await CreateUserAsync("administrator", "Administrator*78", "Inbuilt Administrator", "administrator@beattle.com", "+1 (123) 000-0000", new string[] { adminRoleName });
                await CreateUserAsync("user", "User*78", "Inbuilt Standard User", "user@beattle.com", "+1 (123) 000-0001", new string[] { userRoleName });

                // TODO: Log : "Inbuilt account generation completed"
            }

        }

        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await accountManager.GetRoleByNameAsync(roleName)) == null)
            {
                IApplicationRole applicationRole = new ApplicationRole(roleName, description);

                Tuple<bool, string[]> result = await accountManager.CreateRoleAsync(applicationRole, claims);

                if (!result.Item1)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");
            }
        }

        private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string fullName, string email, string phoneNumber, string[] roles)
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = userName,
                Name = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                IsEnabled = true
            };

            Tuple<bool, string[]> result = await accountManager.CreateUserAsync(applicationUser, roles, password);

            if (!result.Item1)
                throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");

            return applicationUser;
        }
    }
}
