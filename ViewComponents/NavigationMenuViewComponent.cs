using HRCE.Navigation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRCE.ViewComponents;

public sealed class NavigationMenuViewComponent : ViewComponent
{
    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly Services.UiRules.UiRuleEvaluator _evaluator;

    public NavigationMenuViewComponent(IActionDescriptorCollectionProvider actions, Services.UiRules.UiRuleEvaluator evaluator)
    {
        _actions = actions;
        _evaluator = evaluator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();
        var currentPage = ViewContext.RouteData.Values["page"]?.ToString();

        var controllerItems = await BuildControllerItemsAsync(currentController, currentAction);
        var pageItems = await BuildPageItemsAsync(currentPage);

        var combined = controllerItems.Concat(pageItems)
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Label);

        return View(combined.ToList());
    }

    // -------------------------------------------------------------------------
    // Controllers
    // -------------------------------------------------------------------------

    private async Task<List<NavigationItem>> BuildControllerItemsAsync(string? currentController, string? currentAction)
    {
        // Group all descriptors by controller
        var descriptors = _actions.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(d =>
                d.ControllerName is not null &&
                d.ActionName is not null &&
                string.IsNullOrWhiteSpace(d.RouteValues.TryGetValue("area", out var area) ? area : null) &&
                // Auto-exclude: NonController, ApiController, ControllerBase-only
                !d.ControllerTypeInfo.IsDefined(typeof(NonControllerAttribute), inherit: true) &&
                !d.ControllerTypeInfo.IsDefined(typeof(ApiControllerAttribute), inherit: true) &&
                typeof(Controller).IsAssignableFrom(d.ControllerTypeInfo) &&
                // Explicit exclusion
                !d.ControllerTypeInfo.IsDefined(typeof(HideFromNavAttribute), inherit: true) &&
                !d.MethodInfo.IsDefined(typeof(NonActionAttribute), inherit: true) &&
                !d.MethodInfo.IsDefined(typeof(HideFromNavAttribute), inherit: true));

        var grouped = descriptors
            .GroupBy(d => d.ControllerName, StringComparer.OrdinalIgnoreCase)
            .DistinctBy(g => g.Key);

        var result = new List<NavigationItem>();

        foreach (var group in grouped)
        {
            var first = group.First();
            var controllerType = first.ControllerTypeInfo;
            var isExpandable = controllerType.IsDefined(typeof(NavExpandableAttribute), inherit: true);
            var controllerOrder = controllerType.GetCustomAttributes(typeof(NavOrderAttribute), true)
                .OfType<NavOrderAttribute>().FirstOrDefault()?.Order ?? 100;
            var controllerLabel = controllerType.GetCustomAttributes(typeof(NavLabelAttribute), true)
                .OfType<NavLabelAttribute>().FirstOrDefault()?.Label;

            if (isExpandable)
            {
                // ?? Expandable: show all actions as children ??
                var children = new List<NavigationItem>();

                foreach (var descriptor in group)
                {
                    var childItem = BuildActionItem(descriptor, currentController, currentAction, controllerLabel);
                    if (await IsVisibleAsync(childItem))
                    {
                        children.Add(childItem);
                    }
                }

                if (children.Count == 0)
                    continue;

                children.Sort((a, b) => a.Order.CompareTo(b.Order));

                var isAnyChildActive = children.Any(c => c.IsActive);

                result.Add(new NavigationItem
                {
                    Controller = group.Key,
                    Action = "Index",
                    Label = controllerLabel ?? group.Key,
                    Order = controllerOrder,
                    IsActive = isAnyChildActive,
                    Children = children
                });
            }
            else
            {
                // ?? Standard MVC: only Index action ??
                var indexDescriptor = group.FirstOrDefault(d =>
                    string.Equals(d.ActionName, "Index", StringComparison.OrdinalIgnoreCase));

                if (indexDescriptor is null)
                    continue;

                var item = new NavigationItem
                {
                    Controller = group.Key,
                    Action = "Index",
                    Label = controllerLabel ?? group.Key,
                    Order = controllerOrder,
                    IsActive = string.Equals(group.Key, currentController, StringComparison.OrdinalIgnoreCase)
                              && string.Equals(currentAction, "Index", StringComparison.OrdinalIgnoreCase)
                };

                if (await IsVisibleAsync(item))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }

    private static NavigationItem BuildActionItem(
        ControllerActionDescriptor descriptor,
        string? currentController,
        string? currentAction,
        string? parentLabel)
    {
        var actionLabel = descriptor.MethodInfo
            .GetCustomAttributes(typeof(NavLabelAttribute), true)
            .OfType<NavLabelAttribute>().FirstOrDefault()?.Label;

        var actionOrder = descriptor.MethodInfo
            .GetCustomAttributes(typeof(NavOrderAttribute), true)
            .OfType<NavOrderAttribute>().FirstOrDefault()?.Order ?? 100;

        // Default label: "Index" ? controller name, otherwise action name
        var label = actionLabel
            ?? (string.Equals(descriptor.ActionName, "Index", StringComparison.OrdinalIgnoreCase)
                ? parentLabel ?? descriptor.ControllerName
                : descriptor.ActionName);

        return new NavigationItem
        {
            Controller = descriptor.ControllerName,
            Action = descriptor.ActionName,
            Label = label,
            Order = actionOrder,
            IsActive = string.Equals(descriptor.ControllerName, currentController, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(descriptor.ActionName, currentAction, StringComparison.OrdinalIgnoreCase)
        };
    }

    // -------------------------------------------------------------------------
    // Razor Pages
    // -------------------------------------------------------------------------

    private async Task<List<NavigationItem>> BuildPageItemsAsync(string? currentPage)
    {
        var descriptors = _actions.ActionDescriptors.Items
            .OfType<PageActionDescriptor>()
            .Where(d =>
                !string.IsNullOrWhiteSpace(d.ViewEnginePath) &&
                string.IsNullOrWhiteSpace(d.AreaName) &&
                (string.Equals(d.ViewEnginePath, "/", StringComparison.OrdinalIgnoreCase) ||
                 d.ViewEnginePath.EndsWith("/Index", StringComparison.OrdinalIgnoreCase)))
            .DistinctBy(d => d.ViewEnginePath);

        var result = new List<NavigationItem>();

        foreach (var descriptor in descriptors)
        {
            var item = new NavigationItem
            {
                Page = descriptor.ViewEnginePath,
                Label = BuildPageLabel(descriptor.ViewEnginePath),
                IsActive = string.Equals(descriptor.ViewEnginePath, currentPage, StringComparison.OrdinalIgnoreCase)
            };

            if (await IsVisibleAsync(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static string BuildPageLabel(string? viewEnginePath)
    {
        var pageLabel = viewEnginePath?.Trim('/');

        if (string.IsNullOrWhiteSpace(pageLabel))
            return "Home";

        if (pageLabel.EndsWith("/Index", StringComparison.OrdinalIgnoreCase))
            return pageLabel[..^"/Index".Length];

        return pageLabel.Replace('/', ' ');
    }

    // -------------------------------------------------------------------------
    // UiRules integration
    // -------------------------------------------------------------------------

    private async Task<bool> IsVisibleAsync(NavigationItem item)
    {
        var target = new Services.UiRules.UiNavigationTarget
        {
            Controller = item.Controller,
            Action = item.Action,
            Page = item.Page
        };

        return await _evaluator.IsVisibleAsync(HttpContext, target, HttpContext.RequestAborted);
    }

    // -------------------------------------------------------------------------
    // Model
    // -------------------------------------------------------------------------

    public sealed class NavigationItem
    {
        public string? Controller { get; init; }
        public string? Action { get; init; }
        public string? Page { get; init; }
        public required string Label { get; init; }
        public int Order { get; init; } = 100;
        public bool IsActive { get; init; }
        public List<NavigationItem> Children { get; init; } = [];

        public bool IsPage => !string.IsNullOrWhiteSpace(Page);
        public bool IsExpandable => Children.Count > 0;
    }
}
