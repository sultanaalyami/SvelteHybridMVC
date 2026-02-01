using HRCE.Services.Logging;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;
    private readonly LogStore _store;

    public LogsController(ILogger<LogsController> logger, LogStore store)
    {
        _logger = logger;
        _store = store;
    }

    [HttpGet("recent")]
    public IActionResult GetRecent([FromQuery] int take = 200)
    {
        return Ok(_store.GetRecent(take));
    }

    [HttpPost]
    public IActionResult Ingest([FromBody] ClientLogEntry entry)
    {
        _logger.Log(LogLevelFromClient(entry.Level), "UI: {Message}", entry.Message);
        return Accepted();
    }

    private static LogLevel LogLevelFromClient(string? level) => level?.ToUpperInvariant() switch
    {
        "TRACE" => LogLevel.Trace,
        "DEBUG" => LogLevel.Debug,
        "WARN" or "WARNING" => LogLevel.Warning,
        "ERROR" => LogLevel.Error,
        "CRITICAL" => LogLevel.Critical,
        _ => LogLevel.Information
    };
}

public sealed record ClientLogEntry
{
    public string? Level { get; init; }
    public string Message { get; init; } = string.Empty;
}
