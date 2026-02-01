using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

public sealed class ObservabilityController : Controller
{
    [HttpGet("/observability")]
    public IActionResult Index()
    {
        return View();
    }
}
