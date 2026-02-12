namespace HRCE.Navigation;

/// <summary>
/// Overrides the default label shown in the navigation bar for a controller or action.
/// Without this attribute, the controller or action name is used as-is.
/// </summary>
/// <example>
/// <code>
/// [NavLabel("Build Tools")]
/// public class BuilderController : Controller
/// {
///     [NavLabel("Visual Builder")]
///     public IActionResult PlatformBuilder() => View();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class NavLabelAttribute(string label) : Attribute
{
    public string Label { get; } = label;
}
