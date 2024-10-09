using Microsoft.Extensions.DependencyInjection;
using Vertical.Migrate.Postgres.Postgres;

namespace Vertical.Migrate.Postgres;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresProvider(this IServiceCollection services)
    {
        services.AddSingleton(sp => PostgresProviderFactory.GetInstance(sp.GetRequiredService<RuntimeOptions>()));
        
        return services;
    }
}