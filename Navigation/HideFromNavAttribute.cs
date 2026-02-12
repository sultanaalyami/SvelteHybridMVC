namespace HRCE.Navigation;

/// <summary>
/// Hides a controller or action from the navigation bar.
/// Can be applied to an entire controller or individual actions.
/// </summary>
/// <example>
/// <code>
/// [HideFromNav]                    // hides entire controller
/// public class InternalController : Controller { }
///
/// public class HomeController : Controller
/// {
///     [HideFromNav]                // hides only this action
///     public IActionResult Secret() => View();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class HideFromNavAttribute : Attribute;
