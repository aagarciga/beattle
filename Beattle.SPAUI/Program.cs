using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beattle.Application.Interfaces;
using Beattle.Infrastructure.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Beattle.SPAUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();

            // Alex: Using Dependency Injection for scoping
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                try
                {
                    IDatabaseInitializer databaseInitializer = 
                        services.GetRequiredService<IDatabaseInitializer>();
                    databaseInitializer.SeedAsync(AuthorizationManager.GetAllAuthorizationValues()).Wait();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
