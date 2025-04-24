namespace Amadeus.Nine.Options;

public sealed record AmadeusOptions(
    Uri Host,
    Version ClientVersion)
{
    public const string SectionName = "Amadeus";
}
