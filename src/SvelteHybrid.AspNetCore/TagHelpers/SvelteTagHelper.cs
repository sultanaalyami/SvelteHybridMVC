using Microsoft.AspNetCore.Razor.TagHelpers;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Text.Json;

namespace SvelteHybrid.AspNetCore.TagHelpers;

/// <summary>
/// Tag helper for rendering Svelte components in Razor views.
/// </summary>
/// <example>
/// <code>
/// &lt;svelte component="ProductCard" props="@Model.Product" /&gt;
/// &lt;svelte component="AdminPanel" require-policy="AdminOnly" /&gt;
/// </code>
/// </example>
[HtmlTargetElement("svelte", TagStructure = TagStructure.WithoutEndTag)]
public class SvelteTagHelper : TagHelper
{
    private readonly ISvelteRenderer _svelteRenderer;

    /// <summary>
    /// Name of the Svelte component to render.
    /// </summary>
    [HtmlAttributeName("component")]
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// Properties to pass to the component.
    /// </summary>
    [HtmlAttributeName("props")]
    public object? Props { get; set; }

    /// <summary>
    /// Authorization policy required to view the component.
    /// </summary>
    [HtmlAttributeName("require-policy")]
    public string? RequirePolicy { get; set; }

    /// <summary>
    /// Render on client-side only (no SSR).
    /// </summary>
    [HtmlAttributeName("client-only")]
    public bool ClientOnly { get; set; }

    /// <summary>
    /// CSS class to apply to the wrapper element.
    /// </summary>
    [HtmlAttributeName("wrapper-class")]
    public string? WrapperClass { get; set; }

    public SvelteTagHelper(ISvelteRenderer svelteRenderer)
    {
        _svelteRenderer = svelteRenderer;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Component))
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = null;

        if (ClientOnly)
        {
            var propsJson = Props != null ? JsonSerializer.Serialize(Props) : "{}";
            var wrapperClass = string.IsNullOrEmpty(WrapperClass) ? "" : $" class=\"{WrapperClass}\"";
            
            output.Content.SetHtmlContent($"""
                <div data-svelte-component="{Component}" data-svelte-client-only="true"{wrapperClass}>
                    <script type="application/json" data-component-props>{propsJson}</script>
                </div>
                """);
            return;
        }

        // Add policy attribute if specified (processed by middleware)
        if (!string.IsNullOrEmpty(RequirePolicy))
        {
            // Render as directive for middleware processing
            output.Content.SetHtmlContent($"""@svelte "{Component}" @require-policy "{RequirePolicy}" """);
            return;
        }

        // Render component directly
        var html = await _svelteRenderer.RenderAsync(Component, Props);
        
        if (!string.IsNullOrEmpty(WrapperClass))
        {
            html = $"<div class=\"{WrapperClass}\">{html}</div>";
        }

        output.Content.SetHtmlContent(html);
    }
}

/// <summary>
/// Tag helper for Svelte component slots.
/// </summary>
[HtmlTargetElement("svelte-slot", ParentTag = "svelte")]
public class SvelteSlotTagHelper : TagHelper
{
    /// <summary>
    /// Name of the slot.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = "default";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();
        
        output.TagName = "template";
        output.Attributes.SetAttribute("data-svelte-slot", Name);
        output.Content.SetHtmlContent(childContent);
    }
}
