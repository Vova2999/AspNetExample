using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Services.Stores;

public class ApplicationContextUserStore : IQueryableUserStore<User>, IUserPasswordStore<User>, IUserRoleStore<User>, IAsyncDisposable
{
    private readonly IRoleStore<Role> _roleStore;
    private readonly ApplicationContext _context;

    public IQueryable<User> Users => _context.Users;

    public ApplicationContextUserStore(
        IRoleStore<Role> roleStore,
        IApplicationContextFactory applicationContextFactory)
    {
        _roleStore = roleStore;
        _context = applicationContextFactory.Create();
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.Name)!;
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.Name = userName.EmptyIfNull();
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.NormalizedName)!;
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.NormalizedName = normalizedName.EmptyIfNull();
        return Task.CompletedTask;
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = Guid.Parse(userId);
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Users.FirstOrDefaultAsync(user => user.NormalizedName == normalizedUserName, cancellationToken);
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _context.Users.Attach(user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.PasswordHash = passwordHash.EmptyIfNull();
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.PasswordHash)!;
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(user.PasswordHash.IsSignificant());
    }

    public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with name {roleName} not found");

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with name {roleName} not found");

        await _context.UserRoles
            .Where(userRole => userRole.UserId == user.Id && userRole.RoleId == role.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.UserRoles
            .Where(userRole => userRole.UserId == user.Id)
            .Select(userRole => userRole.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.UserRoles
            .AnyAsync(userRole => userRole.UserId == user.Id && userRole.Role.NormalizedName == roleName, cancellationToken);
    }

    public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Users
            .Where(user => user.UserRoles.Any(userRole => userRole.Role.NormalizedName == roleName))
            .ToListAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}