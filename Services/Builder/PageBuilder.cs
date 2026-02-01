using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;

namespace HRCE.Services.Builder;

public sealed class PageBuilder
{
    private readonly BuilderOptions _options;

    public PageBuilder(IOptions<BuilderOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.RootPath))
        {
            _options.RootPath = environment.ContentRootPath;
        }
    }

    public async Task<PageBuildResult> CreatePageAsync(PageBuildRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);
        var controllerName = $"{name}Controller";
        var viewFolder = Path.Combine(_options.RootPath, _options.ViewsPath, name);
        var controllerPath = Path.Combine(_options.RootPath, _options.ControllersPath, $"{controllerName}.cs");
        var viewPath = Path.Combine(viewFolder, "Index.cshtml");

        if (File.Exists(controllerPath) || File.Exists(viewPath))
        {
            return new PageBuildResult
            {
                Created = false,
                ControllerPath = controllerPath,
                ViewPath = viewPath
            };
        }

        Directory.CreateDirectory(viewFolder);

        var controllerContent = BuildController(controllerName, name, request.Role, request.Policy);
        var viewContent = BuildView(name, request.Title ?? name);

        await File.WriteAllTextAsync(controllerPath, controllerContent, Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(viewPath, viewContent, Encoding.UTF8, cancellationToken);

        return new PageBuildResult
        {
            Created = true,
            ControllerPath = controllerPath,
            ViewPath = viewPath
        };
    }

    private static string NormalizeName(string name)
    {
        var cleaned = new string(name.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return "Page";
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
    }

    private static string BuildController(string controllerName, string viewName, string? role, string? policy)
    {
        var authorize = BuildAuthorize(role, policy);
        return $$"""
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

{{authorize}}
public sealed class {{controllerName}} : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
""";
    }

    private static string BuildAuthorize(string? role, string? policy)
    {
        if (!string.IsNullOrWhiteSpace(role))
        {
            return $"[Authorize(Roles = \"{role}\")]";
        }

        if (!string.IsNullOrWhiteSpace(policy))
        {
            return $"[Authorize(Policy = \"{policy}\")]";
        }

        return string.Empty;
    }

    private static string BuildView(string name, string title)
    {
        return $$"""
@{
    ViewData["Title"] = "{{title}}";
}

<section class="hrce-stack">
    <header class="hrce-stack">
        <h1>{{title}}</h1>
        <p class="hrce-muted">’›Õ… „Ê·œ… »Ê«”ÿ… „‰’… «·»‰«¡ «·–ﬂÌ….</p>
    </header>

    <div class="hrce-grid hrce-grid--two">
        <div class="hrce-card">
            <div class="hrce-card__label">Module</div>
            <div class="hrce-card__value">{{name}}</div>
        </div>
        <div class="hrce-card">
            <div class="hrce-card__label">Status</div>
            <div class="hrce-card__value">Ready</div>
        </div>
    </div>
</section>
""";
    }
}
