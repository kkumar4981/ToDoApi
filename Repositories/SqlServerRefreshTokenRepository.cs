using System.Data;
using Microsoft.Data.SqlClient;
using ToDoApi.Models;

namespace ToDoApi.Repositories;

public sealed class SqlServerRefreshTokenRepository(IConfiguration configuration) : IRefreshTokenRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    public async Task CreateAsync(ApplicationRefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_RefreshTokens_Create", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = refreshToken.Id;
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = refreshToken.UserId;
        command.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = refreshToken.Token;
        command.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar, 100).Value = refreshToken.SecurityStamp;
        command.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = refreshToken.ExpiresAtUtc;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = refreshToken.CreatedAtUtc;
        command.Parameters.Add("@CreatedByIp", SqlDbType.NVarChar, 45).Value = (object?)refreshToken.CreatedByIp ?? DBNull.Value;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ApplicationRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_RefreshTokens_GetByToken", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = token;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRefreshToken(reader) : null;
    }

    public async Task RevokeAsync(
        Guid refreshTokenId,
        DateTime revokedAtUtc,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_RefreshTokens_Revoke", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = refreshTokenId;
        command.Parameters.Add("@RevokedAtUtc", SqlDbType.DateTime2).Value = revokedAtUtc;
        command.Parameters.Add("@RevokedByIp", SqlDbType.NVarChar, 45).Value = (object?)revokedByIp ?? DBNull.Value;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        DateTime revokedAtUtc,
        string? revokedByIp,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_RefreshTokens_RevokeAllForUser", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        command.Parameters.Add("@RevokedAtUtc", SqlDbType.DateTime2).Value = revokedAtUtc;
        command.Parameters.Add("@RevokedByIp", SqlDbType.NVarChar, 45).Value = (object?)revokedByIp ?? DBNull.Value;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static ApplicationRefreshToken MapRefreshToken(SqlDataReader reader)
    {
        return new ApplicationRefreshToken
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetGuid(1),
            Token = reader.GetString(2),
            SecurityStamp = reader.GetString(3),
            ExpiresAtUtc = reader.GetDateTime(4),
            CreatedAtUtc = reader.GetDateTime(5),
            RevokedAtUtc = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            CreatedByIp = reader.IsDBNull(7) ? null : reader.GetString(7),
            RevokedByIp = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }
}
