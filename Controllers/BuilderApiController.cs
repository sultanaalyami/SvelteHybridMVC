using HRCE.Services.Builder;
using HRCE.Services.UiRules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HRCE.Controllers;

[ApiController]
[Route("api/builder")]
public sealed class BuilderApiController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly PageBuilder _pageBuilder;
    private readonly UiRulesDbContext _dbContext;
    private readonly LayoutStore _layoutStore;

    public BuilderApiController(IWebHostEnvironment environment, PageBuilder pageBuilder, UiRulesDbContext dbContext, LayoutStore layoutStore)
    {
        _environment = environment;
        _pageBuilder = pageBuilder;
        _dbContext = dbContext;
        _layoutStore = layoutStore;
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

    [HttpGet("layout")]
    public async Task<IActionResult> GetLayout(CancellationToken cancellationToken)
    {
        var layout = await _layoutStore.GetCurrentAsync(cancellationToken);
        return Ok(layout);
    }

    [HttpGet("layout/palette")]
    public async Task<IActionResult> GetLayoutPalette(CancellationToken cancellationToken)
    {
        var links = await _dbContext.UiRules.AsNoTracking()
            .Where(rule => rule.Scope == UiRuleScope.Navigation)
            .Where(rule => rule.IsVisible == null || rule.IsVisible == true)
            .OrderByDescending(rule => rule.Priority)
            .Select(rule => new LayoutPaletteLink(
                Label: BuildLinkLabel(rule),
                Url: BuildLinkUrl(rule)))
            .ToListAsync(cancellationToken);

        if (User.Identity?.IsAuthenticated == true)
        {
            links.AddRange(new[]
            {
                new LayoutPaletteLink("Identity/Profile", "/Identity/Account/Manage"),
                new LayoutPaletteLink("Identity/Logout", "/Identity/Account/Logout")
            });
        }

        var blocks = new[]
        {
            new LayoutPaletteBlock("header", "Header"),
            new LayoutPaletteBlock("nav", "Navigation"),
            new LayoutPaletteBlock("section", "Section"),
            new LayoutPaletteBlock("footer", "Footer"),
            new LayoutPaletteBlock("link", "Link")
        };

        return Ok(new LayoutPaletteResponse(blocks, links));
    }

    [HttpGet("layout/versions")]
    public async Task<IActionResult> GetLayoutVersions(CancellationToken cancellationToken)
    {
        var versions = await _layoutStore.GetVersionsAsync(cancellationToken);
        return Ok(versions);
    }

    [HttpPost("layout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLayout([FromBody] LayoutSaveRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var version = await _layoutStore.SaveAsync(request.Name ?? "Auto", request.Layout ?? LayoutDefinition.Empty, cancellationToken);
        return Ok(version);
    }

    [HttpPost("layout/restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreLayout([FromBody] LayoutRestoreRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var restored = await _layoutStore.RestoreAsync(request.VersionId, cancellationToken);
        return restored ? Ok() : NotFound();
    }

    private static string BuildLinkLabel(UiRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.Controller) && !string.IsNullOrWhiteSpace(rule.Action))
        {
            return string.Concat(rule.Controller, "/", rule.Action);
        }

        if (!string.IsNullOrWhiteSpace(rule.Page))
        {
            return rule.Page;
        }

        return "Link";
    }

    private static string BuildLinkUrl(UiRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.Controller) && !string.IsNullOrWhiteSpace(rule.Action))
        {
            return string.Concat("/", rule.Controller, "/", rule.Action);
        }

        if (!string.IsNullOrWhiteSpace(rule.Page))
        {
            return string.Concat("/", rule.Page.TrimStart('/'));
        }

        return "/";
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Platform Builder API Endpoints
    // ???????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Save a platform builder project
    /// </summary>
    [HttpPost("platform/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePlatformProject([FromBody] PlatformProject project, CancellationToken cancellationToken)
    {
        if (project == null)
        {
            return BadRequest(new { error = "Project data is required" });
        }

        try
        {
            var projectsPath = Path.Combine(_environment.ContentRootPath, "App_Data", "platforms");
            Directory.CreateDirectory(projectsPath);

            var fileName = $"{project.Id}.json";
            var filePath = Path.Combine(projectsPath, fileName);

            var json = System.Text.Json.JsonSerializer.Serialize(project, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });

            await System.IO.File.WriteAllTextAsync(filePath, json, cancellationToken);

            return Ok(new { success = true, id = project.Id, savedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Load a platform builder project
    /// </summary>
    [HttpGet("platform/{id}")]
    public async Task<IActionResult> LoadPlatformProject(string id, CancellationToken cancellationToken)
    {
        var projectsPath = Path.Combine(_environment.ContentRootPath, "App_Data", "platforms");
        var filePath = Path.Combine(projectsPath, $"{id}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "Project not found" });
        }

        var json = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken);
        var project = System.Text.Json.JsonSerializer.Deserialize<PlatformProject>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        return Ok(project);
    }

    /// <summary>
    /// List all platform projects
    /// </summary>
    [HttpGet("platform/list")]
    public IActionResult ListPlatformProjects()
    {
        var projectsPath = Path.Combine(_environment.ContentRootPath, "App_Data", "platforms");
        
        if (!Directory.Exists(projectsPath))
        {
            return Ok(Array.Empty<object>());
        }

        var files = Directory.GetFiles(projectsPath, "*.json");
        var projects = files.Select(f => new
        {
            id = Path.GetFileNameWithoutExtension(f),
            modifiedAt = System.IO.File.GetLastWriteTimeUtc(f)
        }).OrderByDescending(p => p.modifiedAt);

        return Ok(projects);
    }

    /// <summary>
    /// Publish a platform project as static files
    /// </summary>
    [HttpPost("platform/publish")]
    public async Task<IActionResult> PublishPlatformProject([FromBody] PlatformPublishRequest request, CancellationToken cancellationToken)
    {
        if (request?.Project == null)
        {
            return BadRequest(new { error = "Project data is required" });
        }

        try
        {
            var publishPath = Path.Combine(_environment.WebRootPath, "published", request.Project.Id);
            Directory.CreateDirectory(publishPath);

            // Save HTML
            if (!string.IsNullOrEmpty(request.Html))
            {
                await System.IO.File.WriteAllTextAsync(
                    Path.Combine(publishPath, "index.html"), 
                    request.Html, 
                    cancellationToken);
            }

            // Save CSS
            if (!string.IsNullOrEmpty(request.Css))
            {
                await System.IO.File.WriteAllTextAsync(
                    Path.Combine(publishPath, "styles.css"), 
                    request.Css, 
                    cancellationToken);
            }

            var url = $"/published/{request.Project.Id}/index.html";
            return Ok(new { success = true, url, publishedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a platform project
    /// </summary>
    [HttpDelete("platform/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePlatformProject(string id)
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var projectsPath = Path.Combine(_environment.ContentRootPath, "App_Data", "platforms");
        var filePath = Path.Combine(projectsPath, $"{id}.json");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "Project not found" });
        }

        System.IO.File.Delete(filePath);

        // Also delete published files if they exist
        var publishPath = Path.Combine(_environment.WebRootPath, "published", id);
        if (Directory.Exists(publishPath))
        {
            Directory.Delete(publishPath, true);
        }

        return Ok(new { success = true, deletedAt = DateTime.UtcNow });
    }
}

public sealed record LayoutSaveRequest(string? Name, LayoutDefinition? Layout);

public sealed record LayoutRestoreRequest(Guid VersionId);

public sealed record LayoutPaletteResponse(IReadOnlyList<LayoutPaletteBlock> Blocks, IReadOnlyList<LayoutPaletteLink> Links);

public sealed record LayoutPaletteBlock(string Type, string Label);

public sealed record LayoutPaletteLink(string Label, string Url);

// Platform Builder Models
public sealed record PlatformProject(
    string Id,
    string Name,
    List<PlatformPage> Pages,
    string CurrentPage,
    Dictionary<string, object>? Styles,
    Dictionary<string, object>? Settings
);

public sealed record PlatformPage(
    string Id,
    string Name,
    List<PlatformElement> Elements
);

public sealed record PlatformElement(
    string Id,
    string Type,
    Dictionary<string, string>? Styles,
    string? Content,
    Dictionary<string, string>? Attributes,
    List<PlatformElement>? Children
);

public sealed record PlatformPublishRequest(
    PlatformProject? Project,
    string? Html,
    string? Css
);
