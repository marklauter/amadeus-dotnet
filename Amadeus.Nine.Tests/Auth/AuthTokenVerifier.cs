namespace Amadeus.Nine.Tests.Auth;

internal sealed class AuthTokenVerifier
    : DelegatingHandler
{
    public bool HasToken { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HasToken = request.Headers.Authorization is not null;
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("test response")
        });
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HasToken = request.Headers.Authorization is not null;
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("test response")
        };
    }
}
