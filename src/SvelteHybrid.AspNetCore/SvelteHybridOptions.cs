namespace SvelteHybrid.AspNetCore;

/// <summary>
/// Configuration options for SvelteHybrid engine.
/// </summary>
public class SvelteHybridOptions
{
    /// <summary>
    /// Enable Svelte Server-Side Rendering (SSR).
    /// Default: true
    /// </summary>
    public bool EnableSvelteSSR { get; set; } = true;

    /// <summary>
    /// Base path for Svelte components.
    /// Default: ~/Components
    /// </summary>
    public string ComponentBasePath { get; set; } = "~/Components";

    /// <summary>
    /// Component isolation mode for CSS and JavaScript.
    /// Default: Full
    /// </summary>
    public IsolationMode IsolationMode { get; set; } = IsolationMode.Full;

    /// <summary>
    /// Default renderer type for components.
    /// Default: Svelte
    /// </summary>
    public RendererType DefaultRenderer { get; set; } = RendererType.Svelte;

    /// <summary>
    /// Enable hot reload during development.
    /// Default: false
    /// </summary>
    public bool EnableHotReload { get; set; } = false;

    /// <summary>
    /// Node.js SSR server URL.
    /// Default: http://localhost:3000
    /// </summary>
    public string NodeServerUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// Timeout for SSR requests in milliseconds.
    /// Default: 5000
    /// </summary>
    public int SSRTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Enable component caching for improved performance.
    /// Default: true
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes.
    /// Default: 60
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Authorization options for component-level access control.
    /// </summary>
    public SvelteAuthorizationOptions Authorization { get; set; } = new();

    /// <summary>
    /// Development mode options.
    /// </summary>
    public DevelopmentOptions Development { get; set; } = new();
}

/// <summary>
/// Component isolation modes.
/// </summary>
public enum IsolationMode
{
    /// <summary>
    /// Full isolation - CSS classes and JavaScript selectors are scoped.
    /// </summary>
    Full,

    /// <summary>
    /// Partial isolation - Only CSS classes are scoped.
    /// </summary>
    Partial,

    /// <summary>
    /// No isolation - Components share global styles and scripts.
    /// </summary>
    None
}

/// <summary>
/// Supported renderer types.
/// </summary>
public enum RendererType
{
    /// <summary>
    /// Svelte component renderer.
    /// </summary>
    Svelte,

    /// <summary>
    /// React component renderer (future support).
    /// </summary>
    React,

    /// <summary>
    /// Vue component renderer (future support).
    /// </summary>
    Vue,

    /// <summary>
    /// Razor-only rendering without frontend framework.
    /// </summary>
    RazorOnly
}

/// <summary>
/// Authorization options for Svelte components.
/// </summary>
public class SvelteAuthorizationOptions
{
    /// <summary>
    /// Default authorization policy for components.
    /// Default: null (no authorization required)
    /// </summary>
    public string? DefaultPolicy { get; set; }

    /// <summary>
    /// Component to render when authorization fails.
    /// Default: Unauthorized
    /// </summary>
    public string FallbackComponent { get; set; } = "Unauthorized";

    /// <summary>
    /// Enable authorization caching.
    /// Default: true
    /// </summary>
    public bool EnableCaching { get; set; } = true;
}

/// <summary>
/// Development-specific options.
/// </summary>
public class DevelopmentOptions
{
    /// <summary>
    /// Show detailed error messages in rendered output.
    /// Default: false
    /// </summary>
    public bool ShowDetailedErrors { get; set; } = false;

    /// <summary>
    /// Log all component renders.
    /// Default: false
    /// </summary>
    public bool LogRenders { get; set; } = false;

    /// <summary>
    /// Enable source maps for debugging.
    /// Default: true
    /// </summary>
    public bool EnableSourceMaps { get; set; } = true;
}
