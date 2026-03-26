using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ToDoApi.Repositories;

public sealed class SqlServerDatabaseInitializer(IConfiguration configuration) : IDatabaseInitializer
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Database", "Initialize.sql");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException("Database initialization script was not found.", scriptPath);
        }

        var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(batch => !string.IsNullOrWhiteSpace(batch))
            .ToArray();

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in batches)
        {
            await using var command = new SqlCommand(batch, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
