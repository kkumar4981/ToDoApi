using System.Data;
using Microsoft.Data.SqlClient;
using ToDoApi.Models;

namespace ToDoApi.Repositories;

public sealed class SqlServerRoleRepository(IConfiguration configuration) : IRoleRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    public async Task<ApplicationRole?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityRoles_GetById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = roleId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRole(reader) : null;
    }

    public async Task<ApplicationRole?> GetByNormalizedNameAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityRoles_GetByNormalizedName", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@NormalizedName", SqlDbType.NVarChar, 256).Value = normalizedRoleName;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRole(reader) : null;
    }

    public async Task<IReadOnlyList<ApplicationRole>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = new List<ApplicationRole>();

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityRoles_List", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(MapRole(reader));
        }

        return roles;
    }

    public async Task CreateAsync(ApplicationRole role, CancellationToken cancellationToken = default)
    {
        await ExecuteRoleCommandAsync("dbo.usp_IdentityRoles_Create", role, cancellationToken);
    }

    public async Task UpdateAsync(ApplicationRole role, CancellationToken cancellationToken = default)
    {
        await ExecuteRoleCommandAsync("dbo.usp_IdentityRoles_Update", role, cancellationToken);
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.usp_IdentityRoles_Delete", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = roleId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task ExecuteRoleCommandAsync(
        string procedureName,
        ApplicationRole role,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = role.Id;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 256).Value = (object?)role.Name ?? DBNull.Value;
        command.Parameters.Add("@NormalizedName", SqlDbType.NVarChar, 256).Value = (object?)role.NormalizedName ?? DBNull.Value;
        command.Parameters.Add("@ConcurrencyStamp", SqlDbType.NVarChar, 100).Value = (object?)role.ConcurrencyStamp ?? DBNull.Value;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static ApplicationRole MapRole(SqlDataReader reader)
    {
        return new ApplicationRole
        {
            Id = reader.GetGuid(0),
            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
            NormalizedName = reader.IsDBNull(2) ? null : reader.GetString(2),
            ConcurrencyStamp = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }
}
