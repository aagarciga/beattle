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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Core;
using Serilog;
using Serilog.Sinks.Email;

namespace Beattle.SPAUI
{
    public class Program
    {

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
           .AddEnvironmentVariables()
           .Build();


        public static void Main(string[] args)
        {
            var smtpSettings = Configuration.GetSection("SmtpConfig");
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.File("Logs" + Path.DirectorySeparatorChar + "log.txt", rollingInterval: RollingInterval.Day)
               .WriteTo.Email(
                new EmailConnectionInfo
                {
                    FromEmail = smtpSettings["EmailAddress"],
                    ToEmail = "aagarciga@gmail.com",
                    MailServer = smtpSettings["Host"],
                    NetworkCredentials = new NetworkCredential
                    {
                        UserName = smtpSettings["Username"],
                        Password = smtpSettings["Password"]
                    },
                    EnableSsl = true,
                    Port = int.Parse(smtpSettings["Port"]),
                    EmailSubject = "Beattle Logging"
                },
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
                batchPostingLimit: 10
                , restrictedToMinimumLevel: LogEventLevel.Information
                )
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
