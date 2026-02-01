namespace SvelteHybrid.AspNetCore.Abstractions;

/// <summary>
/// Interface for rendering Svelte components server-side.
/// </summary>
public interface ISvelteRenderer
{
    /// <summary>
    /// Renders a Svelte component with the provided data.
    /// </summary>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <param name="props">Properties to pass to the component.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML string.</returns>
    Task<string> RenderAsync(
        string componentName, 
        object? props = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a Svelte component with typed properties.
    /// </summary>
    /// <typeparam name="TProps">Type of the component properties.</typeparam>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <param name="props">Typed properties to pass to the component.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML string.</returns>
    Task<string> RenderAsync<TProps>(
        string componentName, 
        TProps props,
        CancellationToken cancellationToken = default) where TProps : class;

    /// <summary>
    /// Checks if a component exists.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <returns>True if the component exists.</returns>
    bool ComponentExists(string componentName);

    /// <summary>
    /// Gets the component path.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <returns>Full path to the component file.</returns>
    string GetComponentPath(string componentName);
}
