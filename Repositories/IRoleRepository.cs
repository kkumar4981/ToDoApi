using ToDoApi.Models;

namespace ToDoApi.Repositories;

public interface IRoleRepository
{
    Task<ApplicationRole?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<ApplicationRole?> GetByNormalizedNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationRole>> GetAllAsync(CancellationToken cancellationToken = default);

    Task CreateAsync(ApplicationRole role, CancellationToken cancellationToken = default);

    Task UpdateAsync(ApplicationRole role, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);
}
