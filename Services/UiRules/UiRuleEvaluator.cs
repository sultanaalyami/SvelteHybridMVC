using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HRCE.Services.UiRules;

public sealed class UiRuleEvaluator
{
    private readonly UiRuleStore _store;
    private readonly UiRuleOptions _options;

    public UiRuleEvaluator(UiRuleStore store, IOptions<UiRuleOptions> options)
    {
        _store = store;
        _options = options.Value;
    }

    public async Task<UiLayoutDecision> GetLayoutDecisionAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var rules = await _store.GetRulesAsync(cancellationToken);
        var matches = rules.Where(rule => rule.Scope == UiRuleScope.Layout && Matches(rule, httpContext, target: null));
        var rule = matches.FirstOrDefault(rule => !string.IsNullOrWhiteSpace(rule.Layout));

        return new UiLayoutDecision
        {
            LayoutKey = rule?.Layout ?? _options.DefaultLayout,
            Zone = _options.DefaultZone
        };
    }

    public async Task<bool> IsVisibleAsync(HttpContext httpContext, UiNavigationTarget target, CancellationToken cancellationToken = default)
    {
        var rules = await _store.GetRulesAsync(cancellationToken);
        var matches = rules.Where(rule => rule.Scope == UiRuleScope.Navigation && Matches(rule, httpContext, target));
        var rule = matches.FirstOrDefault(rule => rule.IsVisible.HasValue);

        return rule?.IsVisible ?? true;
    }

    private static bool Matches(UiRule rule, HttpContext httpContext, UiNavigationTarget? target)
    {
        if (!Match(rule.Domain, httpContext.Request.Host.Host))
        {
            return false;
        }

        if (!Match(rule.Environment, httpContext.RequestServices.GetService<IWebHostEnvironment>()?.EnvironmentName))
        {
            return false;
        }

        if (!Match(rule.Area, httpContext.GetRouteValue("area")?.ToString()))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(rule.Role))
        {
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            if (!httpContext.User.IsInRole(rule.Role))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(rule.ClaimType))
        {
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            if (!httpContext.User.HasClaim(rule.ClaimType, rule.ClaimValue ?? string.Empty))
            {
                return false;
            }
        }

        if (target is null)
        {
            return true;
        }

        if (!Match(rule.Controller, target.Controller))
        {
            return false;
        }

        if (!Match(rule.Action, target.Action))
        {
            return false;
        }

        if (!Match(rule.Page, target.Page))
        {
            return false;
        }

        return true;
    }

    private static bool Match(string? ruleValue, string? currentValue)
    {
        if (string.IsNullOrWhiteSpace(ruleValue))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return false;
        }

        return string.Equals(ruleValue, currentValue, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record UiLayoutDecision
{
    public required string LayoutKey { get; init; }
    public required string Zone { get; init; }
}

public sealed record UiNavigationTarget
{
    public string? Controller { get; init; }
    public string? Action { get; init; }
    public string? Page { get; init; }
}
