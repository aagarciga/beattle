using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Beattle.Application.Interfaces;
using Beattle.Identity;
using Beattle.Infrastructure.Security;
using Beattle.Infrastructure.Security.Handlers;
using Beattle.Infrastructure.Security.Requirements;
using Beattle.Persistence.PostgreSQL;
using Beattle.SPAUI.ViewModels.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using System;

namespace Beattle.SPAUI
{
    public class Startup
    {
        private const string DEFAULT_CONNECTION = "DefaultConnection";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Alex: Adding default connection string using postgresql connector
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString(DEFAULT_CONNECTION));
                options.UseOpenIddict();
            });

            #region Security Configurations
            #region Identity Configuration
            // Alex: Set Identity Persistence through Entity Framework Core
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Alex: Configure Identity options
            services.Configure<IdentityOptions>(options =>
            {
                #region Identity User settings
                // Identity User Options Defaults
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                #endregion

                #region Identity Password settings
                // Identity Password Options Defaults
                options.Password.RequireDigit = Security.PasswordRequiredDigit;
                options.Password.RequiredLength = Security.PasswordRequiredLength;
                options.Password.RequireNonAlphanumeric = Security.PasswordRequireNonAlphanumeric;
                options.Password.RequireUppercase = Security.PasswordRequireUppercase;
                options.Password.RequireLowercase = Security.PasswordRequireLowercase;
                options.Password.RequiredUniqueChars = Security.PasswordRequiredUniqueChars;
                #endregion

                #region Identity Lockout settings
                // Identity Lockout Options Defaults
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                #endregion

                #region Identity Claims Types
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                //TODO: Alex i think is better to change subject for ClientId
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                #endregion
            });

            #endregion

            #region OpenIdDict
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options
                    .UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    options.UseMvc();
                    options.EnableTokenEndpoint("/connect/token");
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    options.AcceptAnonymousClients();
                    options.DisableHttpsRequirement(); // Note: Comment this out in production
                    options.RegisterScopes(
                        OpenIdConnectConstants.Scopes.OpenId,
                        OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Phone,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIdConnectConstants.Scopes.OfflineAccess,
                        OpenIddictConstants.Scopes.Roles
                        );
                    //options.UseRollingTokens();
                    //options.UseJsonWebTokens();
                }).AddValidation();
            #endregion

            #region Authorizations
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ViewUsers,
                    policy => policy.RequireClaim(
                        ApplicationClaimType.Authorization,
                        AuthorizationManager.ViewUsers));
                options.AddPolicy(Policies.ManageUsers,
                    policy => policy.RequireClaim(
                        ApplicationClaimType.Authorization,
                        AuthorizationManager.ManageUsers));

                options.AddPolicy(Policies.ViewRoles,
                    policy => policy.RequireClaim(
                        ApplicationClaimType.Authorization,
                        AuthorizationManager.ViewRoles));
                options.AddPolicy(Policies.ManageRoles,
                    policy => policy.RequireClaim(
                        ApplicationClaimType.Authorization,
                        AuthorizationManager.ManageRoles));
                options.AddPolicy(Policies.ViewRoleByRoleName,
                    policy => policy.Requirements.Add(new ViewRoleAuthorizationRequirement()));
                options.AddPolicy(Policies.AssignAllowedRoles,
                    policy => policy.Requirements.Add(new AssignRolesAuthorizationRequirement()));
            });
            #endregion

            #region Authorization Handlers
            services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ManageUserAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewRoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, AssignRolesAuthorizationHandler>();
            #endregion
            #endregion

            #region Services Configurations
            // Alex: services.Configure<Service>(Configuration.GetSection("ServiceConfigurationSectionInAppSettings.json"))
            #endregion

            #region Scoping Business Services
            // Alex: services.AddScoped<IService, Service>();
            #endregion

            #region Scoping Repositories
            services.AddScoped<IAccountManager, AccountManager>();
            #endregion

            Mapper.Initialize(Configuration =>
            {
                Configuration.AddProfile<AutoMapperProfile>();
            });


            #region Database Initialization (Seeding)
            // TODO: Implement DatabaseInitializer Class for Seeding
            //services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // As per the documentation https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/?view=aspnetcore-2.2, 
                    // the project is configured to start the front end in the background when ASP.NET Core starts in development mode. 
                    // This feature is designed with productivity in mind. However, when making frequent back end changes productivity 
                    // can suffer as it takes up to 10 seconds to launch the application after a back end change.

                    // For Production
                    //spa.UseAngularCliServer(npmScript: "start");

                    // For Development
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
