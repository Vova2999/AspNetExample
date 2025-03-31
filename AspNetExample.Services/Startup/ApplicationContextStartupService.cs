using AspNetExample.Common.Extensions;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Services.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Services.Startup;

public class ApplicationContextStartupService : IApplicationContextStartupService
{
    private readonly ApplicationContextUserManager _applicationContextUserManager;
    private readonly ApplicationContextRoleManager _applicationContextRoleManager;
    private readonly ILogger<ApplicationContextStartupService> _logger;
    private readonly string? _initializeUserLogin;
    private readonly string? _initializeUserPassword;

    public ApplicationContextStartupService(
        ApplicationContextUserManager applicationContextUserManager,
        ApplicationContextRoleManager applicationContextRoleManager,
        ILogger<ApplicationContextStartupService> logger,
        string? initializeUserLogin,
        string? initializeUserPassword)
    {
        _applicationContextUserManager = applicationContextUserManager;
        _applicationContextRoleManager = applicationContextRoleManager;
        _logger = logger;
        _initializeUserLogin = initializeUserLogin;
        _initializeUserPassword = initializeUserPassword;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await InitializeRolesAsync();
            await InitializeUsersAsync();
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Error on initialize");
        }
    }

    private async Task InitializeRolesAsync()
    {
        var roles = await _applicationContextRoleManager.Roles.ToArrayAsync();
        var normalizeAdminRole = _applicationContextRoleManager.NormalizeKey(RoleTokens.AdminRole);
        var normalizeSwaggerRole = _applicationContextRoleManager.NormalizeKey(RoleTokens.SwaggerRole);

        var hasAdminRole = roles.Any(role => role.NormalizedName == normalizeAdminRole);
        var hasSwaggerRole = roles.Any(role => role.NormalizedName == normalizeSwaggerRole);

        if (!hasAdminRole)
            await _applicationContextRoleManager.CreateAsync(new Role { Id = Guid.NewGuid(), Name = RoleTokens.AdminRole });
        if (!hasSwaggerRole)
            await _applicationContextRoleManager.CreateAsync(new Role { Id = Guid.NewGuid(), Name = RoleTokens.SwaggerRole });
    }

    private async Task InitializeUsersAsync()
    {
        if (_initializeUserLogin.IsNullOrEmpty() || _initializeUserPassword.IsNullOrEmpty())
            return;

        var hasUsers = await _applicationContextUserManager.Users.AnyAsync();
        if (hasUsers)
            return;

        var user = new User { Id = Guid.NewGuid(), Name = _initializeUserLogin };
        await _applicationContextUserManager.CreateAsync(user, _initializeUserPassword);
        await _applicationContextUserManager.AddToRolesAsync(user, [RoleTokens.AdminRole, RoleTokens.SwaggerRole]);
    }
}