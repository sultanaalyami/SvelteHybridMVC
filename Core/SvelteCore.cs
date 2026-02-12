namespace HRCE.Core;

/// <summary>
/// Svelte rendering options
/// </summary>
public class SvelteOptions
{
    public bool EnableSSR { get; set; } = true;
    public HydrationMode HydrationMode { get; set; } = HydrationMode.Selective;
    public bool WatchForChanges { get; set; }
}

public enum HydrationMode
{
    None = 0,
    Full = 1,
    Selective = 2
}

/// <summary>
/// Svelte server-side renderer interface
/// </summary>
public interface ISvelteRenderer
{
    Task<string> RenderAsync(string componentName, object? data = null);
}
