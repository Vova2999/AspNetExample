using AspNetExample.Database.Context.Factory;
using AspNetExample.Database.Helpers;
using AspNetExample.Domain.Entities;
using AspNetExample.Services.Managers;
using AspNetExample.Services.Startup;
using AspNetExample.Services.Stores;
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

        service.AddScoped<ApplicationContextUserManager>();
        service.AddScoped<ApplicationContextRoleManager>();
        service.AddScoped<ApplicationContextSignInManager>();

        service.AddScoped<UserManager<User>, ApplicationContextUserManager>();
        service.AddScoped<RoleManager<Role>, ApplicationContextRoleManager>();
        service.AddScoped<SignInManager<User>, ApplicationContextSignInManager>();

        service.AddScoped<IApplicationContextUserStore, ApplicationContextUserStore>();
        service.AddScoped<IApplicationContextRoleStore, ApplicationContextRoleStore>();

        service.AddIdentity<User, Role>()
            .AddUserStore<ApplicationContextUserStore>()
            .AddRoleStore<ApplicationContextRoleStore>();

        service.AddScoped<IApplicationContextStartupService>(serviceProvider =>
            new ApplicationContextStartupService(
                serviceProvider.GetRequiredService<ApplicationContextUserManager>(),
                serviceProvider.GetRequiredService<ApplicationContextRoleManager>(),
                serviceProvider.GetRequiredService<ILogger<ApplicationContextStartupService>>(),
                initializeUserLogin,
                initializeUserPassword));
    }
}