using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;


public sealed class PrivacyController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}