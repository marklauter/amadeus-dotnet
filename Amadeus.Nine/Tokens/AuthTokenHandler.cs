using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Amadeus.Nine.Tokens;

internal sealed class AuthTokenHandler(TokenProvider tokenProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetAuthTokenAsync(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthTokenAsync(request, cancellationToken).Wait(cancellationToken);
        return base.Send(request, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task SetAuthTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenProvider.ReadTokenAsync(cancellationToken));
}
