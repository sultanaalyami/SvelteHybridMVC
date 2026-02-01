using Microsoft.EntityFrameworkCore;

namespace HRCE.Services.UiRules;

public sealed class UiRulesDbContext(DbContextOptions<UiRulesDbContext> options) : DbContext(options)
{
    public DbSet<UiRule> UiRules => Set<UiRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UiRule>(entity =>
        {
            entity.HasKey(rule => rule.Id);
            entity.HasIndex(rule => new { rule.Scope, rule.Controller, rule.Action, rule.Page, rule.Area, rule.Domain, rule.Environment, rule.Role });
        });
    }
}
