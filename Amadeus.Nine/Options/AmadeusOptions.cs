namespace Amadeus.Nine.Options;

public sealed record AmadeusOptions(
    Uri Host,
    Version ClientVersion,
    string ClientName)
{
    public const string SectionName = "Amadeus";
}
