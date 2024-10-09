namespace Vertical.Migrate.Engine;

public interface ISourceFileProvider
{
    IAsyncEnumerable<MigrationSourceFile> GetSourceFilesAsync(
        IDatabaseProvider provider,
        CancellationToken cancellationToken);
}