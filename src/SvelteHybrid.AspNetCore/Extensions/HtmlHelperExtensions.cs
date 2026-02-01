using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SvelteHybrid.AspNetCore.Abstractions;

namespace SvelteHybrid.AspNetCore;

/// <summary>
/// HTML helper extensions for rendering Svelte components in Razor views.
/// </summary>
public static class SvelteHtmlHelperExtensions
{
    /// <summary>
    /// Renders a Svelte component inline.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <param name="props">Optional properties to pass to the component.</param>
    /// <returns>HTML content for the component.</returns>
    public static async Task<IHtmlContent> SvelteAsync(
        this IHtmlHelper htmlHelper,
        string componentName,
        object? props = null)
    {
        var renderer = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService(typeof(ISvelteRenderer)) as ISvelteRenderer;

        if (renderer == null)
        {
            return new HtmlString($"<!-- SvelteHybrid not configured for {componentName} -->");
        }

        var html = await renderer.RenderAsync(componentName, props);
        return new HtmlString(html);
    }

    /// <summary>
    /// Renders a Svelte component inline with typed props.
    /// </summary>
    /// <typeparam name="TProps">Type of the component props.</typeparam>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <param name="props">Typed properties to pass to the component.</param>
    /// <returns>HTML content for the component.</returns>
    public static async Task<IHtmlContent> SvelteAsync<TProps>(
        this IHtmlHelper htmlHelper,
        string componentName,
        TProps props) where TProps : class
    {
        return await SvelteAsync(htmlHelper, componentName, props);
    }

    /// <summary>
    /// Creates a Svelte component placeholder for client-side rendering.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <param name="props">Optional properties to pass to the component.</param>
    /// <returns>HTML placeholder for client-side hydration.</returns>
    public static IHtmlContent SvelteClientOnly(
        this IHtmlHelper htmlHelper,
        string componentName,
        object? props = null)
    {
        var propsJson = props != null 
            ? System.Text.Json.JsonSerializer.Serialize(props) 
            : "{}";

        var html = $"""
            <div data-svelte-component="{componentName}" data-svelte-client-only="true">
                <script type="application/json" data-component-props>{propsJson}</script>
            </div>
            """;

        return new HtmlString(html);
    }

    /// <summary>
    /// Checks if a Svelte component exists.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="componentName">Name of the Svelte component.</param>
    /// <returns>True if the component exists.</returns>
    public static bool SvelteComponentExists(
        this IHtmlHelper htmlHelper,
        string componentName)
    {
        var renderer = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService(typeof(ISvelteRenderer)) as ISvelteRenderer;

        return renderer?.ComponentExists(componentName) ?? false;
    }
}
