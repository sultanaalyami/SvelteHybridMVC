using Microsoft.AspNetCore.Mvc;
using HRCE.Core.Monitoring;
using HRCE.Core.Platform;

namespace HRCE.Controllers;

/// <summary>
/// Controller لعرض معلومات المراقبة والمنصة
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class MonitoringController : ControllerBase
{
    private readonly IKernelMonitor _kernelMonitor;
    private readonly IPlatformDetectionService _platformService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IKernelMonitor kernelMonitor,
        IPlatformDetectionService platformService,
        ILogger<MonitoringController> logger)
    {
        _kernelMonitor = kernelMonitor;
        _platformService = platformService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على معلومات المنصة الحالية
    /// </summary>
    [HttpGet("platform")]
    public IActionResult GetPlatformInfo()
    {
        var info = _platformService.GetPlatformInfo();
        return Ok(info);
    }

    /// <summary>
    /// الحصول على إحصائيات المراقبة
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _kernelMonitor.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// التحقق من حالة المراقبة
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            IsActive = _kernelMonitor.IsActive,
            SystemType = _kernelMonitor.SystemType.ToString(),
            Platform = _platformService.CurrentPlatform.ToString()
        });
    }

    /// <summary>
    /// بث الأحداث في الوقت الفعلي (Server-Sent Events)
    /// </summary>
    [HttpGet("events")]
    public async Task StreamEvents(CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("Access-Control-Allow-Origin", "*");

        try
        {
            await foreach (var evt in _kernelMonitor.StreamEventsAsync(cancellationToken))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(evt);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client disconnected from event stream");
        }
    }

    /// <summary>
    /// ربط تذكرة تنفيذ بعملية
    /// </summary>
    [HttpPost("tag")]
    public async Task<IActionResult> TagProcess([FromBody] TagProcessRequest request)
    {
        try
        {
            await _kernelMonitor.TagProcessAsync(request.TicketId, request.ProcessId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tag process {ProcessId} with ticket {TicketId}", 
                request.ProcessId, request.TicketId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على القدرات المتاحة على المنصة الحالية
    /// </summary>
    [HttpGet("capabilities")]
    public IActionResult GetCapabilities()
    {
        return Ok(new
        {
            EbpfSupported = _platformService.IsEbpfSupported,
            EtwSupported = _platformService.IsEtwSupported,
            DTraceSupported = _platformService.IsDTraceSupported,
            HasKernelHeaders = _platformService.HasKernelHeaders(),
            AvailableMonitoringSystem = _platformService.GetAvailableMonitoringSystem().ToString()
        });
    }
}

/// <summary>
/// نموذج طلب لربط عملية بتذكرة
/// </summary>
public record TagProcessRequest
{
    public required Guid TicketId { get; init; }
    public required uint ProcessId { get; init; }
}
