namespace HRCE.Services.HRCE;

/// <summary>
/// واجهة عارض Svelte
/// </summary>
public interface ISvelteRenderer
{
    /// <summary>
    /// عرض مكون Svelte من جانب الخادم
    /// </summary>
    Task<string> RenderAsync(string componentName, object data);

    /// <summary>
    /// التحقق من وجود مكون Svelte
    /// </summary>
    bool ComponentExists(string componentName);
}
