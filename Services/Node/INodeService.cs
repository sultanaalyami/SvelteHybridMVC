namespace HRCE.Services.Node;

/// <summary>
/// واجهة للتواصل مع خدمة Node.js SSR
/// </summary>
public interface INodeService
{
    /// <summary>
    /// طلب عرض مكون من خدمة Node.js
    /// </summary>
    Task<NodeRenderResult> RenderComponentAsync(string componentName, object props);
}

public record NodeRenderResult(string Html, string Css, string Head);
