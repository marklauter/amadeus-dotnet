using Amadeus.Nine.Options;
using Microsoft.Extensions.Logging;

namespace Amadeus.Nine;

public sealed class AmadeusClient(
    HttpClient httpClient,
    AmadeusOptions options,
    ILogger<AmadeusClient> logger)
{
    public async Task Ping(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://microsoft.com");
        using var response = await httpClient.SendAsync(request, cancellationToken);
    }
}
