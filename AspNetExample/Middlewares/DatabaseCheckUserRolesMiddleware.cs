using AspNetExample.Common.Extensions;
using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Middlewares;

public class DatabaseCheckUserRolesMiddleware : IMiddleware
{
    private readonly ILogger<DatabaseCheckUserRolesMiddleware> _logger;

    public DatabaseCheckUserRolesMiddleware(
        ILogger<DatabaseCheckUserRolesMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (await CheckUserRolesAsync(context))
            await next.Invoke(context);
    }

    private async Task<bool> CheckUserRolesAsync(HttpContext context)
    {
        var authorizeRoles = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>()?.Roles;
        var requiredRoles = authorizeRoles?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (requiredRoles.IsNullOrEmpty() || context.User.Identity?.IsAuthenticated != true)
            return true;

        var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();

        var userId = userManager.GetUserId(context.User);
        if (userId == null)
        {
            _logger.LogWarning("UserId not found");

            await context.ForbidAsync();
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User {userId} not found");

            await context.ForbidAsync();
            return false;
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!requiredRoles.Any(role => userRoles.Contains(role.Trim(), StringComparer.OrdinalIgnoreCase)))
        {
            _logger.LogWarning($"Access denied for {userId}. Required roles: {authorizeRoles}");

            await context.ForbidAsync();
            return false;
        }

        return true;
    }
}