using Microsoft.Extensions.Options;
using System.Text.Json;
using HRCE.Services.Node;

namespace HRCE.Services.HRCE;

/// <summary>
/// عارض Svelte - مسؤول عن SSR للمكونات
/// </summary>
public class SvelteRenderer : ISvelteRenderer
{
    private readonly HRCEOptions _options;
    private readonly ILogger<SvelteRenderer> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly INodeService _nodeService;

    public SvelteRenderer(
        IOptions<HRCEOptions> options,
        ILogger<SvelteRenderer> logger,
        IWebHostEnvironment env,
        INodeService nodeService)
    {
        _options = options.Value;
        _logger = logger;
        _env = env;
        _nodeService = nodeService;
    }

    public async Task<string> RenderAsync(string componentName, object data)
    {
        if (!_options.EnableSvelteSSR)
        {
            _logger.LogWarning("Svelte SSR is disabled, returning placeholder");
            return $"<!-- Svelte component: {componentName} (SSR disabled) -->";
        }

        try
        {
            _logger.LogInformation("Rendering Svelte component: {ComponentName}", componentName);

            // استدعاء خدمة Node.js للعرض الفعلي
            var renderResult = await _nodeService.RenderComponentAsync(componentName, data);

            // دمج النتيجة مع بيانات الترطيب (Hydration)
            var html =
                $"<div data-svelte-component=\"{componentName}\">" +
                renderResult.Html +
                $"<script type=\"application/json\" data-component-props>{JsonSerializer.Serialize(data)}</script>" +
                (string.IsNullOrEmpty(renderResult.Css) ? "" : $"<style>{renderResult.Css}</style>") +
                $"</div>";

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering Svelte component: {ComponentName}", componentName);
            return $"<!-- Error rendering {componentName}: {ex.Message} -->";
        }
    }

    public bool ComponentExists(string componentName)
    {
        var componentPath = Path.Combine(
            _env.ContentRootPath,
            "Components",
            componentName,
            $"{componentName}.svelte");

        return File.Exists(componentPath);
    }
}
