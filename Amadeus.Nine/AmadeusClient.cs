using Amadeus.Nine.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amadeus.Nine;

public sealed class AmadeusClient(
    HttpClient httpClient,
    AmadeusOptions options,
    ILogger<AmadeusClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private const string FlightInpirationPath = "/v1/shopping/flight-destinations";
    public async Task<string> ReadFlightInspirationAsync(
        string origin,
        CancellationToken cancellationToken)
    {
        using var request = BuildRequest(
            FlightInpirationPath,
            [("origin", origin), ("maxPrice", "200")]);

        return await SendAsync(request, cancellationToken);
    }

    private async Task<string> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return !response.IsSuccessStatusCode
            ? throw new InvalidOperationException($"request failed for '{request.RequestUri}', code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {content}")
            : content;
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return !response.IsSuccessStatusCode
            ? throw new InvalidOperationException($"failed getting token from '{request.RequestUri}', code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {content}")
            : JsonSerializer.Deserialize<T>(content, JsonOptions)
                ?? throw new InvalidOperationException($"could not deserialize token response from '{request.RequestUri}', content: {content}");
    }

    private HttpRequestMessage BuildRequest(string path)
        => BuildRequest(path, null);

    private HttpRequestMessage BuildRequest(
        string path,
        (string key, string value)[]? query)
    {
        path = query != null && query.Length > 0
            ? $"{path}?{String.Join('&', query.Select(kvp => $"{Uri.EscapeDataString(kvp.key)}={Uri.EscapeDataString(kvp.value)}"))}"
            : path;

        var message = new HttpRequestMessage(HttpMethod.Get, path);
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue(options.ClientName, options.ClientVersion.ToString()));
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("dotnet", "9"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return message;
    }
}
