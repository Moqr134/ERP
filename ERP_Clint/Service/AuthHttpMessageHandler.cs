using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;

namespace ERP_Clint.Service;
public class AuthHttpMessageHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnsureBufferedContentAsync(request);
        ApplyAuthHeaders(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || IsAuthEndpoint(request))
            return response;

        if (!await TryRefreshAsync(cancellationToken))
            return response;

        response.Dispose();
        var retry = await CloneRequestAsync(request);
        ApplyAuthHeaders(retry);
        return await base.SendAsync(retry, cancellationToken);
    }

    private static void ApplyAuthHeaders(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        if (!request.Headers.Contains("X-Requested-With"))
            request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
    }

    private static async Task EnsureBufferedContentAsync(HttpRequestMessage request)
    {
        if (request.Content == null) return;
        var bytes = await request.Content.ReadAsByteArrayAsync();
        var buffered = new ByteArrayContent(bytes);
        foreach (var header in request.Content.Headers)
            buffered.Headers.TryAddWithoutValidation(header.Key, header.Value);
        request.Content = buffered;
    }

    private static bool IsAuthEndpoint(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        return path.Contains("/api/Account/login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/Account/Login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/Account/refresh-token", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/account/userinfo", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/Account/refresh-token");
            ApplyAuthHeaders(refreshRequest);
            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
            return refreshResponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
