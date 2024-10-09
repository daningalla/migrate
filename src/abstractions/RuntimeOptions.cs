namespace Vertical.Migrate;

/// <summary>
/// Options used by the RDBMS provider.
/// </summary>
public class RuntimeOptions
{
    /// <summary>
    /// Gets the database provider id.
    /// </summary>
    public string ProviderId { get; set; } = default!;

    /// <summary>
    /// Gets the root source path.
    /// </summary>
    public DirectoryInfo RootSourcePath { get; set; } = default!;

    /// <summary>
    /// Gets the source file match pattern (glob).
    /// </summary>
    public string SourceMatchPattern { get; set; } = default!;
    
    /// <summary>
    /// Gets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// Gets the host name.
    /// </summary>
    public string? Host { get; set; } = default!;
    
    /// <summary>
    /// Gets the connection port.
    /// </summary>
    public uint? Port { get; set; }

    /// <summary>
    /// Gets the name of the database to migrate.
    /// </summary>
    public string? Database { get; set; } = default!;
    
    /// <summary>
    /// Gets the user used to build the connection credential.
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Gets the password used to build the connection string.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets a dictionary of additional connection properties.
    /// </summary>
    public KeyValuePair<string, string>[] Properties { get; set; } = [];
    
    /// <summary>
    /// Gets whether the operation is rollback.
    /// </summary>
    public bool RollbackMode { get; set; }
    
    /// <summary>
    /// Gets the specific migration to execute.
    /// </summary>
    public Guid? MigrationId { get; set; }
    
    /// <summary>
    /// Gets the id of the first migration to execute (inclusive).
    /// </summary>
    public Guid? MigrationIdRangeStart { get; set; }
    
    /// <summary>
    /// Gets the id of the last migration to execute (inclusive).
    /// </summary>
    public Guid? MigrationIdRangeEnd { get; set; }
    
    /// <summary>
    /// Gets whether to ignore existing state and force-apply migration statements. 
    /// </summary>
    public bool Force { get; set; }
}