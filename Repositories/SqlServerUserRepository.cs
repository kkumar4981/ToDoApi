using System.Data;
using Microsoft.Data.SqlClient;
using ToDoApi.Models;

namespace ToDoApi.Repositories;

public sealed class SqlServerUserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUsers_GetById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<ApplicationUser?> GetByNormalizedUserNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUsers_GetByNormalizedUserName", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@NormalizedUserName", SqlDbType.NVarChar, 256).Value = normalizedUserName;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<ApplicationUser?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUsers_GetByNormalizedEmail", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@NormalizedEmail", SqlDbType.NVarChar, 256).Value = normalizedEmail;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = new List<ApplicationUser>();

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUsers_List", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await ExecuteUserCommandAsync("dbo.usp_IdentityUsers_Create", user, cancellationToken);
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await ExecuteUserCommandAsync("dbo.usp_IdentityUsers_Update", user, cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUsers_Delete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roles = new List<string>();

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUserRoles_GetRoleNamesByUserId", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    public async Task AddToRoleAsync(
        Guid userId,
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUserRoles_Add", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        command.Parameters.Add("@NormalizedRoleName", SqlDbType.NVarChar, 256).Value = normalizedRoleName;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(
        Guid userId,
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUserRoles_Remove", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        command.Parameters.Add("@NormalizedRoleName", SqlDbType.NVarChar, 256).Value = normalizedRoleName;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(
        Guid userId,
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUserRoles_IsInRole", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
        command.Parameters.Add("@NormalizedRoleName", SqlDbType.NVarChar, 256).Value = normalizedRoleName;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && Convert.ToBoolean(result);
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        var users = new List<ApplicationUser>();

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityUserRoles_GetUsersInRole", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@NormalizedRoleName", SqlDbType.NVarChar, 256).Value = normalizedRoleName;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    private async Task ExecuteUserCommandAsync(
        string procedureName,
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = user.Id;
        command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = (object?)user.UserName ?? DBNull.Value;
        command.Parameters.Add("@NormalizedUserName", SqlDbType.NVarChar, 256).Value = (object?)user.NormalizedUserName ?? DBNull.Value;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 256).Value = (object?)user.Email ?? DBNull.Value;
        command.Parameters.Add("@NormalizedEmail", SqlDbType.NVarChar, 256).Value = (object?)user.NormalizedEmail ?? DBNull.Value;
        command.Parameters.Add("@EmailConfirmed", SqlDbType.Bit).Value = user.EmailConfirmed;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = (object?)user.PasswordHash ?? DBNull.Value;
        command.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar, 100).Value = (object?)user.SecurityStamp ?? DBNull.Value;
        command.Parameters.Add("@ConcurrencyStamp", SqlDbType.NVarChar, 100).Value = (object?)user.ConcurrencyStamp ?? DBNull.Value;
        command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value = (object?)user.PhoneNumber ?? DBNull.Value;
        command.Parameters.Add("@PhoneNumberConfirmed", SqlDbType.Bit).Value = user.PhoneNumberConfirmed;
        command.Parameters.Add("@TwoFactorEnabled", SqlDbType.Bit).Value = user.TwoFactorEnabled;
        command.Parameters.Add("@LockoutEnd", SqlDbType.DateTimeOffset).Value =
            user.LockoutEnd.HasValue ? user.LockoutEnd.Value : DBNull.Value;
        command.Parameters.Add("@LockoutEnabled", SqlDbType.Bit).Value = user.LockoutEnabled;
        command.Parameters.Add("@AccessFailedCount", SqlDbType.Int).Value = user.AccessFailedCount;
        command.Parameters.Add("@CreatedAtUtc", SqlDbType.DateTime2).Value = user.CreatedAtUtc;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static ApplicationUser MapUser(SqlDataReader reader)
    {
        return new ApplicationUser
        {
            Id = reader.GetGuid(0),
            UserName = reader.IsDBNull(1) ? null : reader.GetString(1),
            NormalizedUserName = reader.IsDBNull(2) ? null : reader.GetString(2),
            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
            NormalizedEmail = reader.IsDBNull(4) ? null : reader.GetString(4),
            EmailConfirmed = reader.GetBoolean(5),
            PasswordHash = reader.IsDBNull(6) ? null : reader.GetString(6),
            SecurityStamp = reader.IsDBNull(7) ? null : reader.GetString(7),
            ConcurrencyStamp = reader.IsDBNull(8) ? null : reader.GetString(8),
            PhoneNumber = reader.IsDBNull(9) ? null : reader.GetString(9),
            PhoneNumberConfirmed = reader.GetBoolean(10),
            TwoFactorEnabled = reader.GetBoolean(11),
            LockoutEnd = reader.IsDBNull(12) ? null : reader.GetDateTimeOffset(12),
            LockoutEnabled = reader.GetBoolean(13),
            AccessFailedCount = reader.GetInt32(14),
            CreatedAtUtc = reader.GetDateTime(15)
        };
    }
}
