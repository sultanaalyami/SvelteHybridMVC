using Microsoft.Extensions.Hosting;

namespace HRCE.Services.UiRules;

public sealed class UiRulesInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _environment;

    public UiRulesInitializer(IServiceScopeFactory scopeFactory, IWebHostEnvironment environment)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UiRulesDbContext>();
        if (_environment.IsDevelopment())
        {
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
