using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

[Authorize(Policy = "AdminOnly")]
public sealed class BuildOpsController : Controller
{
    [HttpGet("/builder/ops")]
    public IActionResult Index()
    {
        return View();
    }
}
