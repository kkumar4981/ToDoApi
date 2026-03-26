using ToDoApi.Models;

namespace ToDoApi.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(ApplicationRefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<ApplicationRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeAsync(Guid refreshTokenId, DateTime revokedAtUtc, string? revokedByIp, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, DateTime revokedAtUtc, string? revokedByIp, CancellationToken cancellationToken = default);
}
