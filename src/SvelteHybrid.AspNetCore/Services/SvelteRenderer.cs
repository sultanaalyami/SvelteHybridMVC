using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Text.Json;

namespace SvelteHybrid.AspNetCore.Services;

/// <summary>
/// Default implementation of ISvelteRenderer.
/// </summary>
public class SvelteRenderer : ISvelteRenderer
{
    private readonly SvelteHybridOptions _options;
    private readonly ILogger<SvelteRenderer> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly INodeService _nodeService;
    private readonly IMemoryCache _cache;
    private readonly IIsolationService _isolationService;

    public SvelteRenderer(
        IOptions<SvelteHybridOptions> options,
        ILogger<SvelteRenderer> logger,
        IWebHostEnvironment env,
        INodeService nodeService,
        IMemoryCache cache,
        IIsolationService isolationService)
    {
        _options = options.Value;
        _logger = logger;
        _env = env;
        _nodeService = nodeService;
        _cache = cache;
        _isolationService = isolationService;
    }

    /// <inheritdoc/>
    public async Task<string> RenderAsync(
        string componentName, 
        object? props = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableSvelteSSR)
        {
            _logger.LogDebug("SSR disabled, returning client-side placeholder for {Component}", componentName);
            return GenerateClientSidePlaceholder(componentName, props);
        }

        // Check cache
        var cacheKey = GenerateCacheKey(componentName, props);
        if (_options.EnableCaching && _cache.TryGetValue(cacheKey, out string? cachedHtml) && cachedHtml != null)
        {
            _logger.LogDebug("Cache hit for component {Component}", componentName);
            return cachedHtml;
        }

        try
        {
            _logger.LogDebug("Rendering Svelte component: {Component}", componentName);

            var renderResult = await _nodeService.RenderComponentAsync(
                componentName, 
                props,
                cancellationToken);

            var html = BuildComponentHtml(componentName, renderResult, props);

            // Apply isolation
            html = _isolationService.ApplyIsolation(html, componentName);

            // Cache result
            if (_options.EnableCaching)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.CacheDurationMinutes));
                _cache.Set(cacheKey, html, cacheOptions);
            }

            if (_options.Development.LogRenders)
            {
                _logger.LogInformation(
                    "Rendered {Component} in {Time}ms", 
                    componentName, 
                    renderResult.RenderTimeMs);
            }

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering component {Component}", componentName);
            
            if (_options.Development.ShowDetailedErrors)
            {
                return $"<!-- Error rendering {componentName}: {ex.Message} -->";
            }
            
            return $"<!-- Component {componentName} failed to render -->";
        }
    }

    /// <inheritdoc/>
    public Task<string> RenderAsync<TProps>(
        string componentName, 
        TProps props,
        CancellationToken cancellationToken = default) where TProps : class
    {
        return RenderAsync(componentName, props, cancellationToken);
    }

    /// <inheritdoc/>
    public bool ComponentExists(string componentName)
    {
        var componentPath = GetComponentPath(componentName);
        return File.Exists(componentPath);
    }

    /// <inheritdoc/>
    public string GetComponentPath(string componentName)
    {
        var basePath = _options.ComponentBasePath.Replace("~/", "");
        return Path.Combine(
            _env.ContentRootPath,
            basePath,
            componentName,
            $"{componentName}.svelte");
    }

    private string GenerateCacheKey(string componentName, object? props)
    {
        var propsHash = props != null 
            ? JsonSerializer.Serialize(props).GetHashCode() 
            : 0;
        return $"svelte_{componentName}_{propsHash}";
    }

    private string BuildComponentHtml(
        string componentName, 
        NodeRenderResult renderResult, 
        object? props)
    {
        var propsJson = props != null 
            ? JsonSerializer.Serialize(props) 
            : "{}";

        return $"""
            <div data-svelte-component="{componentName}" data-svelte-hydrate="true">
                {renderResult.Html}
                <script type="application/json" data-component-props>{propsJson}</script>
                {(string.IsNullOrEmpty(renderResult.Css) ? "" : $"<style data-component-styles>{renderResult.Css}</style>")}
            </div>
            """;
    }

    private string GenerateClientSidePlaceholder(string componentName, object? props)
    {
        var propsJson = props != null 
            ? JsonSerializer.Serialize(props) 
            : "{}";

        return $"""
            <div data-svelte-component="{componentName}" data-svelte-hydrate="true" data-svelte-ssr="false">
                <script type="application/json" data-component-props>{propsJson}</script>
            </div>
            """;
    }
}
