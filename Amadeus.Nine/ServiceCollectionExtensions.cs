using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Amadeus.Nine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmadeusClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = services
            .Configure<AmadeusOptions>(configuration.GetSection(AmadeusOptions.SectionName))
            .AddLogging()
            .AddHttpClient<AmadeusClient>();
        // todo: configure the client message handler to deal with tokens

        return services;
    }
}
