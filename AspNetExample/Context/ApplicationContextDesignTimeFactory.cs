using AspNetExample.Database.Context;
using AspNetExample.Database.Helpers;
using Microsoft.EntityFrameworkCore.Design;

namespace AspNetExample.Context;

public class ApplicationContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var options = ApplicationContextHelper.BuildOptions(connectionString);

        return new ApplicationContext(options);
    }
}