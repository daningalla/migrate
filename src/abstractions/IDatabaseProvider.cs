namespace Vertical.Migrate;

public interface IDatabaseProvider
{
    /// <summary>
    /// Creates a dialect specific to the provider.
    /// </summary>
    /// <returns><see cref="SourceParsingDialect"/></returns>
    SourceParsingDialect CreateParsingDialect();

    /// <summary>
    /// Gets migrations.
    /// </summary>
    /// <param name="cancellationToken">Token observed for cancellation.</param>
    /// <returns><see cref="MigrationEntry"/></returns>
    Task<IEnumerable<MigrationEntry>> GetMigrationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes a statement.
    /// </summary>
    /// <param name="statement">Statement to execute.</param>
    /// <param name="cancellationToken">Token observed for cancellation.</param>
    Task<int> ExecuteStatementAsync(Statement statement, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a migration.
    /// </summary>
    /// <param name="migrationId">Id of the migration to delete.</param>
    /// <param name="cancellationToken">Token observed for cancellation.</param>
    Task DeleteMigrationAsync(Guid migrationId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a migration.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="update">Whether to update an existing entry.</param>
    /// <param name="cancellationToken">Token observed for cancellation.</param>
    Task SaveMigrationAsync(MigrationEntry entry, bool update, CancellationToken cancellationToken);
}