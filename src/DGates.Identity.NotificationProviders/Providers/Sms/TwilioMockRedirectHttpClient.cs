using System.Net.Http.Headers;
using Twilio.Http;
using HttpMethod = Twilio.Http.HttpMethod;

namespace DGates.Identity.NotificationProviders.Providers.Sms;

/// <summary>
/// Redirects Twilio SDK requests to a Twilio-compatible mock server instead
/// of api.twilio.com, for local SMS 2FA testing (see
/// <see cref="Options.Sms.TwilioSmsOptions.BaseUrlOverride"/>).
/// </summary>
/// <remarks>
/// The Twilio SDK has no supported way to override its API host: the
/// <c>TwilioRestClient</c> constructor takes no base-URL parameter, and
/// <c>Request</c> always builds a "*.twilio.com" URL internally. Implementing
/// <see cref="Twilio.Http.HttpClient"/> directly lets us read the path/query
/// Twilio already built (<see cref="Request.Uri"/>) and replay it against a
/// different host, without touching the generated resource classes
/// (e.g. <c>MessageResource.CreateAsync</c>).
/// </remarks>
public class TwilioMockRedirectHttpClient : Twilio.Http.HttpClient
{
    private readonly string _mockBaseUrl;
    private readonly System.Net.Http.HttpClient _inner = new();

    /// <summary>Creates the client, redirecting all requests to <paramref name="mockBaseUrl"/>.</summary>
    public TwilioMockRedirectHttpClient(string mockBaseUrl)
    {
        _mockBaseUrl = mockBaseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public override Response MakeRequest(Request request) => MakeRequestAsync(request).GetAwaiter().GetResult();

    /// <inheritdoc />
    public override async Task<Response> MakeRequestAsync(Request request)
    {
        var redirectedUri = new Uri(_mockBaseUrl + request.Uri!.PathAndQuery);

        var httpRequest = new HttpRequestMessage(new System.Net.Http.HttpMethod(request.Method.ToString()), redirectedUri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Authentication(request.Username, request.Password));

        if (request.Method == HttpMethod.Post)
        {
            httpRequest.Content = new FormUrlEncodedContent(request.PostParams);
        }

        var response = await _inner.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();
        return new Response(response.StatusCode, content);
    }
}
