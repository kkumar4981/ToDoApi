using Microsoft.AspNetCore.Identity;
using ToDoApi.Models;
using ToDoApi.Models.Admin;
using ToDoApi.Models.Auth;
using ToDoApi.Repositories;

namespace ToDoApi.Services;

public sealed class AdminService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IAdminService
{
    public async Task<IReadOnlyList<UserAdminResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var responses = new List<UserAdminResponse>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            responses.Add(new UserAdminResponse
            {
                Id = user.Id.ToString(),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                CreatedAtUtc = user.CreatedAtUtc,
                Roles = roles.ToArray()
            });
        }

        return responses;
    }

    public async Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(role => new RoleResponse
        {
            Id = role.Id.ToString(),
            Name = role.Name ?? string.Empty
        }).ToArray();
    }

    public async Task<ActionResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.RoleName.Trim(),
            NormalizedName = request.RoleName.Trim().ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var result = await roleManager.CreateAsync(role);
        EnsureSucceeded(result);

        return new ActionResponse
        {
            Message = "Role created successfully."
        };
    }

    public async Task<ActionResponse> AssignRoleAsync(
        string userId,
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User was not found.");

        var result = await userManager.AddToRoleAsync(user, request.RoleName.Trim());
        EnsureSucceeded(result);

        return new ActionResponse
        {
            Message = "Role assigned successfully."
        };
    }

    public async Task<ActionResponse> RemoveRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User was not found.");

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        EnsureSucceeded(result);

        return new ActionResponse
        {
            Message = "Role removed successfully."
        };
    }

    public async Task<ActionResponse> UnlockUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User was not found.");

        var lockoutResult = await userManager.SetLockoutEndDateAsync(user, null);
        EnsureSucceeded(lockoutResult);

        var resetResult = await userManager.ResetAccessFailedCountAsync(user);
        EnsureSucceeded(resetResult);

        return new ActionResponse
        {
            Message = "User unlocked successfully."
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
