using Amadeus.Nine.Options;
using Amadeus.Nine.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Amadeus.Nine.Tests.Tokens.TokensTests;

namespace Amadeus.Nine.Tests.Tokens;

internal sealed class TestClient(HttpClient httpClient)
{
    public async Task TestAsync(CancellationToken cancellationToken) =>
        _ = await httpClient.GetStringAsync("path", cancellationToken);

    internal static IServiceCollection AddTestClient(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetRequiredSection(AmadeusOptions.SectionName)
            .Get<AmadeusOptions>()
            ?? throw new InvalidOperationException($"can't read {nameof(AmadeusOptions)} in section {AmadeusOptions.SectionName}");

        var credentials = configuration
            .GetRequiredSection(nameof(AmadeusCredentials))
            .Get<AmadeusCredentials>()
            ?? throw new InvalidOperationException($"can't read {nameof(AmadeusCredentials)} in section {nameof(AmadeusCredentials)}");

        _ = services
            .AddSingleton(options)
            .AddSingleton(credentials)
            .AddTransient<AuthTokenHandler>()
            .AddSingleton<AuthTokenVerifier>();

        _ = services.AddHttpClient<TokenProvider>(client => client.BaseAddress = options.Host);
        _ = services.AddHttpClient<TestClient>(client => client.BaseAddress = options.Host)
            .AddHttpMessageHandler<AuthTokenHandler>()
            .AddHttpMessageHandler<AuthTokenVerifier>();

        return services;
    }
}

