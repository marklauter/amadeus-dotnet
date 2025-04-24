using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amadeus.Nine;

internal sealed class TokenProvider(HttpClient httpClient, AmadeusCredentials credentials)
{
    private const string GrantType = "client_credentials";

    // todo: cache results

    public string ReadToken() => String.Empty;
}

public sealed class AmadeusClient(
    HttpClient httpClient,
    IOptions<AmadeusOptions> options,
    ILogger<AmadeusClient> logger)
{
    private readonly IOptions<AmadeusOptions> options = options ?? throw new ArgumentNullException(nameof(options));
}
