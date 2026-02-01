using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace SvelteHybrid.AspNetCore;

/// <summary>
/// Extension methods for adding SvelteHybrid to the application pipeline
/// </summary>
public static class SvelteHybridExtensions
{
    /// <summary>
    /// Adds SvelteHybrid reactive capabilities to your application.
    /// This single line transforms your MVC/Razor Pages into a reactive SPA!
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configure">Optional configuration</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseSvelteHybrid(
        this IApplicationBuilder app,
        Action<SvelteHybridOptions>? configure = null)
    {
        var options = new SvelteHybridOptions();
        configure?.Invoke(options);

        // Serve embedded static files
        var assembly = typeof(SvelteHybridExtensions).Assembly;
        var provider = new ManifestEmbeddedFileProvider(assembly, "wwwroot");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = provider,
            RequestPath = "/_sveltehybrid"
        });

        // Add middleware to inject scripts
        app.UseMiddleware<SvelteHybridMiddleware>(options);

        return app;
    }

    /// <summary>
    /// Adds SvelteHybrid with WebApplication (minimal API style)
    /// </summary>
    public static WebApplication UseSvelteHybrid(
        this WebApplication app,
        Action<SvelteHybridOptions>? configure = null)
    {
        ((IApplicationBuilder)app).UseSvelteHybrid(configure);
        return app;
    }
}

/// <summary>
/// Configuration options for SvelteHybrid
/// </summary>
public class SvelteHybridOptions
{
    /// <summary>
    /// Enable browser DevTools integration (default: true in Development)
    /// </summary>
    public bool EnableDevTools { get; set; } = true;

    /// <summary>
    /// Auto-inject default styles (default: true)
    /// </summary>
    public bool AutoInjectStyles { get; set; } = true;

    /// <summary>
    /// Animation duration in milliseconds (default: 200)
    /// </summary>
    public int AnimationDuration { get; set; } = 200;

    /// <summary>
    /// Paths to exclude from processing
    /// </summary>
    public List<string> ExcludePaths { get; set; } = new() { "/api", "/_", "/swagger" };
}
