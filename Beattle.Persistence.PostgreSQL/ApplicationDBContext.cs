using Beattle.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Persistence.PostgreSQL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private const string RENAMED_TABLE_ASPNETROLECLAIMS = "RoleClaim";
        private const string RENAMED_TABLE_ASPNETROLES = "Role";
        private const string RENAMED_TABLE_ASPNETUSERCLAIMS = "UserClaim";
        private const string RENAMED_TABLE_ASPNETUSERLOGINS = "UserLogin";
        private const string RENAMED_TABLE_ASPNETUSERROLES = "UserRole";
        private const string RENAMED_TABLE_ASPNETUSERTOKENS = "UserToken";
        private const string RENAMED_TABLE_ASPNETUSERS = "User"; 

        // Maybe i need to just use DbContextOptions options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Changing Default Identity Table Names
            builder.Entity<IdentityRoleClaim<string>>().ToTable(RENAMED_TABLE_ASPNETROLECLAIMS);
            builder.Entity<ApplicationRole>().ToTable(RENAMED_TABLE_ASPNETROLES);
            builder.Entity<IdentityUserClaim<string>>().ToTable(RENAMED_TABLE_ASPNETUSERCLAIMS);
            builder.Entity<IdentityUserLogin<string>>().ToTable(RENAMED_TABLE_ASPNETUSERLOGINS);
            builder.Entity<IdentityUserRole<string>>().ToTable(RENAMED_TABLE_ASPNETUSERROLES);
            builder.Entity<IdentityUserToken<string>>().ToTable(RENAMED_TABLE_ASPNETUSERTOKENS);
            builder.Entity<ApplicationUser>().ToTable(RENAMED_TABLE_ASPNETUSERS);
            #endregion

        }
    }
}
