using Beattle.Application.Interfaces;
using Beattle.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Persistence.PostgreSQL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
    {
        private const string RENAMED_TABLE_ASPNETROLECLAIMS = "RoleClaim";
        private const string RENAMED_TABLE_ASPNETROLES = "Role";
        private const string RENAMED_TABLE_ASPNETUSERCLAIMS = "UserClaim";
        private const string RENAMED_TABLE_ASPNETUSERLOGINS = "UserLogin";
        private const string RENAMED_TABLE_ASPNETUSERROLES = "UserRole";
        private const string RENAMED_TABLE_ASPNETUSERTOKENS = "UserToken";
        private const string RENAMED_TABLE_ASPNETUSERS = "User";

        public string CurrentUserId { get; set; }

        // Maybe i need to just use DbContextOptions options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {}

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

            #region Setting Relationship between Users, Roles and Claims
            builder.Entity<ApplicationUser>()
                .HasMany(user => user.Claims)
                .WithOne()
                .HasForeignKey(claim => claim.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationUser>()
                .HasMany(user => user.Roles)
                .WithOne()
                .HasForeignKey(role => role.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationRole>()
                .HasMany(role => role.Claims)
                .WithOne()
                .HasForeignKey(claim => claim.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationRole>()
                .HasMany(role => role.Users)
                .WithOne()
                .HasForeignKey(user => user.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

        }
    }
}
