using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Amadeus.Nine.Tests;

public class UnitTest1
{
    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Amadeus:Host", "test.api.amadeus.com" },
            { "Amadeus:Version", "3.0.0" },
        }!)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), true, false)
        .Build();

    [Fact]
    public void ServiceRegistrationSucceeds()
    {
        using var services = new ServiceCollection()
            .AddAmadeusClient(configuration)
            .BuildServiceProvider();

        var client = services.GetRequiredService<AmadeusClient>();
        Assert.NotNull(client);
    }
}
