using AspNetExample.Database.Context.Factory;
using AspNetExample.Database.Helpers;
using AspNetExample.Domain.Entities;
using AspNetExample.Services.Startup;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample;

public static class AspNetExampleModule
{
    public static void RegisterDependencies(IServiceCollection service, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var initializeUserLogin = configuration["InitializeUser:Login"];
        var initializeUserPassword = configuration["InitializeUser:Password"];

        service.AddSingleton<IApplicationContextFactory, ApplicationContextFactory>();
        service.AddSingleton(_ => ApplicationContextHelper.BuildOptions(connectionString));

        service.AddScoped<IApplicationContextStartupService>(serviceProvider =>
            new ApplicationContextStartupService(
                serviceProvider.GetRequiredService<UserManager<User>>(),
                serviceProvider.GetRequiredService<RoleManager<Role>>(),
                serviceProvider.GetRequiredService<ILogger<ApplicationContextStartupService>>(),
                initializeUserLogin,
                initializeUserPassword));
    }
}