using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HRCE.Services.HRCE;

/// <summary>
/// خدمة العزل - تطبق العزل على CSS/JS/Runtime
/// </summary>
public class IsolationService : IIsolationService
{
    private readonly HRCEOptions _options;
    private readonly ILogger<IsolationService> _logger;
    private readonly Dictionary<string, string> _componentHashes = new();

    public IsolationService(
        IOptions<HRCEOptions> options,
        ILogger<IsolationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string ApplyIsolation(string componentHtml, string componentName)
    {
        if (_options.IsolationMode == IsolationMode.None)
        {
            return componentHtml;
        }

        var hash = GenerateComponentHash(componentName);
        var isolatedHtml = componentHtml;

        // عزل CSS classes
        isolatedHtml = IsolateCSSClasses(isolatedHtml, hash);

        // عزل data attributes
        isolatedHtml = AddIsolationAttributes(isolatedHtml, hash);

        _logger.LogDebug("Applied isolation to component: {ComponentName} with hash: {Hash}",
            componentName, hash);

        return isolatedHtml;
    }

    public string GenerateComponentHash(string componentName)
    {
        if (_componentHashes.TryGetValue(componentName, out var existingHash))
        {
            return existingHash;
        }

        // توليد hash قصير وفريد
        var hash = Convert.ToBase64String(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(componentName)))
            .Substring(0, 8)
            .Replace("+", "")
            .Replace("/", "")
            .ToLower();

        _componentHashes[componentName] = hash;
        return hash;
    }

    public string IsolateCSS(string css, string hash)
    {
        // إضافة hash لكل CSS class
        var regex = new Regex(@"\.([a-zA-Z][\w-]*)");
        return regex.Replace(css, $".{hash}_$1");
    }

    public string IsolateJavaScript(string js, string hash)
    {
        // تقييد الوصول للـ selectors
        var scopedJs = js.Replace(
            "document.querySelector",
            $"document.querySelector('[data-hrce-scope=\"{hash}\"]').querySelector");

        return scopedJs;
    }

    private string IsolateCSSClasses(string html, string hash)
    {
        // إضافة hash لـ CSS classes في HTML
        var regex = new Regex(
            @"class=""([^""]+)""");

        return regex.Replace(html, match =>
        {
            var classes = match.Groups[1].Value.Split(' ');
            var isolatedClasses = classes.Select(c => $"{hash}_{c}");
            return $"class=\"{string.Join(" ", isolatedClasses)}\"";
        });
    }

    private string AddIsolationAttributes(string html, string hash)
    {
        // إضافة data-hrce-scope للعنصر الأساسي
        return html.Replace(
            "<div",
            $"<div data-hrce-scope=\"{hash}\"",
            StringComparison.OrdinalIgnoreCase);
    }
}
