﻿using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Models.Users;
using AspNetExample.Services.Managers;

namespace AspNetExample.Extensions.Models;

public static class UserExtensions
{
    public static async IAsyncEnumerable<UserModel> ToModelsAsync(
        this IEnumerable<User> users,
        ApplicationContextUserManager applicationContextUserManager)
    {
        foreach (var user in users)
            yield return await user.ToModelAsync(applicationContextUserManager);
    }

    public static async Task<UserModel> ToModelAsync(
        this User user,
        ApplicationContextUserManager applicationContextUserManager)
    {
        var roles = await applicationContextUserManager.GetRolesAsync(user);
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