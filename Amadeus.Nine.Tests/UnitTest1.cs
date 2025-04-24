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
}
