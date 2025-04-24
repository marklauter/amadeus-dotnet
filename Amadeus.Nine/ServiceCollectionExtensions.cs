using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Amadeus.Nine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmadeusClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        _ = services.AddHttpClient<AmadeusClient>();
        _ = services.AddHttpClient<TokenProvider>();
        // todo: configure the client message handler to deal with tokens

        var options = configuration
            .GetRequiredSection(AmadeusOptions.SectionName)
            .Get<AmadeusOptions>()
            ?? throw new InvalidOperationException($"can't read {nameof(AmadeusOptions)} in section {AmadeusOptions.SectionName}");

        return services
            .Configure<AmadeusOptions>(configuration.GetSection(AmadeusOptions.SectionName))
            .AddSingleton(options.Credentials)
            .AddLogging();
    }
}
