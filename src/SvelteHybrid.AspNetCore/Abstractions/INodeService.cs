namespace SvelteHybrid.AspNetCore.Abstractions;

/// <summary>
/// Interface for Node.js SSR communication.
/// </summary>
public interface INodeService
{
    /// <summary>
    /// Renders a component using Node.js SSR.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <param name="props">Component properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Render result with HTML, CSS, and head content.</returns>
    Task<NodeRenderResult> RenderComponentAsync(
        string componentName, 
        object? props = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the Node.js server is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if server is available.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the server status.
    /// </summary>
    /// <returns>Server status information.</returns>
    Task<NodeServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of Node.js SSR rendering.
/// </summary>
/// <param name="Html">Rendered HTML content.</param>
/// <param name="Css">Scoped CSS styles.</param>
/// <param name="Head">Head content (meta tags, etc.).</param>
/// <param name="RenderTimeMs">Time taken to render in milliseconds.</param>
public record NodeRenderResult(
    string Html, 
    string Css, 
    string Head,
    double RenderTimeMs = 0
);

/// <summary>
/// Node.js server status information.
/// </summary>
/// <param name="IsRunning">Whether the server is running.</param>
/// <param name="Version">Node.js version.</param>
/// <param name="Uptime">Server uptime.</param>
/// <param name="ComponentsLoaded">Number of loaded components.</param>
public record NodeServerStatus(
    bool IsRunning,
    string? Version = null,
    TimeSpan? Uptime = null,
    int ComponentsLoaded = 0
);
