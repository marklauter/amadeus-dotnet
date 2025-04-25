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

        public bool HasNotExpired => Expires > DateTime.UtcNow;

        public static CachedToken Create(string token, int expiresInSeconds)
        {
            var expireSpan = TimeSpan.FromSeconds(expiresInSeconds) - ClockSkew;
            return new(
                token,
                DateTime.UtcNow.Add(expireSpan.Ticks > 0
                    ? expireSpan
                    : TimeSpan.Zero));
        }
    }

    private enum TokenState
    {
        Undefined,
        Approved,
        Expired,
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

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
        int ExpiresInSeconds,
        [property: JsonPropertyName("state")]
        TokenState State,
        [property: JsonPropertyName("scope")]
        string Scope);

    private const string GrantType = "client_credentials";
    private const string TokenPath = "/v1/security/oauth2/token";
    private const string GrantTypeKey = "grant_type";
    private const string ClientIdKey = "client_id";
    private const string ClientSecretKey = "client_secret";
    private const string ContentTypeFormUrlEncoded = "application/x-www-form-urlencoded";

    private static readonly AsyncLock Gate = new();
    private static CachedToken? Token;

    private readonly Uri tokenEndpoint = new(options.Host, TokenPath);

    public async ValueTask<string> ReadTokenAsync(CancellationToken cancellationToken) =>
        TokenIsReady()
            ? Token!.Token
            : await Gate.WithLockAsync(async (cancellationToken) =>
                TokenIsReady()
                    ? Token!.Token
                    : await RequestTokenAsync(cancellationToken),
                cancellationToken);

    private static bool TokenIsReady() => Token is not null && Token.HasNotExpired;

    private async Task<string> RequestTokenAsync(CancellationToken cancellationToken)
    {
        using var message = BuildMessage(
            CreateContent());

        using var response = await httpClient
            .SendAsync(message, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var tokenResponse = !response.IsSuccessStatusCode
            ? throw new InvalidOperationException($"failed getting token from '{tokenEndpoint}', code: {response.StatusCode}, reason: {response.ReasonPhrase}, content: {content}")
            : JsonSerializer.Deserialize<TokenResponse>(content, JsonOptions)
                ?? throw new InvalidOperationException($"could not deserialize token response from '{tokenEndpoint}', content: {content}");

        Token = CachedToken.Create(tokenResponse.AccessToken, tokenResponse.ExpiresInSeconds);

        return Token.Token;
    }

    private FormUrlEncodedContent CreateContent()
    {
        var content = new FormUrlEncodedContent(
            [
                new (GrantTypeKey, GrantType),
                new (ClientIdKey, credentials.ApiKey),
                new (ClientSecretKey, credentials.ApiSecret)
            ]);

        content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeFormUrlEncoded);

        return content;
    }

    private HttpRequestMessage BuildMessage(FormUrlEncodedContent content)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = content,
        };

        message.Headers.UserAgent.Add(new ProductInfoHeaderValue(options.ClientName, options.ClientVersion.ToString()));
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("dotnet", "9"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.amadeus+json"));
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return message;
    }
}
