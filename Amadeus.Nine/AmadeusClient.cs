using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amadeus.Nine;

public sealed class AmadeusClient(
    HttpClient httpClient,
    IOptions<AmadeusOptions> options,
    ILogger<AmadeusClient> logger)
{
    public const string ApiVersion = "3.0.0";

}
