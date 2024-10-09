namespace Vertical.Migrate.Engine;

public interface ISourceFileProcessor
{
    Task ProcessSourceAsync(IDatabaseProvider provider, MigrationSourceFile source, CancellationToken cancellationToken);
}