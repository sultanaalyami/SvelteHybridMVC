using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SvelteHybrid.AspNetCore.TagHelpers;

/// <summary>
/// Tag helper that makes any element reactive
/// Usage: &lt;div s-reactive s-data="{ count: 0 }"&gt;...&lt;/div&gt;
/// </summary>
[HtmlTargetElement(Attributes = "s-reactive")]
public class ReactiveTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // The s-reactive attribute is processed client-side
        // This tag helper just ensures proper rendering
    }
}

/// <summary>
/// Toast container component
/// Usage: &lt;s-toast&gt;&lt;/s-toast&gt;
/// </summary>
[HtmlTargetElement("s-toast")]
public class ToastTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "s-toast-container");
    }
}

/// <summary>
/// Modal dialog component
/// Usage: &lt;s-modal id="myModal"&gt;...&lt;/s-modal&gt;
/// </summary>
[HtmlTargetElement("s-modal")]
public class ModalTagHelper : TagHelper
{
    [HtmlAttributeName("id")]
    public string? ModalId { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("s-modal", ModalId ?? "");
        
        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent($"<div class=\"s-modal-content\">{content.GetContent()}</div>");
    }
}

/// <summary>
/// Loading spinner component
/// Usage: &lt;s-loading&gt;&lt;/s-loading&gt; or &lt;s-loading size="lg"&gt;&lt;/s-loading&gt;
/// </summary>
[HtmlTargetElement("s-loading")]
public class LoadingTagHelper : TagHelper
{
    [HtmlAttributeName("size")]
    public string Size { get; set; } = "md";

    [HtmlAttributeName("s-show")]
    public string? ShowCondition { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.SetAttribute("s-loading", "");
        output.Attributes.SetAttribute("size", Size);
        
        if (!string.IsNullOrEmpty(ShowCondition))
        {
            output.Attributes.SetAttribute("s-show", ShowCondition);
        }
    }
}

/// <summary>
/// Tabs container component
/// Usage: &lt;s-tabs&gt;&lt;s-tab title="Tab 1"&gt;...&lt;/s-tab&gt;&lt;/s-tabs&gt;
/// </summary>
[HtmlTargetElement("s-tabs")]
public class TabsTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "s-tabs");
    }
}

/// <summary>
/// Individual tab component
/// </summary>
[HtmlTargetElement("s-tab", ParentTag = "s-tabs")]
public class TabTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = "Tab";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "s-tab-content");
        output.Attributes.SetAttribute("data-tab-title", Title);
    }
}

/// <summary>
/// Card component
/// Usage: &lt;s-card&gt;...&lt;/s-card&gt;
/// </summary>
[HtmlTargetElement("s-card")]
public class CardTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "s-card");
    }
}

/// <summary>
/// Badge component
/// Usage: &lt;s-badge&gt;5&lt;/s-badge&gt; or &lt;s-badge type="success"&gt;Done&lt;/s-badge&gt;
/// </summary>
[HtmlTargetElement("s-badge")]
public class BadgeTagHelper : TagHelper
{
    [HtmlAttributeName("type")]
    public string Type { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        var cssClass = string.IsNullOrEmpty(Type) ? "s-badge" : $"s-badge s-badge-{Type}";
        output.Attributes.SetAttribute("class", cssClass);
    }
}

/// <summary>
/// Skeleton loading placeholder
/// Usage: &lt;s-skeleton width="200px" height="20px"&gt;&lt;/s-skeleton&gt;
/// </summary>
[HtmlTargetElement("s-skeleton")]
public class SkeletonTagHelper : TagHelper
{
    [HtmlAttributeName("width")]
    public string Width { get; set; } = "100%";

    [HtmlAttributeName("height")]
    public string Height { get; set; } = "20px";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "s-skeleton");
        output.Attributes.SetAttribute("style", $"width: {Width}; height: {Height};");
    }
}
