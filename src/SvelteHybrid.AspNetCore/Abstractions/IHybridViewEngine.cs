namespace SvelteHybrid.AspNetCore.Abstractions;

/// <summary>
/// Interface for the hybrid view engine that combines Razor and Svelte.
/// </summary>
public interface IHybridViewEngine
{
    /// <summary>
    /// Renders hybrid content combining Razor HTML with Svelte components.
    /// </summary>
    /// <param name="razorHtml">The Razor-rendered HTML containing Svelte directives.</param>
    /// <param name="model">The view model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed HTML with rendered Svelte components.</returns>
    Task<string> RenderHybridAsync(
        string razorHtml, 
        object? model = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all Svelte directives in the HTML content.
    /// </summary>
    /// <param name="razorHtml">The HTML content to search.</param>
    /// <returns>Collection of found Svelte directives.</returns>
    IEnumerable<SvelteDirective> FindSvelteDirectives(string razorHtml);

    /// <summary>
    /// Prepares component data from directive and model.
    /// </summary>
    /// <param name="directive">The Svelte directive.</param>
    /// <param name="model">The view model.</param>
    /// <returns>Prepared component data.</returns>
    object PrepareComponentData(SvelteDirective directive, object? model);

    /// <summary>
    /// Replaces a directive with rendered component HTML.
    /// </summary>
    /// <param name="razorHtml">The original HTML.</param>
    /// <param name="directive">The directive to replace.</param>
    /// <param name="componentHtml">The rendered component HTML.</param>
    /// <returns>HTML with replaced directive.</returns>
    string ReplaceDirectiveWithComponent(
        string razorHtml, 
        SvelteDirective directive, 
        string componentHtml);
}

/// <summary>
/// Represents a Svelte directive found in Razor content.
/// </summary>
/// <param name="ComponentName">Name of the Svelte component.</param>
/// <param name="Position">Position in the HTML string.</param>
/// <param name="Length">Length of the directive string.</param>
/// <param name="Policy">Authorization policy (optional).</param>
/// <param name="Data">Data expression (optional).</param>
/// <param name="Attributes">Additional attributes.</param>
public record SvelteDirective(
    string ComponentName,
    int Position,
    int Length,
    string? Policy = null,
    string? Data = null,
    Dictionary<string, string>? Attributes = null
);
