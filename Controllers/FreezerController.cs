using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;


public sealed class FreezerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}