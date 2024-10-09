using Microsoft.Extensions.DependencyInjection;

namespace Vertical.Migrate.Engine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMigrateEngine(this IServiceCollection services, RuntimeOptions options)
    {
        services
            .AddSingleton<IMigrationEngine, MigrationEngine>()
            .AddSingleton<ISourceFileProvider, SourceFileProvider>()
            .AddSingleton<ISourceFileProcessor, SourceFileProcessor>()
            .AddSingleton(options);
        
        return services;
    }
}