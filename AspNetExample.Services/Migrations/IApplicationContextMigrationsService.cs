namespace AspNetExample.Services.Migrations;

public interface IApplicationContextMigrationsService
{
    Task ApplyMigrationsAsync();
}