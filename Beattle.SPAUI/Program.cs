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
using Serilog;
using Serilog.Events;
using System.Net;

namespace Beattle.SPAUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NetworkCredential networkCredential = new NetworkCredential("alex.alvarez@turmundo.com", "Dandelion*78");
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.File("Logs" + Path.DirectorySeparatorChar + "log.txt", rollingInterval: RollingInterval.Day)
               .WriteTo.Email(
                fromEmail: "alex.alvarez@turmundo.com",
                toEmail: "aagarciga@gmail.com",
                mailServer: "smtp.google.com",
                mailSubject: "Beattle Logging",                
                networkCredential: networkCredential)
               .CreateLogger();

            try
            {
                Log.Information("Starting web host");

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
                        Log.Fatal(e, "Error whilst creating and seeding database");
                    }
                }

                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
