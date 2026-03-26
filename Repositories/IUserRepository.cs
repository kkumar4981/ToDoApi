using ToDoApi.Models;

namespace ToDoApi.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);

    Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddToRoleAsync(Guid userId, string normalizedRoleName, CancellationToken cancellationToken = default);

    Task RemoveFromRoleAsync(Guid userId, string normalizedRoleName, CancellationToken cancellationToken = default);

    Task<bool> IsInRoleAsync(Guid userId, string normalizedRoleName, CancellationToken cancellationToken = default);

    Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default);
}
