using Amadeus.Nine.Locks;
using Amadeus.Nine.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amadeus.Nine.Tokens;

internal sealed class TokenProvider(
    HttpClient httpClient,
    AmadeusOptions options,
    AmadeusCredentials credentials)
{
    private sealed record CachedToken(
        string Token,
        DateTime Expires)
    {
        private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

        public bool IsExpired => Expires >= DateTime.UtcNow;

        public static CachedToken From(string token, int expiresIn) =>
            new(token, DateTime.UtcNow - (TimeSpan.FromSeconds(expiresIn) + ClockSkew));
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("type")]
        string Type,
        [property: JsonPropertyName("username")]
        string UserName,
        [property: JsonPropertyName("application_name")]
        string ApplicationName,
        [property: JsonPropertyName("client_id")]
        string CliendId,
        [property: JsonPropertyName("token_type")]
        string TokenType,
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("state")]
        // todo: state should be enum
        string State,
        [property: JsonPropertyName("scope")]
        string Scope);

    private const string GrantType = "client_credentials";
    public const string TokenEndPoint = "/v1/security/oauth2/token";
    private readonly Uri tokenEndpoint = new(options.Host, TokenEndPoint);

    private static readonly AsyncLock Gate = new();
    private static CachedToken? Token;

    public async ValueTask<string> ReadTokenAsync(CancellationToken cancellationToken) =>
        Token is not null && !Token.IsExpired
            ? Token.Token
            : await Gate.WithLockAsync(async (cancellationToken) => Token is not null && !Token.IsExpired ? Token.Token : await RequestTokenAsync(cancellationToken),
            cancellationToken);

    private async Task<string> RequestTokenAsync(CancellationToken cancellationToken)
    {
        using var message = BuildMessage(CreateContent());
        using var response = await httpClient
            .SendAsync(message, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var tokenResponse = !response.IsSuccessStatusCode
            ? throw new InvalidOperationException($"failed getting token {response.ReasonPhrase}, {content}")
            : JsonSerializer.Deserialize<TokenResponse>(content)
            ?? throw new InvalidOperationException("could not read token response from response.content");

        Token = CachedToken.From(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

        return Token.Token;
    }

    private FormUrlEncodedContent CreateContent() =>
        // todo: replace red text with consts
        new(
        [
            new ("grant_type", GrantType),
            new ("client_id", credentials.ApiKey),
            new ("client_secret", credentials.ApiSecret)
        ]);

    private HttpRequestMessage BuildMessage(FormUrlEncodedContent content)
    {
        // todo: replace red text with consts
        var message = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("TWAI", options.ClientVersion.ToString()));
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("dotnet", "9"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //if (token is not null)
        //    message.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        message.Content = content;
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        return message;
    }
}
