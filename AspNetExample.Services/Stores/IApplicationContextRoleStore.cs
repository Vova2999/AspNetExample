using AspNetExample.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AspNetExample.Services.Stores;

public interface IApplicationContextRoleStore : IRoleStore<Role>
{
}