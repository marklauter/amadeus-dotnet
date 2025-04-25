using System.ComponentModel.DataAnnotations;

namespace Amadeus.Nine.Options;

public sealed record AmadeusCredentials(
    [Required] string ApiKey,
    [Required] string ApiSecret);
