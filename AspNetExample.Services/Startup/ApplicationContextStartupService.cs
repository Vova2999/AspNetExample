using AspNetExample.Common.Extensions;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Services.Startup;

public class ApplicationContextStartupService : IApplicationContextStartupService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<ApplicationContextStartupService> _logger;
    private readonly string? _initializeUserLogin;
    private readonly string? _initializeUserPassword;

    public ApplicationContextStartupService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<ApplicationContextStartupService> logger,
        string? initializeUserLogin,
        string? initializeUserPassword)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
        var roles = await _roleManager.Roles.ToArrayAsync();
        var normalizeAdminRole = _roleManager.NormalizeKey(RoleTokens.AdminRole);
        var normalizeSwaggerRole = _roleManager.NormalizeKey(RoleTokens.SwaggerRole);

        var hasAdminRole = roles.Any(role => role.NormalizedName == normalizeAdminRole);
        var hasSwaggerRole = roles.Any(role => role.NormalizedName == normalizeSwaggerRole);

        if (!hasAdminRole)
            await _roleManager.CreateAsync(new Role { Id = Guid.NewGuid(), Name = RoleTokens.AdminRole });
        if (!hasSwaggerRole)
            await _roleManager.CreateAsync(new Role { Id = Guid.NewGuid(), Name = RoleTokens.SwaggerRole });
    }

    private async Task InitializeUsersAsync()
    {
        if (_initializeUserLogin.IsNullOrEmpty() || _initializeUserPassword.IsNullOrEmpty())
            return;

        var hasUsers = await _userManager.Users.AnyAsync();
        if (hasUsers)
            return;

        var user = new User { Id = Guid.NewGuid(), Name = _initializeUserLogin };
        await _userManager.CreateAsync(user, _initializeUserPassword);
        await _userManager.AddToRolesAsync(user, [RoleTokens.AdminRole, RoleTokens.SwaggerRole]);
    }
}