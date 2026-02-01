using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HRCE.Services.HRCE;

/// <summary>
/// محرك العرض الهجين - يدمج Razor مع Svelte
/// </summary>
public class HybridViewEngine : IHybridViewEngine
{
    private readonly ISvelteRenderer _svelteRenderer;
    private readonly IIsolationService _isolationService;
    private readonly HRCEOptions _options;
    private readonly ILogger<HybridViewEngine> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HybridViewEngine(
        ISvelteRenderer svelteRenderer,
        IIsolationService isolationService,
        IOptions<HRCEOptions> options,
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

    public async Task<string> RenderHybridAsync(string razorHtml, object model)
    {
        try
        {
            // 1. البحث عن directives
            var directives = FindSvelteDirectives(razorHtml);

            if (!directives.Any())
            {
                _logger.LogDebug("No Svelte directives found, returning Razor content as-is");
                return razorHtml;
            }

            var resultHtml = razorHtml;

            // 2. معالجة كل directive
            foreach (var directive in directives.OrderByDescending(d => d.Position))
            {
                // التحقق من الصلاحيات إذا وجدت سياسة
                if (!string.IsNullOrEmpty(directive.Policy))
                {
                    var user = _httpContextAccessor.HttpContext?.User;
                    if (user == null)
                    {
                        _logger.LogWarning("Authorization failed: No user context for policy {Policy}", directive.Policy);
                        resultHtml = ReplaceDirectiveWithComponent(resultHtml, directive, "<!-- Unauthorized: No User -->");
                        continue;
                    }

                    var authResult = await _authorizationService.AuthorizeAsync(user, directive.Policy);
                    if (!authResult.Succeeded)
                    {
                        _logger.LogWarning("Authorization failed for policy {Policy}", directive.Policy);
                        
                        // عرض مكون البديل (Fallback)
                        var fallbackHtml = await _svelteRenderer.RenderAsync(_options.Authorization.FallbackComponent, new { });
                        resultHtml = ReplaceDirectiveWithComponent(resultHtml, directive, fallbackHtml);
                        continue;
                    }
                }

                // 3. تحضير البيانات للمكون
                var componentData = PrepareComponentData(directive, model);

                // 4. استدعاء Svelte Renderer
                var svelteHtml = await _svelteRenderer.RenderAsync(
                    directive.ComponentName,
                    componentData);

                // 5. تطبيق العزل
                var isolatedHtml = _isolationService.ApplyIsolation(
                    svelteHtml,
                    directive.ComponentName);

                // 6. الاستبدال في المكان المناسب
                resultHtml = ReplaceDirectiveWithComponent(
                    resultHtml,
                    directive,
                    isolatedHtml);
            }

            return resultHtml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering hybrid content");
            throw;
        }
    }

    public IEnumerable<SvelteDirective> FindSvelteDirectives(string razorHtml)
    {
        var directives = new List<SvelteDirective>();
        var regex = new Regex(
            @"@svelte\s+""([^""]+)""(?:\s+@require-policy\s+""([^""]+)"")?",
            RegexOptions.Multiline);

        var matches = regex.Matches(razorHtml);

        foreach (Match match in matches)
        {
            directives.Add(new SvelteDirective(
                ComponentName: match.Groups[1].Value,
                Position: match.Index,
                Policy: match.Groups[2].Success ? match.Groups[2].Value : null
            ));
        }

        return directives;
    }

    public object PrepareComponentData(SvelteDirective directive, object model)
    {
        // تحويل Model إلى صيغة JSON-friendly
        return new
        {
            componentName = directive.ComponentName,
            model = model,
            policy = directive.Policy
        };
    }

    public string ReplaceDirectiveWithComponent(
        string razorHtml,
        SvelteDirective directive,
        string componentHtml)
    {
        var directivePattern = $@"@svelte\s+""{directive.ComponentName}""[^\n]*";
        var regex = new Regex(directivePattern);

        return regex.Replace(razorHtml, componentHtml, 1);
    }
}
