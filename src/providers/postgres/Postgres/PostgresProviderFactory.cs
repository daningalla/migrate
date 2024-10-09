using Microsoft.Extensions.Logging;
using Npgsql;

namespace Vertical.Migrate.Postgres.Postgres;

public static class PostgresProviderFactory
{
    public static DatabaseProviderFactory GetInstance(RuntimeOptions options)
    {
        var connectionBuilder = CreateConnectionBuilder(options);

        return new DatabaseProviderFactory(
            "postgres",
            async loggerFactory =>
            {
                var logger = loggerFactory.CreateLogger<PostgresDatabaseProvider>();
                var connection = new NpgsqlConnection(connectionBuilder.ToString());
                await connection.OpenAsync();

                return new PostgresDatabaseProvider(logger, connection);
            });
    }

    private static NpgsqlConnectionStringBuilder CreateConnectionBuilder(RuntimeOptions options)
    {
        var builder = new NpgsqlConnectionStringBuilder();

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            builder.ConnectionString = options.ConnectionString;
            return builder;
        }

        builder.Host = options.Host ?? throw MissingRequiredOption("host");
        builder.Database = options.Database ?? throw MissingRequiredOption("database");
        builder.Username = options.UserId ?? throw MissingRequiredOption("user name");
        builder.Password = options.Password ?? throw MissingRequiredOption("password");

        if (options.Port.HasValue)
        {
            builder.Port = (int)options.Port.Value;
        }
        
        foreach (var (key, value) in options.Properties)
        {
            switch (key.ToLower())
            {
                case "multiplexing":
                    builder.Multiplexing = ParseHelper.ParseOrThrow<bool>(value, "postgres.multiplexing");
                    break;
                
                case "options":
                    builder.Options = value;
                    break;
                
                case "pooling":
                    builder.Pooling = ParseHelper.ParseOrThrow<bool>(value, "postgres.pooling");
                    break;
                
                case "timeout":
                    builder.Timeout = ParseHelper.ParseOrThrow<int>(value, "postgres.timeout");
                    break;
                
                case "cancellationtimeout":
                    builder.CancellationTimeout = ParseHelper.ParseOrThrow<int>(value, "postgres.cancellationTimeout");
                    break;
                
                case "commandtimeout":
                    builder.CommandTimeout = ParseHelper.ParseOrThrow<int>(value, "postgres.commandtimeout");
                    break;
                
                case "connectionlifetime":
                    builder.ConnectionLifetime = ParseHelper.ParseOrThrow<int>(value, "postgres.connectionlifetime");
                    break;
                
                case "keepalive":
                    builder.KeepAlive = ParseHelper.ParseOrThrow<int>(value, "postgres.keepalive");
                    break;
                
                case "rootcertificate":
                    builder.RootCertificate = value;
                    break;
                
                case "sslcertificate":
                    builder.SslCertificate = value;
                    break;
                
                case "sslkey":
                    builder.SslKey = value;
                    break;
                
                default:
                    throw new ArgumentException($"Connection property '{key}' is unsupported. Use a connection string instead.");
            }
        }

        return builder;
    }

    private static InvalidOperationException MissingRequiredOption(string property)
    {
        return new InvalidOperationException($"Required value missing from Postgres configuration: '{property}.'");
    }
}