namespace HRCE.Services.UiRules;

public sealed class UiRuleOptions
{
    public string DefaultLayout { get; set; } = "core";
    public string DefaultZone { get; set; } = "primary";
    public List<UiRuleSeed> Rules { get; set; } = new();
}

public sealed class UiRuleSeed
{
    public UiRuleScope Scope { get; set; } = UiRuleScope.Navigation;
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public string? Page { get; set; }
    public string? Area { get; set; }
    public string? Domain { get; set; }
    public string? Environment { get; set; }
    public string? Role { get; set; }
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
    public string? Layout { get; set; }
    public bool? IsVisible { get; set; }
    public int Priority { get; set; } = 0;
}
