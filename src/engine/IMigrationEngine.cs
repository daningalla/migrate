namespace Vertical.Migrate.Engine;

public interface IMigrationEngine
{
    Task<int> ExecuteAsync(CancellationToken cancellationToken);
}