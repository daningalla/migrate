using Microsoft.Extensions.Logging;

namespace Vertical.Migrate.Cli;

/// <summary>
/// Options used by the CLI tool.
/// </summary>
public sealed class CliOptions : RuntimeOptions
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}