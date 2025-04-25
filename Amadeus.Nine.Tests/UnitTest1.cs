using Amadeus.Nine.Options;
using Amadeus.Nine.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Amadeus.Nine.Tests;

public class UnitTest1
{
    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Amadeus:Host", "https://test.api.amadeus.com" },
            { "Amadeus:ClientVersion", "0.0.0" },
            { "Amadeus:ClientName", "TWAI" },
        }!)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), true, false)
        .Build();

    // verifies the amadeus client is registered correctly
    [Fact]
    public void ServiceRegistrationSucceeds()
    {
        using var services = new ServiceCollection()
            .AddAmadeusClient(configuration)
            .BuildServiceProvider();

        var client = services.GetRequiredService<AmadeusClient>();
        Assert.NotNull(client);
    }

    // verifies fetching tokens works
    [Fact]
    public async Task ReadTokenSucceeds()
    {
        using var services = new ServiceCollection()
            .AddAmadeusClient(configuration)
            .BuildServiceProvider();

        var tokenProvider = services.GetRequiredService<TokenProvider>();
        Assert.NotNull(tokenProvider);

        var token = await tokenProvider.ReadTokenAsync(CancellationToken.None);
        Assert.NotEmpty(token);
    }

    // verifies the token is cached
    [Fact]
    public async Task ReadTokenTwiceReturnsSameValue()
    {
        using var services = new ServiceCollection()
            .AddAmadeusClient(configuration)
            .BuildServiceProvider();

        var tokenProvider = services.GetRequiredService<TokenProvider>();
        Assert.NotNull(tokenProvider);

        var expected = await tokenProvider.ReadTokenAsync(CancellationToken.None);
        var actual = await tokenProvider.ReadTokenAsync(CancellationToken.None);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("https://test.api.amadeus.com/", "v1/security/oauth2/token")]
    [InlineData("https://test.api.amadeus.com", "/v1/security/oauth2/token")]
    [InlineData("https://test.api.amadeus.com/", "/v1/security/oauth2/token")]
    [InlineData("https://test.api.amadeus.com", "v1/security/oauth2/token")]
    public void UriBuildingWorksAsExpected(string host, string tokenPath)
    {
        var uri = new Uri(new Uri(host), tokenPath);
        Assert.Equal("https://test.api.amadeus.com/v1/security/oauth2/token", uri.ToString());
    }

    [Fact]
    public async Task AuthHeaderIsAdded()
    {
        using var services = AddAmadeusClientForTest(new ServiceCollection(), configuration)
            .BuildServiceProvider();

        var client = services.GetRequiredService<AmadeusClient>();
        await client.PingAsync(CancellationToken.None);
        var verifier = services.GetRequiredService<AuthTokenVerifier>();
        Assert.True(verifier.HasToken);
    }

    private sealed class AuthTokenVerifier
        : DelegatingHandler
    {
        public bool HasToken { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HasToken = request.Headers.Authorization is not null;
            return await base.SendAsync(request, cancellationToken);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HasToken = request.Headers.Authorization is not null;
            return base.Send(request, cancellationToken);
        }
    }

    private static IServiceCollection AddAmadeusClientForTest(
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
        _ = services.AddHttpClient<AmadeusClient>()
            .AddHttpMessageHandler<AuthTokenHandler>()
            .AddHttpMessageHandler<AuthTokenVerifier>();

        return services;
    }
}
