namespace HRCE.Navigation;

/// <summary>
/// Makes a controller appear as a dropdown menu in the navigation bar,
/// listing all its public actions (except those marked with <see cref="HideFromNavAttribute"/>).
/// Without this attribute, only the <c>Index</c> action is shown (standard MVC behavior).
/// </summary>
/// <example>
/// <code>
/// [NavExpandable]                           // dropdown with all actions
/// public class BuilderController : Controller
/// {
///     public IActionResult Index() => View();            // first item
///     public IActionResult PlatformBuilder() => View();  // second item
///
///     [HideFromNav]
///     public IActionResult InternalTool() => View();     // hidden
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NavExpandableAttribute : Attribute;
