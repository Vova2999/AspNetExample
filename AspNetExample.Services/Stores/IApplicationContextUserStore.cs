using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Services.Stores;

public interface IApplicationContextUserStore : IQueryableUserStore<User>, IUserPasswordStore<User>, IUserRoleStore<User>
{
    Task<User?> FindByIdAndLoadRolesAsync(string userId, CancellationToken cancellationToken);
    Task<User?> FindByNameAndLoadRolesAsync(string normalizedUserName, CancellationToken cancellationToken);
}