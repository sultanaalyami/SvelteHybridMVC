using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HRCE.Services.UiRules;

public sealed class UiRuleStore
{
    private readonly UiRulesDbContext _dbContext;
    private readonly UiRuleOptions _options;

    public UiRuleStore(UiRulesDbContext dbContext, IOptions<UiRuleOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<UiRule>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var dbRules = await _dbContext.UiRules.AsNoTracking().ToListAsync(cancellationToken);
        var seededRules = _options.Rules.Select(seed => new UiRule
        {
            Scope = seed.Scope,
            Controller = seed.Controller,
            Action = seed.Action,
            Page = seed.Page,
            Area = seed.Area,
            Domain = seed.Domain,
            Environment = seed.Environment,
            Role = seed.Role,
            ClaimType = seed.ClaimType,
            ClaimValue = seed.ClaimValue,
            Layout = seed.Layout,
            IsVisible = seed.IsVisible,
            Priority = seed.Priority
        });

        return dbRules.Concat(seededRules)
            .OrderByDescending(rule => rule.Priority)
            .ToList();
    }
}
