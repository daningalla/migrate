using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Vertical.Migrate.Engine;

/// <summary>
/// Executes statements in a source file.
/// </summary>
public sealed class SourceFileProcessor(
    ILogger<SourceFileProcessor> logger,
    RuntimeOptions options) : ISourceFileProcessor
{
    /// <inheritdoc />
    public async Task ProcessSourceAsync(
        IDatabaseProvider provider, 
        MigrationSourceFile source,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        if (options.RollbackMode)
        {
            await ExecuteRollbackAsync(provider, source, cancellationToken);
        }
        else
        {
            await ExecuteMigrationAsync(provider, source, cancellationToken);
        }
        
        logger.LogInformation("Migration complete ({time}ms)", timer.ElapsedMilliseconds);
    }

    private async Task ExecuteMigrationAsync(
        IDatabaseProvider provider, 
        MigrationSourceFile source,
        CancellationToken cancellationToken)
    {
        var statements = await source.ReadMigrationStatementsAsync();

        if (await TryExecuteStatementsAsync(provider, statements, cancellationToken))
        {
            var update = source.MetadataEntry != null;
            var entry = source.MetadataEntry ?? new MigrationEntry
            {
                Id = source.MigrationId,
                Sha256 = source.Sha,
                DateApplied = DateTimeOffset.Now,
                SourcePath = source.Path
            };

            await provider.SaveMigrationAsync(entry, update, cancellationToken);
            return;
        }
        
        logger.LogWarning("Failed to apply migration statement, beginning rollbacks.");

        await ExecuteRollbackAsync(provider, source, cancellationToken);
        throw new ApplicationException("Stop: migration failed.");
    }

    private async Task ExecuteRollbackAsync(
        IDatabaseProvider provider,
        MigrationSourceFile source,
        CancellationToken cancellationToken)
    {
        var statements = await source.ReadRollbackStatementsAsync();

        if (!await TryExecuteStatementsAsync(provider, statements, cancellationToken))
        {
            throw new ApplicationException("Stop: rollback failed.");
        }
        
        await provider.DeleteMigrationAsync(source.MigrationId, cancellationToken);
    }

    private async Task<bool> TryExecuteStatementsAsync(
        IDatabaseProvider provider, 
        Statement[] statements,
        CancellationToken cancellationToken)
    {
        var i = 1;
        var timer = Stopwatch.StartNew();
        
        foreach (var statement in statements)
        {
            timer.Restart();

            if (!await TryExecuteStatementAsync(provider, statement, cancellationToken))
                return false;

            logger.LogInformation("Statement {number}/{count} applied ({time}ms)",
                i++,
                statements.Length,
                timer.ElapsedMilliseconds);
        }

        return true;
    }

    private async Task<bool> TryExecuteStatementAsync(
        IDatabaseProvider provider, 
        Statement statement,
        CancellationToken cancellationToken)
    {
        try
        {
            await provider.ExecuteStatementAsync(statement, cancellationToken);
            LogDebugInfo(statement);
            
            return true;
        }
        catch (Exception exception)
        {
            var body = statement.Type switch
            {
                StatementType.StatementBody => statement.Body,
                StatementType.BeginTransaction => "(begin transaction)",
                _ => "(commit transaction)"
            };
            
            logger.LogError(
                """
                An error occurred while executing a statement: {message}
                Statement:
                {body}
                """, exception.Message, body);

            return false;
        }
    }

    private void LogDebugInfo(Statement statement)
    {
        if (!logger.IsEnabled(LogLevel.Debug)) 
            return;
        
        switch (statement.Type)
        {
            case StatementType.BeginTransaction:
                logger.LogDebug("Started transaction");
                break;
                
            case StatementType.CommitTransaction:
                logger.LogDebug("Committed transaction");
                break;
                
            default:
                logger.LogDebug(
                    """
                    Executed statement:
                    {body}
                    """, statement.Body);
                break;
        }
    }
}