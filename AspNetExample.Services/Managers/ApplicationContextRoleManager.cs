using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Services.Managers;

public class ApplicationContextRoleManager : RoleManager<Role>
{
    public ApplicationContextRoleManager(
        IRoleStore<Role> store,
        IEnumerable<IRoleValidator<Role>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<ApplicationContextRoleManager> logger)
        : base(
            store,
            roleValidators,
            keyNormalizer,
            errors,
            logger)
    {
    }
}