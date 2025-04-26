using Amadeus.Nine.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Amadeus.Nine.Tests.Tokens;

public sealed partial class TokensTests
{
    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Amadeus:Host", "https://test.api.amadeus.com" },
            { "Amadeus:ClientVersion", "0.0.0" },
            { "Amadeus:ClientName", "tests" },
        }!)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), true, false)
        .Build();

    // verifies fetching tokens works
    [Fact]
    public async Task ReadTokenSucceeds()
    {
        using var services = TestClient
            .AddTestClient(new ServiceCollection(), configuration)
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
        using var services = TestClient
            .AddTestClient(new ServiceCollection(), configuration)
            .BuildServiceProvider();

        var tokenProvider = services.GetRequiredService<TokenProvider>();
        Assert.NotNull(tokenProvider);

        var expected = await tokenProvider.ReadTokenAsync(CancellationToken.None);
        var actual = await tokenProvider.ReadTokenAsync(CancellationToken.None);
        Assert.Equal(expected, actual);
    }

    // verifies the token is cached
    [Fact]
    public async Task ReadTokenAvoidsStampede()
    {
        using var services = TestClient
            .AddTestClient(new ServiceCollection(), configuration)
            .BuildServiceProvider();

        var tokenProvider = services.GetRequiredService<TokenProvider>();
        Assert.NotNull(tokenProvider);

        var tasks = new Task<string>[1000];
        for (var i = 0; i < tasks.Length; ++i)
        {
            tasks[i] = tokenProvider.ReadTokenAsync(CancellationToken.None).AsTask();
        }

        var results = await Task.WhenAll(tasks);
        var token = Assert.Single(results.Distinct());
        Assert.NotNull(token);
    }

    [Fact]
    public async Task AuthHeaderIsAdded()
    {
        using var services = TestClient
            .AddTestClient(new ServiceCollection(), configuration)
            .BuildServiceProvider();

        var client = services.GetRequiredService<TestClient>();
        await client.TestAsync(CancellationToken.None);
        var verifier = services.GetRequiredService<AuthTokenVerifier>();
        Assert.True(verifier.HasToken);
    }
}
