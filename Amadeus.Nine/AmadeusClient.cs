using Amadeus.Nine.Options;
using Microsoft.Extensions.Logging;

namespace Amadeus.Nine;

public sealed class AmadeusClient(
    HttpClient httpClient,
    AmadeusOptions options,
    ILogger<AmadeusClient> logger)
{

}
