namespace AspNetExample.Services.Startup;

public interface IApplicationContextStartupService
{
    Task ApplyMigrationsAsync();
    Task InitializeUsersAndRoles();
}