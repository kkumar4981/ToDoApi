using ToDoApi.Models.Admin;
using ToDoApi.Models.Auth;

namespace ToDoApi.Services;

public interface IAdminService
{
    Task<IReadOnlyList<UserAdminResponse>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<ActionResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> AssignRoleAsync(string userId, AssignRoleRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> RemoveRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    Task<ActionResponse> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
}
