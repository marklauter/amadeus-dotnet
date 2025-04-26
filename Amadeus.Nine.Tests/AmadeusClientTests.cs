using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Amadeus.Nine.Tests;

public class AmadeusClientTests
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

    // proving copilot wrong
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
    public async Task FlightInspiration()
    {
        using var services = new ServiceCollection()
            .AddAmadeusClient(configuration)
            .BuildServiceProvider();

        var client = services.GetRequiredService<AmadeusClient>();
        var r = await client.ReadFlightInspirationAsync("PAR", CancellationToken.None);
        Assert.NotNull(r);
    }
}
