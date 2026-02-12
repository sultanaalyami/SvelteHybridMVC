namespace HRCE.Navigation;

/// <summary>
/// Controls the display order of a controller or action in the navigation bar.
/// Lower values appear first. Default order is <c>100</c>.
/// </summary>
/// <example>
/// <code>
/// [NavOrder(1)]
/// public class HomeController : Controller { }
///
/// [NavOrder(2)]
/// public class ProductsController : Controller { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class NavOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
