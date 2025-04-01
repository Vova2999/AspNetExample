using AspNetExample.Domain.Entities;
using AspNetExample.Services.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Services.Managers;

public class ApplicationContextRoleManager : RoleManager<Role>
{
    public ApplicationContextRoleManager(
        IApplicationContextRoleStore store,
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