using System.Security.Cryptography;

namespace Vertical.Migrate.Engine;

/// <summary>
/// Wraps reading of a migration file.
/// </summary>
public sealed class MigrationSourceFile
{
    private readonly Lazy<Task<Statement[]>> _lazyMigrationStatements;
    private readonly Lazy<Task<Statement[]>> _lazyRollbackStatements;
    
    private MigrationSourceFile(Func<Stream> provider,
        string path,
        string sha,
        Guid migrationId,
        SourceParsingDialect parsingDialect,
        MigrationEntry? metadataEntry)
    {
        _provider = provider;
        _lazyMigrationStatements = new Lazy<Task<Statement[]>>(() => ReadStatementsAsync(
            parsingDialect.MigrationSectionPredicate,
            parsingDialect.RollbackSectionPredicate));
        _lazyRollbackStatements = new Lazy<Task<Statement[]>>(() => ReadStatementsAsync(
            parsingDialect.RollbackSectionPredicate,
            parsingDialect.MigrationSectionPredicate));
        
        Path = path;
        Sha = sha;
        MigrationId = migrationId;
        ParsingDialect = parsingDialect;
        MetadataEntry = metadataEntry;
    }

    private readonly Func<Stream> _provider;

    /// <summary>
    /// Gets a path that describes the location of the source.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets a SHA of the content.
    /// </summary>
    public string Sha { get; }

    /// <summary>
    /// Gets the migration id - could be <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid MigrationId { get; }

    /// <summary>
    /// Gets the parsing dialect.
    /// </summary>
    public SourceParsingDialect ParsingDialect { get; }

    /// <summary>
    /// Gets the migration entry.
    /// </summary>
    public MigrationEntry? MetadataEntry { get; }

    /// <summary>
    /// Returns a new instance with metadata.
    /// </summary>
    /// <param name="entry">Entry to attach.</param>
    /// <returns><see cref="MigrationSourceFile"/></returns>
    public MigrationSourceFile AttachMetadata(MigrationEntry? entry) => new(
        _provider,
        Path,
        Sha,
        MigrationId,
        ParsingDialect,
        entry);

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="provider">Stream provider.</param>
    /// <param name="path">Path that describes the location of the source.</param>
    /// <param name="parsingDialect">Dialect used when parsing content.</param>
    /// <param name="cancellationToken">Token observed for cancellation.</param>
    /// <returns><see cref="MigrationSourceFile"/></returns>
    public static async Task<MigrationSourceFile> CreateAsync(
        Func<Stream> provider,
        string path,
        SourceParsingDialect parsingDialect,
        CancellationToken cancellationToken)
    {
        return new MigrationSourceFile(
            provider,
            path,
            await ComputeHashAsync(provider, cancellationToken),
            await ReadMigrationIdAsync(provider, parsingDialect, cancellationToken),
            parsingDialect,
            null);
    }

    /// <summary>
    /// Reads migration statements from the source file.
    /// </summary>
    /// <returns>A task that completes with the statements.</returns>
    public async Task<Statement[]> ReadMigrationStatementsAsync() => await _lazyMigrationStatements.Value;

    /// <summary>
    /// Reads rollback statements from the source file.
    /// </summary>
    /// <returns>A task that completes with the statements.</returns>
    public async Task<Statement[]> ReadRollbackStatementsAsync() => await _lazyRollbackStatements.Value;

    private async Task<Statement[]> ReadStatementsAsync(
        Func<string, bool> segmentStartPredicate,
        Func<string, bool> segmentStopPredicate)
    {
        var dialect = ParsingDialect;
        var state = new StatementListBuilder();
        
        await StreamScanner.ScanAsync(
            _provider,
            state,
            state.Statements,
            (scan, content) =>
            {
                var builder = scan.State;
                if (!builder.SectionEntered)
                {
                    builder.SectionEntered = segmentStartPredicate(content);
                    return scan;
                }

                if (segmentStopPredicate(content))
                    return scan.Complete();

                if (dialect.BeginTransactionPredicate(content))
                {
                    builder.TryEnterTransactionBlock(Path, scan.LineNumber);
                    return scan;
                }

                if (dialect.CommitTransactionPredicate(content))
                {
                    builder.TryLeaveTransactionBlock(Path, scan.LineNumber);
                    return scan;
                }
                
                builder.AppendStatements(content);
                return scan;
            },
            CancellationToken.None);

        return state.Build();
    }

    private static async Task<string> ComputeHashAsync(Func<Stream> provider, CancellationToken cancellationToken)
    {
        await using var stream = provider();
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    private static async Task<Guid> ReadMigrationIdAsync(Func<Stream> provider,
        SourceParsingDialect parsingDialect,
        CancellationToken cancellationToken)
    {
        return await StreamScanner.ScanAsync(
            provider,
            parsingDialect,
            Guid.Empty,
            (scan, content) =>
            {
                var match = scan.State.IdMatchProvider.Match(content);
                return match.Success
                    ? scan.Complete(Guid.Parse(match.Groups[1].Value))
                    : scan.Next();
            },
            cancellationToken);
    }
}