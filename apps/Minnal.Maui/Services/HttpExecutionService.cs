using System.Diagnostics;

namespace Minnal.Maui.Services;

public sealed record HttpResult(
    int StatusCode,
    string Body,
    long DurationMs,
    long SizeBytes,
    bool IsError,
    string? ErrorMessage);

public interface IHttpExecutionService
{
    Task<HttpResult> SendAsync(string method, string url, string? body = null, CancellationToken ct = default);
}

public sealed class HttpExecutionService : IHttpExecutionService, IDisposable
{
    private readonly HttpClient _client = new(new HttpClientHandler { AllowAutoRedirect = true })
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public async Task<HttpResult> SendAsync(string method, string url, string? body = null, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            request.Headers.UserAgent.ParseAdd("Minnal/0.1 (spike)");
            if (body is not null)
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            sw.Stop();

            return new HttpResult(
                (int)response.StatusCode,
                responseBody,
                sw.ElapsedMilliseconds,
                System.Text.Encoding.UTF8.GetByteCount(responseBody),
                !response.IsSuccessStatusCode,
                null);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HttpResult(0, "", sw.ElapsedMilliseconds, 0, true, ex.Message);
        }
    }

    public void Dispose() => _client.Dispose();
}
