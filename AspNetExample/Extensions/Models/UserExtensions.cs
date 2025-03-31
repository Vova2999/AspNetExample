using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Extensions.Models;

public static class UserExtensions
{
    public static async IAsyncEnumerable<UserModel> ToModelsAsync(this IEnumerable<User> users, UserManager<User> userManager)
    {
        foreach (var user in users)
            yield return await user.ToModelAsync(userManager);
    }

    public static async Task<UserModel> ToModelAsync(this User user, UserManager<User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        return user.ToModel(roles.Contains(RoleTokens.AdminRole), roles.Contains(RoleTokens.SwaggerRole));
    }

    public static UserModel ToModel(this User user, bool hasAdminRole, bool hasSwaggerRole)
    {
        return new UserModel
        {
            Id = user.Id,
            Login = user.Name,
            HasAdminRole = hasAdminRole,
            HasSwaggerRole = hasSwaggerRole
        };
    }
}