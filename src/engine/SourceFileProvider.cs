using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace Vertical.Migrate.Engine;

public sealed class SourceFileProvider(
    ILogger<SourceFileProvider> logger, 
    RuntimeOptions options) : ISourceFileProvider
{
    public async IAsyncEnumerable<MigrationSourceFile> GetSourceFilesAsync(
        IDatabaseProvider provider,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var matcher = new Matcher();
        matcher.AddInclude(options.SourceMatchPattern);

        // Get all paths
        var paths = matcher.GetResultsInFullPath(options.RootSourcePath.FullName);
        
        // Sort based on mode
        var sortedPaths = options.RollbackMode
            ? paths.OrderByDescending(path => path)
            : paths.OrderBy(path => path);
        
        // Create load tasks
        var dialect = provider.CreateParsingDialect();
        var loadSourceTasks = sortedPaths.Select(async path => await MigrationSourceFile.CreateAsync(
            () => File.OpenRead(path),
            path,
            dialect,
            cancellationToken));
        
        // Load
        var sources = await Task.WhenAll(loadSourceTasks);
        
        // Filters by migrationId or range start/end options
        var filteredSources = FilterByOptions(sources, options);
        
        // Retrieve migrations applied from database
        var migrations = (await provider.GetMigrationsAsync(cancellationToken))
            .ToDictionary(src => src.Id);

        foreach (var source in filteredSources)
        {
            var exists = migrations.TryGetValue(source.MigrationId, out var migration);

            if (exists == options.RollbackMode)
                yield return source.AttachMetadata(migration);

            if (migration!.Sha256 != source.Sha)
            {
                logger.LogWarning(
                    "Migration {id} in {path} content may have been altered after it was applied (hashes differ).",
                    source.MigrationId,
                    source.Path);
            }

            if (!options.Force)
            {
                logger.LogDebug("Migration {id} already applied (skipping).", source.MigrationId);
                continue;
            }

            logger.LogWarning(
                "Migration {id} in {path} has already been applied but will be processed again (force=true).",
                source.MigrationId,
                source.Path);
            
            yield return source.AttachMetadata(migration);
        }
    }

    private static IEnumerable<MigrationSourceFile> FilterByOptions(
        MigrationSourceFile[] sources,
        RuntimeOptions options)
    {
        switch (options)
        {
            case { MigrationId: not null }:
                return sources.Where(src => src.MigrationId == options.MigrationId.Value);
            
            case { MigrationIdRangeStart: not null }:
            case { MigrationIdRangeEnd: not null }:
                var (start, end) = (options.MigrationIdRangeStart, options.MigrationIdRangeEnd);
                return sources
                    .SkipWhile(src => !start.HasValue || !start.Value.Equals(src.MigrationId))
                    .TakeWhile(src => !end.HasValue || !end.Value.Equals(src.MigrationId));
            
            default:
                return sources;
        }
    }
}