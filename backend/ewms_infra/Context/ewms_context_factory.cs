using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ewms_infra.Context
{
    public class ewms_context_factory : IDesignTimeDbContextFactory<ewms_context>
    {
        public ewms_context CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../ewms_api");
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=ewms_db;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<ewms_context>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ewms_context(optionsBuilder.Options);
        }
    }
}
