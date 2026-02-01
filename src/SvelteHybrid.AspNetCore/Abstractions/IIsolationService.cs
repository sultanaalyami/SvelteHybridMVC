namespace SvelteHybrid.AspNetCore.Abstractions;

/// <summary>
/// Interface for component isolation service.
/// </summary>
public interface IIsolationService
{
    /// <summary>
    /// Applies isolation to component HTML based on configuration.
    /// </summary>
    /// <param name="componentHtml">The component HTML.</param>
    /// <param name="componentName">Name of the component.</param>
    /// <returns>Isolated HTML.</returns>
    string ApplyIsolation(string componentHtml, string componentName);

    /// <summary>
    /// Generates a unique hash for a component.
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <returns>Unique hash string.</returns>
    string GenerateComponentHash(string componentName);

    /// <summary>
    /// Isolates CSS by scoping class names.
    /// </summary>
    /// <param name="css">The CSS content.</param>
    /// <param name="hash">The component hash.</param>
    /// <returns>Scoped CSS.</returns>
    string IsolateCSS(string css, string hash);

    /// <summary>
    /// Isolates JavaScript by scoping selectors.
    /// </summary>
    /// <param name="js">The JavaScript content.</param>
    /// <param name="hash">The component hash.</param>
    /// <returns>Scoped JavaScript.</returns>
    string IsolateJavaScript(string js, string hash);

    /// <summary>
    /// Clears the isolation cache.
    /// </summary>
    void ClearCache();
}
