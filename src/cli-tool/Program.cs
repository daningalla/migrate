using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.Cli;
using Vertical.Migrate.Cli;
using Vertical.Migrate.Engine;
using Vertical.Migrate.Postgres;

var rootCommand = CliConfiguration.ConfigureRootCommand();

rootCommand.HandleAsync(async (options, cancellationToken) =>
{
    await using var serviceProvider = new ServiceCollection()
        .AddLogging(builder => builder
            .SetMinimumLevel(options.LogLevel)
            .AddConsole())
        .AddMigrateEngine(options)
        .AddPostgresProvider()
        .BuildServiceProvider();

    var engine = serviceProvider.GetRequiredService<IMigrationEngine>();

    return await engine.ExecuteAsync(cancellationToken);
});

var argString =
    """
    --root-path C:\Users\dan\Dev\Vertical\vertical-migrate\test\migrations
    --match-pattern **/*.sql
    --provider postgres
    -h localhost
    -d testdb
    -u postgres
    -p P@ssw0rd!
    --rollback
    """;

args = argString
    .Split(Environment.NewLine)
    .SelectMany(str => str.Split(" "))
    .ToArray();

try
{
    return await rootCommand.InvokeAsync(args);
}
catch (CommandLineException cliException)
{
    Console.Write("\x1b[39m");
    Console.Write(cliException.Message);
    Console.WriteLine("\x1b[39m");
    return -1;
}
