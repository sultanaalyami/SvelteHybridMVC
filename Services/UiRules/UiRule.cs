namespace HRCE.Services.UiRules;

public sealed class UiRule
{
    public int Id { get; set; }
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

public enum UiRuleScope
{
    Navigation = 0,
    Layout = 1
}
