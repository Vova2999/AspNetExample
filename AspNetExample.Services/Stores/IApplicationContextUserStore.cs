using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Services.Stores;

public interface IApplicationContextUserStore : IUserStore<User>
{
    Task<User?> FindByIdAndLoadRolesAsync(string userId, CancellationToken cancellationToken);
    Task<User?> FindByNameAndLoadRolesAsync(string normalizedUserName, CancellationToken cancellationToken);
}