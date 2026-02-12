using HRCE.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

[NavExpandable]
[NavLabel("Builder")]
[NavOrder(50)]
public sealed class BuilderController : Controller
{
    [HttpGet("/builder")]
    [NavLabel("Builder Home")]
    [NavOrder(1)]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Platform Builder - Professional No-Code Drag & Drop Builder
    /// </summary>
    [HttpGet("/builder/platform")]
    [NavLabel("Platform Builder")]
    [NavOrder(2)]
    public IActionResult PlatformBuilder()
    {
        return View();
    }
}
