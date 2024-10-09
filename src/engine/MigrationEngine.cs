using Microsoft.Extensions.Logging;

namespace Vertical.Migrate.Engine;

public sealed class MigrationEngine(
        ILoggerFactory loggerFactory,
        RuntimeOptions options,
        ISourceFileProvider sourceProvider,
        ISourceFileProcessor sourceFileProcessor,
        IEnumerable<DatabaseProviderFactory> providerFactories) : IMigrationEngine
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<MigrationEngine>();
    
    /// <inheritdoc />
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            LogOptions();
            await ExecuteInternalAsync(cancellationToken);
            _logger.LogInformation("Migration(s) complete.");
            return 0;
        }
        catch (ApplicationException applicationException)
        {
            _logger.LogError("Stop: {error}", applicationException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred.");
        }

        _logger.LogInformation("Migrations stopped");
        return -1;
    }

    private async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting migration tasks");

        var provider = await ResolveDatabaseProviderAsync(cancellationToken);

        await foreach (var source in sourceProvider.GetSourceFilesAsync(provider, cancellationToken))
        {
            await sourceFileProcessor.ProcessSourceAsync(provider, source, cancellationToken);
        }
    }

    private async Task<IDatabaseProvider> ResolveDatabaseProviderAsync(CancellationToken cancellationToken)
    {
        var factory = providerFactories
            .FirstOrDefault(pf => pf.ProviderId.Equals(
                options.ProviderId, 
                StringComparison.OrdinalIgnoreCase))?
            .AsyncFactory;

        if (factory == null)
        {
            throw new ApplicationException($"Unsupported database provider '{options.ProviderId}'");
        }

        var provider = await factory(loggerFactory);
        _logger.LogInformation("Created database provider {provider}", provider);

        return provider;
    }

    private void LogOptions()
    {
        _logger.LogInformation(
            """
            Starting process with the following parameters:
            providerId:         {providerId},
            rollbackMode:       {rollback},
            forceContinue:      {force},
            migrationId:        {migrationId}
            rangedTarget:       {ranged}
            """,
            options.ProviderId,
            options.RollbackMode,
            options.Force,
            options.MigrationId,
            options.MigrationIdRangeStart.HasValue || options.MigrationIdRangeEnd.HasValue);
    }
}