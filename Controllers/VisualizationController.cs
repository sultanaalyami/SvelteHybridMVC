using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

/// <summary>
/// تحكم في صفحات التصور المتقدم
/// </summary>
public class VisualizationController : Controller
{
    private readonly ILogger<VisualizationController> _logger;

    public VisualizationController(ILogger<VisualizationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// صفحة تصور النخاع الشوكي الرقمي
    /// </summary>
    [HttpGet]
    public IActionResult NeuralSpine()
    {
        _logger.LogInformation("Neural Spine Visualization page accessed");
        return View();
    }

    /// <summary>
    /// معاينة تفاعلية بدون UI
    /// </summary>
    [HttpGet]
    public IActionResult FullScreen()
    {
        return View("NeuralSpine");
    }

    /// <summary>
    /// محرك الكون الكمي المتعدد - تصور فيزياء الكم والأبعاد المتعددة
    /// Quantum Multiverse Engine - eBPF data visualization as quantum phenomena
    /// </summary>
    [HttpGet]
    public IActionResult QuantumMultiverse()
    {
        _logger.LogInformation("Quantum Multiverse Visualization page accessed");
        return View();
    }

    /// <summary>
    /// صائد الكم - لعبة البحث عن العمليات في الفضاء الكمي
    /// Quantum Hunter - Process hunting game in quantum space
    /// </summary>
    [HttpGet]
    public IActionResult QuantumHunter()
    {
        _logger.LogInformation("Quantum Hunter game accessed");
        return View();
    }
}
