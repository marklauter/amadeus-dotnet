namespace Amadeus.Nine;

public sealed record AmadeusOptions(
    Uri Host,
    Version Version,
    AmadeusCredentials Credentials)
{
    public const string SectionName = "Amadeus";
}
