using HRCE.Services.Builder;
using HRCE.Services.UiRules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRCE.Controllers;

[ApiController]
[Route("api/builder")]
public sealed class BuilderApiController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly PageBuilder _pageBuilder;
    private readonly UiRulesDbContext _dbContext;

    public BuilderApiController(IWebHostEnvironment environment, PageBuilder pageBuilder, UiRulesDbContext dbContext)
    {
        _environment = environment;
        _pageBuilder = pageBuilder;
        _dbContext = dbContext;
    }

    [HttpPost("page")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePage([FromBody] PageBuildRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var result = await _pageBuilder.CreatePageAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("rule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRule([FromBody] UiRule rule, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        _dbContext.UiRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(rule);
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var rules = await _dbContext.UiRules.AsNoTracking().OrderByDescending(r => r.Priority).ToListAsync(cancellationToken);
        return Ok(rules);
    }
}
