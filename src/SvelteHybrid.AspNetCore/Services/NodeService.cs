using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SvelteHybrid.AspNetCore.Services;

/// <summary>
/// Default implementation of INodeService for Node.js SSR communication.
/// </summary>
public class NodeService : INodeService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SvelteHybridOptions _options;
    private readonly ILogger<NodeService> _logger;
    private bool _disposed;

    public NodeService(
        HttpClient httpClient,
        IOptions<SvelteHybridOptions> options,
        ILogger<NodeService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.NodeServerUrl);
        _httpClient.Timeout = TimeSpan.FromMilliseconds(_options.SSRTimeoutMs);
    }

    /// <inheritdoc/>
    public async Task<NodeRenderResult> RenderComponentAsync(
        string componentName, 
        object? props = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var payload = new
            {
                component = componentName,
                props = props ?? new { }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/render",
                payload,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NodeRenderResponse>(
                    cancellationToken: cancellationToken);

                stopwatch.Stop();

                return new NodeRenderResult(
                    Html: result?.Html ?? "",
                    Css: result?.Css?.Code ?? "",
                    Head: result?.Head ?? "",
                    RenderTimeMs: stopwatch.Elapsed.TotalMilliseconds
                );
            }

            _logger.LogWarning(
                "Node SSR returned {StatusCode} for {Component}",
                response.StatusCode,
                componentName);

            return CreateFallbackResult(componentName, props, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "Node.js server not reachable, using fallback");
            return CreateFallbackResult(componentName, props, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Node SSR timeout for {Component}", componentName);
            return CreateFallbackResult(componentName, props, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<NodeServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/status", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<NodeStatusResponse>(
                    cancellationToken: cancellationToken);

                return new NodeServerStatus(
                    IsRunning: true,
                    Version: status?.Version,
                    Uptime: status?.UptimeSeconds != null 
                        ? TimeSpan.FromSeconds(status.UptimeSeconds.Value) 
                        : null,
                    ComponentsLoaded: status?.ComponentsLoaded ?? 0
                );
            }

            return new NodeServerStatus(IsRunning: false);
        }
        catch
        {
            return new NodeServerStatus(IsRunning: false);
        }
    }

    private NodeRenderResult CreateFallbackResult(
        string componentName, 
        object? props, 
        double renderTimeMs)
    {
        var propsJson = props != null 
            ? JsonSerializer.Serialize(props) 
            : "{}";

        // Fallback: client-side rendering placeholder
        var html = $"""
            <div data-svelte-fallback="true" data-component="{componentName}">
                <!-- SSR unavailable, client-side render will initialize -->
            </div>
            """;

        return new NodeRenderResult(
            Html: html,
            Css: "",
            Head: "",
            RenderTimeMs: renderTimeMs
        );
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    // Response DTOs
    private record NodeRenderResponse(
        string Html,
        CssContent? Css,
        string Head
    );

    private record CssContent(string Code, string? Map);

    private record NodeStatusResponse(
        string? Version,
        double? UptimeSeconds,
        int ComponentsLoaded
    );
}
