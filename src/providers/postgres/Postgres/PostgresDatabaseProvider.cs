using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Vertical.Migrate.Postgres.Postgres;

public sealed partial class PostgresDatabaseProvider(ILogger logger, NpgsqlConnection connection) : IDatabaseProvider
{
    private readonly Lazy<Task> _lazySchemaInitializer = new(() => InitializeMigrationSchemaAsync(logger, connection));

    private NpgsqlTransaction? _transaction;

    /// <inheritdoc />
    public SourceParsingDialect CreateParsingDialect()
    {
        const StringComparison comparer = StringComparison.OrdinalIgnoreCase;

        return new SourceParsingDialect(
            IdMatchProvider: MyRegex(),
            MigrationSectionPredicate: s => s.StartsWith("-- up:", comparer),
            RollbackSectionPredicate: s => s.StartsWith("-- down:", comparer),
            BeginTransactionPredicate: s => s.StartsWith("-- start transaction;", comparer),
            CommitTransactionPredicate: s => s.StartsWith("-- commit;", comparer),
            CommentPredicate: s => s.StartsWith("--"));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MigrationEntry>> GetMigrationsAsync(CancellationToken cancellationToken)
    {
        await EnsureSchemaInitializedAsync();

        const string sql =
            """
            select id, dateApplied, sha256, sourcePath
            from sys.migrations
            """;

        var results = await connection.QueryAsync<MigrationEntry>(sql);

        if (results.TryGetNonEnumeratedCount(out var count))
        {
            logger.LogInformation("Loaded {count} migration entries.", count);
        }
        else logger.LogInformation("Loaded migration entries.");

        return results;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteStatementAsync(Statement statement, CancellationToken cancellationToken)
    {
        switch (statement.Type)
        {
            case StatementType.BeginTransaction when _transaction == null:
                _transaction = await connection.BeginTransactionAsync(cancellationToken);
                return 0;
            
            case StatementType.CommitTransaction when _transaction != null:
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
                return 0;
            
            case StatementType.BeginTransaction:
                throw new InvalidOperationException("Nested transactions not allowed.");
            
            case StatementType.CommitTransaction:
            throw new InvalidOperationException("Transaction never started.");
             
            default:
                return await connection.ExecuteAsync(statement.Body, _transaction);
        }
    }

    /// <inheritdoc />
    public async Task DeleteMigrationAsync(Guid migrationId, CancellationToken cancellationToken)
    {
        await EnsureSchemaInitializedAsync();
        
        const string sql = 
            """
            delete from sys.migrations
            where id = @migrationId
            """;

        await connection.ExecuteAsync(sql, new { migrationId });
    }

    /// <inheritdoc />
    public async Task SaveMigrationAsync(MigrationEntry entry, bool update, CancellationToken cancellationToken)
    {
        await EnsureSchemaInitializedAsync();
        
        if (update)
        {
            const string updateSql =
                """
                update sys.migrations
                set dateApplied = CURRENT_TIMESTAMP,
                    sha256 = @Sha256,
                    sourcePath = @SourcePath
                where id = @Id;
                """;

            await connection.ExecuteAsync(updateSql, entry);
            return;
        }

        const string insertSql =
            
            """
            insert into sys.migrations(id, dateApplied, sha256, sourcePath)
            values(@Id, CURRENT_TIMESTAMP, @Sha256, @SourcePath);
            """;

        await connection.ExecuteAsync(insertSql, entry);
    }

    private async Task EnsureSchemaInitializedAsync()
    {
        await _lazySchemaInitializer.Value;
    }

    private static async Task InitializeMigrationSchemaAsync(ILogger logger, NpgsqlConnection connection)
    {
        const string sql =
            """
            create schema if not exists sys;
            create table if not exists sys.migrations(
                id              uuid not null primary key,
                dateApplied     timestamptz not null default CURRENT_TIMESTAMP,
                sha256          text not null,
                sourcePath      text not null
            );
            """;

        await connection.ExecuteAsync(sql);
        
        logger.LogInformation("Migration schema initialized");
    }

    [GeneratedRegex("^-- migration: ([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})")]
    private static partial Regex MyRegex();
}