using Amadeus.Nine.Options;
using Amadeus.Nine.Tokens;
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

        var credentials = configuration
            .GetRequiredSection(nameof(AmadeusCredentials))
            .Get<AmadeusCredentials>()
            ?? throw new InvalidOperationException($"can't read {nameof(AmadeusCredentials)} in section {nameof(AmadeusCredentials)}");

        return services
            .AddSingleton(options)
            .AddSingleton(credentials)
            .AddLogging();
    }
}
