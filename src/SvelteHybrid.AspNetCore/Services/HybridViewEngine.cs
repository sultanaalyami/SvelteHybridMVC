using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Text.RegularExpressions;

namespace SvelteHybrid.AspNetCore.Services;

/// <summary>
/// Default implementation of IHybridViewEngine.
/// </summary>
public partial class HybridViewEngine : IHybridViewEngine
{
    private readonly ISvelteRenderer _svelteRenderer;
    private readonly IIsolationService _isolationService;
    private readonly SvelteHybridOptions _options;
    private readonly ILogger<HybridViewEngine> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Regex pattern for @svelte directives
    private static readonly Regex DirectiveRegex = SvelteDirectiveRegex();

    public HybridViewEngine(
        ISvelteRenderer svelteRenderer,
        IIsolationService isolationService,
        IOptions<SvelteHybridOptions> options,
        ILogger<HybridViewEngine> logger,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _svelteRenderer = svelteRenderer;
        _isolationService = isolationService;
        _options = options.Value;
        _logger = logger;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<string> RenderHybridAsync(
        string razorHtml, 
        object? model = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var directives = FindSvelteDirectives(razorHtml).ToList();

            if (directives.Count == 0)
            {
                _logger.LogDebug("No Svelte directives found");
                return razorHtml;
            }

            _logger.LogDebug("Found {Count} Svelte directives", directives.Count);

            var resultHtml = razorHtml;

            // Process directives in reverse order to maintain positions
            foreach (var directive in directives.OrderByDescending(d => d.Position))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check authorization
                if (!string.IsNullOrEmpty(directive.Policy))
                {
                    var authorized = await CheckAuthorizationAsync(directive.Policy);
                    if (!authorized)
                    {
                        var fallbackHtml = await RenderFallbackAsync(cancellationToken);
                        resultHtml = ReplaceDirectiveWithComponent(resultHtml, directive, fallbackHtml);
                        continue;
                    }
                }

                // Prepare and render component
                var componentData = PrepareComponentData(directive, model);
                var svelteHtml = await _svelteRenderer.RenderAsync(
                    directive.ComponentName,
                    componentData,
                    cancellationToken);

                resultHtml = ReplaceDirectiveWithComponent(resultHtml, directive, svelteHtml);
            }

            return resultHtml;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Hybrid rendering was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in hybrid rendering");
            throw;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<SvelteDirective> FindSvelteDirectives(string razorHtml)
    {
        var matches = DirectiveRegex.Matches(razorHtml);

        foreach (Match match in matches)
        {
            var componentName = match.Groups["component"].Value;
            var policy = match.Groups["policy"].Success ? match.Groups["policy"].Value : null;
            var data = match.Groups["data"].Success ? match.Groups["data"].Value : null;

            yield return new SvelteDirective(
                ComponentName: componentName,
                Position: match.Index,
                Length: match.Length,
                Policy: policy,
                Data: data
            );
        }
    }

    /// <inheritdoc/>
    public object PrepareComponentData(SvelteDirective directive, object? model)
    {
        return new
        {
            componentName = directive.ComponentName,
            model,
            policy = directive.Policy,
            data = directive.Data
        };
    }

    /// <inheritdoc/>
    public string ReplaceDirectiveWithComponent(
        string razorHtml, 
        SvelteDirective directive, 
        string componentHtml)
    {
        return string.Concat(
            razorHtml.AsSpan(0, directive.Position),
            componentHtml,
            razorHtml.AsSpan(directive.Position + directive.Length)
        );
    }

    private async Task<bool> CheckAuthorizationAsync(string policy)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            _logger.LogWarning("No user context for policy {Policy}", policy);
            return false;
        }

        var authResult = await _authorizationService.AuthorizeAsync(user, policy);
        
        if (!authResult.Succeeded)
        {
            _logger.LogDebug("Authorization failed for policy {Policy}", policy);
        }
        
        return authResult.Succeeded;
    }

    private async Task<string> RenderFallbackAsync(CancellationToken cancellationToken)
    {
        var fallbackComponent = _options.Authorization.FallbackComponent;
        
        if (_svelteRenderer.ComponentExists(fallbackComponent))
        {
            return await _svelteRenderer.RenderAsync(fallbackComponent, null, cancellationToken);
        }

        return "<!-- Unauthorized -->";
    }

    [GeneratedRegex(
        @"@svelte\s+""(?<component>[^""]+)""(?:\s+@require-policy\s+""(?<policy>[^""]+)"")?(?:\s+@data=""(?<data>[^""]+)"")?",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex SvelteDirectiveRegex();
}
