using Vertical.Cli;
using Vertical.Cli.Parsing;

namespace Vertical.Migrate.Cli;

public static class CliConfiguration
{
    public static RootCommand<CliOptions> ConfigureRootCommand()
    {
        var rootCommand = new RootCommand<CliOptions>("migrate");

        rootCommand
            .AddOption(x => x.RootSourcePath,
                ["--root-path"],
                description: "The root path where source files can be found (defaults to current directory).",
                defaultProvider: () => new DirectoryInfo(Directory.GetCurrentDirectory()),
                operandSyntax: "PATH")
            .AddOption(x => x.SourceMatchPattern,
                ["--match-pattern"],
                description: "The globbing pattern used to find source files in the root path (defaults to **/*.sql).",
                operandSyntax: "PATTERN")
            .AddOption(x => x.ProviderId,
                ["--provider"],
                description: "Moniker of the specific database provider to use.",
                operandSyntax: "ID")
            .AddOption(x => x.ConnectionString,
                ["--connection"],
                description: "The connection string to use when connecting to the database server. The presence of " +
                             "this value typically informs the provider to ignore all the other parameters.",
                operandSyntax: "CONNECTION_STR")
            .AddOption(x => x.Host,
                ["-h", "--host"],
                description: "Host server to connect to.")
            .AddOption(x => x.Port,
                ["--port"],
                description: "Port the connector driver should use, otherwise the default provider port is used.")
            .AddOption(x => x.Database,
                ["-d", "--database"],
                description: "Specific database to connect to that will have the migrations applied.",
                operandSyntax: "NAME")
            .AddOption(x => x.UserId,
                ["-u", "--user"],
                description: "User name used when building the connection credential.")
            .AddOption(x => x.Password,
                ["-p", "--password"],
                description: "Password used when building the connection credential.")
            .AddOption(x => x.Properties,
                ["--prop"],
                arity: Arity.ZeroOrMany,
                description: "A key/value pair that represents a connection property and value used by the connector " +
                             "provider.",
                operandSyntax: "KEY=VALUE")
            .AddOption(x => x.LogLevel,
                ["--log-level"],
                description: "Verbosity of logging output (Debug, Information, Warning, Error)",
                operandSyntax: "LEVEL")
            .AddSwitch(x => x.RollbackMode,
                ["--rollback"],
                description: "Applied rollback segments of the migration sources and processes the source files in " +
                             "reverse order.")
            .AddOption(x => x.MigrationId,
                ["--migration-id"],
                description: "The id of the migration to execute; all other migration sources are ignored even if " +
                             "it places the source sequence out of order.",
                operandSyntax: "UUID")
            .AddOption(x => x.MigrationIdRangeStart,
                ["--migration-start-id"],
                description:
                "The id of the first migration to execute in the source sequence. Any migrations leading " +
                "up to this identifier are ignored.",
                operandSyntax: "UUID")
            .AddOption(x => x.MigrationIdRangeEnd,
                ["--migration-end-id"],
                description: "The id of the last migration to execute in the source sequence. Any migrations found " +
                             "after this identifier are ignored.",
                operandSyntax: "UUID")
            .AddSwitch(x => x.Force,
                ["--force"],
                description: "When set, ignores the current metadata state. Use this switch with cation as it may " +
                             "invalidate migration state.");

        rootCommand.AddHelpSwitch();

        rootCommand.ConfigureOptions(opt =>
        {
            opt.ValueConverters.Add(new KeyValuePairConverter());

            opt.ArgumentPreProcessors =
            [
                ArgumentPreProcessors.AddResponseFileArguments,
                ArgumentPreProcessors.ReplaceEnvironmentVariables,
                ArgumentPreProcessors.ReplaceSpecialFolderPaths
            ];
        });

        return rootCommand;
    }    
}