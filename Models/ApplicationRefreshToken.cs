namespace ToDoApi.Models;

public sealed class ApplicationRefreshToken
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Token { get; init; } = string.Empty;

    public string SecurityStamp { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? RevokedAtUtc { get; init; }

    public string? CreatedByIp { get; init; }

    public string? RevokedByIp { get; init; }
}
