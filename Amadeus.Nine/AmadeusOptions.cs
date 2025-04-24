namespace Amadeus.Nine;

public sealed record AmadeusOptions(
    Uri Host,
    Version Version,
    AmadeusCredentials Credentials);
