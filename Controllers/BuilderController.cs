using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

public sealed class BuilderController : Controller
{
    [HttpGet("/builder")]
    public IActionResult Index()
    {
        return View();
    }
}
