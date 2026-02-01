using Microsoft.AspNetCore.Mvc;
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

        var controllerItems = _actions.ActionDescriptors.Items
            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
            .Where(descriptor =>
                descriptor.ControllerName is not null &&
                descriptor.ActionName is not null &&
                string.Equals(descriptor.ActionName, "Index", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(descriptor.RouteValues.TryGetValue("area", out var area) ? area : null) &&
                !descriptor.ControllerTypeInfo.IsDefined(typeof(NonControllerAttribute), inherit: true) &&
                !descriptor.MethodInfo.IsDefined(typeof(NonActionAttribute), inherit: true))
            .Select(descriptor => new NavigationItem(
                descriptor.ControllerName,
                descriptor.ActionName,
                null,
                descriptor.ControllerName == currentController && descriptor.ActionName == currentAction))
            .DistinctBy(item => (item.Controller, item.Action, item.Page));

        var pageItems = _actions.ActionDescriptors.Items
            .OfType<PageActionDescriptor>()
            .Where(descriptor =>
                !string.IsNullOrWhiteSpace(descriptor.ViewEnginePath) &&
                string.IsNullOrWhiteSpace(descriptor.AreaName) &&
                (string.Equals(descriptor.ViewEnginePath, "/", StringComparison.OrdinalIgnoreCase) ||
                 descriptor.ViewEnginePath.EndsWith("/Index", StringComparison.OrdinalIgnoreCase)))
            .Select(descriptor => new NavigationItem(
                null,
                null,
                descriptor.ViewEnginePath,
                string.Equals(descriptor.ViewEnginePath, currentPage, StringComparison.OrdinalIgnoreCase)))
            .DistinctBy(item => (item.Controller, item.Action, item.Page));

        var combined = controllerItems.Concat(pageItems).OrderBy(item => item.Label);
        var items = new List<NavigationItem>();
        foreach (var item in combined)
        {
            var target = new Services.UiRules.UiNavigationTarget
            {
                Controller = item.Controller,
                Action = item.Action,
                Page = item.Page
            };

            if (await _evaluator.IsVisibleAsync(HttpContext, target, HttpContext.RequestAborted))
            {
                items.Add(item);
            }
        }

        return View(items);
    }

    public sealed record NavigationItem(string? Controller, string? Action, string? Page, bool IsActive)
    {
        public bool IsPage => !string.IsNullOrWhiteSpace(Page);

        public string Label
        {
            get
            {
                if (IsPage)
                {
                    var pageLabel = Page?.Trim('/');
                    if (string.IsNullOrWhiteSpace(pageLabel))
                    {
                        return "Home";
                    }

                    if (pageLabel.EndsWith("/Index", StringComparison.OrdinalIgnoreCase))
                    {
                        return pageLabel[..^"/Index".Length];
                    }

                    return pageLabel.Replace('/', ' ');
                }

                if (string.Equals(Action, "Index", StringComparison.OrdinalIgnoreCase))
                {
                    return Controller ?? "Home";
                }

                return $"{Controller} / {Action}";
            }
        }
    }
}
