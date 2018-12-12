using AutoMapper;
using Beattle.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Beattle.SPAUI
{
    /// <summary>
    /// Our DbContext is in a separate class library project (Beattle.Persistence.PostgreSQL). 
    /// So in order to add new migrations and update our database we need to implement 
    /// the IDesignTimeDbContextFactory in our startup project (or main project)
    /// 
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            Mapper.Reset();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseNpgsql(configuration["ConnectionStrings:DefaultConnection"], b => b.MigrationsAssembly("Beattle.Persistence.PostgreSQL"));
            //builder.UseOpenIddict();

            return new ApplicationDbContext(builder.Options);
        }
    }
}
