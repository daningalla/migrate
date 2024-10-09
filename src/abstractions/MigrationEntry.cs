namespace Vertical.Migrate;

public sealed class MigrationEntry
{
    /// <summary>
    /// Gets the unique migration id.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets the date the migration was applied.
    /// </summary>
    public DateTimeOffset DateApplied { get; set; }

    /// <summary>
    /// Gets the hash of the content source.
    /// </summary>
    public string Sha256 { get; set; } = default!;

    /// <summary>
    /// Gets the source path.
    /// </summary>
    public string SourcePath { get; set; } = default!;
}