using Microsoft.AspNetCore.Identity;
using ToDoApi.Models;
using ToDoApi.Repositories;

namespace ToDoApi.Stores;

public sealed class SqlRoleStore(IRoleRepository roleRepository) : IRoleStore<ApplicationRole>
{
    public void Dispose()
    {
    }

    public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            await roleRepository.CreateAsync(role, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception exception)
        {
            return Failed(exception);
        }
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            await roleRepository.UpdateAsync(role, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception exception)
        {
            return Failed(exception);
        }
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        try
        {
            await roleRepository.DeleteAsync(role.Id, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception exception)
        {
            return Failed(exception);
        }
    }

    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(ApplicationRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(
        ApplicationRole role,
        string? normalizedName,
        CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<ApplicationRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return Guid.TryParse(roleId, out var parsedRoleId)
            ? await roleRepository.GetByIdAsync(parsedRoleId, cancellationToken)
            : null;
    }

    public Task<ApplicationRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return roleRepository.GetByNormalizedNameAsync(normalizedRoleName, cancellationToken);
    }

    private static IdentityResult Failed(Exception exception)
    {
        return IdentityResult.Failed(new IdentityError
        {
            Description = exception.Message
        });
    }
}
