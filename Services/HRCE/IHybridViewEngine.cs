namespace HRCE.Services.HRCE;

/// <summary>
/// واجهة محرك العرض الهجين
/// </summary>
public interface IHybridViewEngine
{
    /// <summary>
    /// عرض محتوى هجين (Razor + Svelte)
    /// </summary>
    Task<string> RenderHybridAsync(string razorHtml, object model);

    /// <summary>
    /// البحث عن توجيهات Svelte في المحتوى
    /// </summary>
    IEnumerable<SvelteDirective> FindSvelteDirectives(string razorHtml);

    /// <summary>
    /// تحضير بيانات المكون
    /// </summary>
    object PrepareComponentData(SvelteDirective directive, object model);

    /// <summary>
    /// استبدال التوجيه بمحتوى المكون
    /// </summary>
    string ReplaceDirectiveWithComponent(string razorHtml, SvelteDirective directive, string componentHtml);
}

/// <summary>
/// توجيه Svelte المكتشف في المحتوى
/// </summary>
public record SvelteDirective(
    string ComponentName,
    int Position,
    string? Policy = null,
    Dictionary<string, string>? Attributes = null
);
