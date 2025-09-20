using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IT_System.Data
{
    public class ItDbContextFactory : IDesignTimeDbContextFactory<ItDbContext>
    {
        public ItDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("Default")
                     ?? "Server=(localdb)\\MSSQLLocalDB;Database=AutoIntegration;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<ItDbContext>()
                .UseSqlServer(cs)
                .Options;

            return new ItDbContext(options);
        }
    }
}