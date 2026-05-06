using Microsoft.Extensions.DependencyInjection;

namespace OpenPdc.Adapter;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMigrationService(
        this IServiceCollection services,
        Action<MigrationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new MigrationOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddTransient<IMigrationService, MigrationService>();

        return services;
    }
}
