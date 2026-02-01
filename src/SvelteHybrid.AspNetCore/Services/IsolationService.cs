using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SvelteHybrid.AspNetCore.Services;

/// <summary>
/// Default implementation of IIsolationService.
/// </summary>
public partial class IsolationService : IIsolationService
{
    private readonly SvelteHybridOptions _options;
    private readonly ILogger<IsolationService> _logger;
    private readonly Dictionary<string, string> _hashCache = new();
    private readonly object _cacheLock = new();

    private static readonly Regex CssClassRegex = CssClassPattern();
    private static readonly Regex HtmlClassRegex = HtmlClassPattern();

    public IsolationService(
        IOptions<SvelteHybridOptions> options,
        ILogger<IsolationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string ApplyIsolation(string componentHtml, string componentName)
    {
        if (_options.IsolationMode == IsolationMode.None)
        {
            return componentHtml;
        }

        var hash = GenerateComponentHash(componentName);
        var result = componentHtml;

        // Always apply CSS class isolation
        result = IsolateHtmlClasses(result, hash);

        // Add isolation scope attribute
        result = AddScopeAttribute(result, hash);

        _logger.LogDebug(
            "Applied {Mode} isolation to {Component} with hash {Hash}",
            _options.IsolationMode,
            componentName,
            hash);

        return result;
    }

    /// <inheritdoc/>
    public string GenerateComponentHash(string componentName)
    {
        lock (_cacheLock)
        {
            if (_hashCache.TryGetValue(componentName, out var existingHash))
            {
                return existingHash;
            }

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(componentName));
            var hash = Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .ToLowerInvariant()[..8];

            _hashCache[componentName] = hash;
            return hash;
        }
    }

    /// <inheritdoc/>
    public string IsolateCSS(string css, string hash)
    {
        return CssClassRegex.Replace(css, match =>
        {
            var className = match.Groups[1].Value;
            return $".{hash}_{className}";
        });
    }

    /// <inheritdoc/>
    public string IsolateJavaScript(string js, string hash)
    {
        if (_options.IsolationMode != IsolationMode.Full)
        {
            return js;
        }

        // Scope document.querySelector calls
        var scopedJs = js
            .Replace(
                "document.querySelector(",
                $"document.querySelector('[data-svelte-scope=\"{hash}\"]').querySelector(")
            .Replace(
                "document.querySelectorAll(",
                $"document.querySelector('[data-svelte-scope=\"{hash}\"]').querySelectorAll(");

        return scopedJs;
    }

    /// <inheritdoc/>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _hashCache.Clear();
            _logger.LogDebug("Isolation hash cache cleared");
        }
    }

    private string IsolateHtmlClasses(string html, string hash)
    {
        return HtmlClassRegex.Replace(html, match =>
        {
            var classes = match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var isolatedClasses = classes.Select(c => $"{hash}_{c}");
            return $"class=\"{string.Join(" ", isolatedClasses)}\"";
        });
    }

    private static string AddScopeAttribute(string html, string hash)
    {
        // Find the first element and add scope attribute
        var firstTagMatch = Regex.Match(html, @"<(\w+)([^>]*)>");
        if (firstTagMatch.Success)
        {
            var tagName = firstTagMatch.Groups[1].Value;
            var attributes = firstTagMatch.Groups[2].Value;
            
            if (!attributes.Contains("data-svelte-scope"))
            {
                var newTag = $"<{tagName} data-svelte-scope=\"{hash}\"{attributes}>";
                return html.Remove(firstTagMatch.Index, firstTagMatch.Length)
                    .Insert(firstTagMatch.Index, newTag);
            }
        }

        return html;
    }

    [GeneratedRegex(@"\.([a-zA-Z][\w-]*)", RegexOptions.Compiled)]
    private static partial Regex CssClassPattern();

    [GeneratedRegex(@"class=""([^""]+)""", RegexOptions.Compiled)]
    private static partial Regex HtmlClassPattern();
}
