using Microsoft.AspNetCore.Mvc;
using HRCE.Core.Monitoring;
using System.Diagnostics;

namespace HRCE.Controllers;

/// <summary>
/// واجهة API لأدوات المراقب الكمي - تأثيرات حقيقية قابلة للقياس
/// Quantum Observer Tools API - Real measurable effects
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class QuantumToolsController : ControllerBase
{
    private readonly QuantumObserverTools _tools;
    private readonly IKernelMonitor _kernelMonitor;
    private readonly ILogger<QuantumToolsController> _logger;

    public QuantumToolsController(
        QuantumObserverTools tools,
        IKernelMonitor kernelMonitor,
        ILogger<QuantumToolsController> logger)
    {
        _tools = tools;
        _kernelMonitor = kernelMonitor;
        _logger = logger;
    }

    /// <summary>
    /// انهيار دالة الموجة - يفرض GC ويحرر الذاكرة
    /// Wave function collapse - Forces GC and frees memory
    /// </summary>
    [HttpPost("collapse")]
    public async Task<ActionResult<MeasurementResult>> CollapseWaveFunction([FromQuery] int generation = 2)
    {
        _logger.LogInformation("Wave function collapse requested (generation: {Gen})", generation);
        var result = await _tools.CollapseWaveFunctionAsync(Math.Clamp(generation, 0, 2));
        return Ok(result);
    }

    /// <summary>
    /// إنشاء تشابك كمي - إنشاء عملية مرتبطة
    /// Create quantum entanglement - Create linked process
    /// </summary>
    [HttpPost("entangle")]
    public async Task<ActionResult<MeasurementResult>> CreateEntanglement([FromBody] EntanglementRequest? request)
    {
        _logger.LogInformation("Quantum entanglement requested");
        
        // استخدام أمر آمن افتراضي
        var command = request?.Command ?? (OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh");
        var args = request?.Arguments ?? (OperatingSystem.IsWindows() ? "/c echo Quantum_Entangled_%TIME%" : "-c \"echo Quantum_Entangled_$(date +%s)\"");
        
        var result = await _tools.CreateEntanglementAsync(command, args);
        return Ok(result);
    }

    /// <summary>
    /// تعديل التماسك - تغيير أولوية العملية
    /// Adjust coherence - Change process priority
    /// </summary>
    [HttpPost("coherence")]
    public ActionResult<MeasurementResult> AdjustCoherence([FromQuery] string level = "normal")
    {
        var priority = level.ToLower() switch
        {
            "high" => ProcessPriorityClass.High,
            "above" => ProcessPriorityClass.AboveNormal,
            "below" => ProcessPriorityClass.BelowNormal,
            "idle" => ProcessPriorityClass.Idle,
            _ => ProcessPriorityClass.Normal
        };

        _logger.LogInformation("Coherence adjustment requested: {Level}", level);
        var result = _tools.AdjustCoherence(priority);
        return Ok(result);
    }

    /// <summary>
    /// إنشاء تراكب كمي - تخصيص ذاكرة متعددة الحالات
    /// Create superposition - Multi-state memory allocation
    /// </summary>
    [HttpPost("superposition")]
    public async Task<ActionResult<MeasurementResult>> CreateSuperposition([FromBody] SuperpositionRequest? request)
    {
        var stateCount = Math.Clamp(request?.StateCount ?? 10, 1, 100);
        var stateSize = Math.Clamp(request?.StateSize ?? 1024 * 1024, 1024, 10 * 1024 * 1024);

        _logger.LogInformation("Superposition creation requested: {Count} states of {Size} bytes", stateCount, stateSize);
        var result = await _tools.CreateSuperpositionAsync(stateCount, stateSize);
        return Ok(result);
    }

    /// <summary>
    /// قياس الحالة الكمية - قراءة حالة النظام الحالية
    /// Quantum measurement - Read current system state
    /// </summary>
    [HttpGet("measure")]
    public ActionResult<MeasurementResult> MeasureState()
    {
        _logger.LogInformation("Quantum measurement requested");
        var result = _tools.MeasureSystemState();
        return Ok(result);
    }

    /// <summary>
    /// تنظيم البيانات - ضغط الذاكرة وتحسينها
    /// Organize data - Compact and optimize memory
    /// </summary>
    [HttpPost("organize")]
    public async Task<ActionResult<MeasurementResult>> OrganizeData()
    {
        _logger.LogInformation("Data organization requested");
        var result = await _tools.OrganizeDataAsync();
        return Ok(result);
    }

    /// <summary>
    /// فحص العمليات - كشف العمليات المشبوهة
    /// Process scan - Detect suspicious processes
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<MeasurementResult>> ScanProcesses([FromBody] ScanRequest? request)
    {
        _logger.LogInformation("Process scan requested");
        
        // معايير الكشف الافتراضية
        Func<Process, bool> isSuspicious = p =>
        {
            try
            {
                // العمليات التي تستهلك ذاكرة كبيرة جداً
                if (p.WorkingSet64 > (request?.MemoryThresholdMB ?? 2000) * 1024 * 1024)
                    return true;

                // العمليات بدون نافذة واسم مشبوه
                if (request?.SuspiciousNames != null && 
                    request.SuspiciousNames.Any(n => p.ProcessName.Contains(n, StringComparison.OrdinalIgnoreCase)))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        };

        var result = await _tools.RejectHarmfulProcessAsync(isSuspicious);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على جميع القياسات
    /// Get all measurements
    /// </summary>
    [HttpGet("measurements")]
    public ActionResult<IEnumerable<MeasurementResult>> GetMeasurements()
    {
        return Ok(_tools.GetAllMeasurements());
    }

    /// <summary>
    /// تصدير بيانات التحقق الخارجي
    /// Export external verification data
    /// </summary>
    [HttpGet("export")]
    public ActionResult<ExternalVerificationData> ExportVerification()
    {
        return Ok(_tools.ExportForVerification());
    }

    /// <summary>
    /// الحصول على إحصائيات المراقبة
    /// Get monitoring statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<MonitoringStats>> GetStats()
    {
        var stats = await _kernelMonitor.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// بث الأحداث الحقيقية في الوقت الفعلي
    /// Stream real events in real-time
    /// </summary>
    [HttpGet("stream")]
    public async Task StreamRealEvents(CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        await foreach (var evt in _kernelMonitor.StreamEventsAsync(cancellationToken))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                id = Guid.NewGuid().ToString(),
                timestamp = evt.Timestamp.ToString("O"),
                type = evt.EventType,
                processId = evt.ProcessId,
                data = evt.Data,
                // تصنيف كمي
                quantumProperties = new
                {
                    coherence = CalculateEventCoherence(evt),
                    energy = CalculateEventEnergy(evt),
                    entanglement = DetectEntanglement(evt),
                    waveFunction = GetWaveFunctionPhase(evt)
                }
            });

            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    /// <summary>
    /// تشغيل سيناريو تجريبي كامل
    /// Run full experimental scenario
    /// </summary>
    [HttpPost("experiment")]
    public async Task<ActionResult<ExperimentResult>> RunExperiment([FromBody] ExperimentRequest request)
    {
        _logger.LogInformation("Running quantum experiment: {Name}", request.Name);
        var results = new List<MeasurementResult>();
        var startTime = Stopwatch.StartNew();

        // 1. قياس الحالة الأولية
        results.Add(_tools.MeasureSystemState());

        // 2. تنفيذ الإجراءات المطلوبة
        foreach (var action in request.Actions)
        {
            MeasurementResult? result = action.Type switch
            {
                "collapse" => await _tools.CollapseWaveFunctionAsync(action.Generation ?? 2),
                "superposition" => await _tools.CreateSuperpositionAsync(action.StateCount ?? 10, action.StateSize ?? 1024 * 1024),
                "organize" => await _tools.OrganizeDataAsync(),
                "measure" => _tools.MeasureSystemState(),
                _ => null
            };

            if (result != null)
            {
                results.Add(result);
            }

            // تأخير بين الإجراءات
            await Task.Delay(request.DelayBetweenActionsMs);
        }

        // 3. قياس الحالة النهائية
        results.Add(_tools.MeasureSystemState());

        startTime.Stop();

        return Ok(new ExperimentResult
        {
            Name = request.Name,
            TotalDurationMs = startTime.ElapsedMilliseconds,
            ActionCount = request.Actions.Length,
            Results = results,
            Summary = GenerateExperimentSummary(results)
        });
    }

    // Helper methods
    private static double CalculateEventCoherence(KernelEvent evt)
    {
        // التماسك بناءً على نوع الحدث
        return evt.EventType switch
        {
            var t when t.Contains("gc") => 0.3,  // GC يقلل التماسك
            var t when t.Contains("network") => 0.8,
            var t when t.Contains("process") => 0.6,
            _ => 0.7
        };
    }

    private static double CalculateEventEnergy(KernelEvent evt)
    {
        if (evt.Data.TryGetValue("bytes", out var bytes) && bytes is int b)
            return Math.Min(1.0, Math.Log10(b + 1) / 5);
        
        if (evt.Data.TryGetValue("working_set", out var ws) && ws is long wsVal)
            return Math.Min(1.0, Math.Log10(wsVal + 1) / 10);
        
        return 0.5;
    }

    private static string DetectEntanglement(KernelEvent evt)
    {
        if (evt.Data.TryGetValue("src_ip", out _) && evt.Data.TryGetValue("dst_ip", out _))
            return "network_pair";
        
        if (evt.Data.TryGetValue("parent_pid", out _))
            return "process_tree";
        
        return "none";
    }

    private static double GetWaveFunctionPhase(KernelEvent evt)
    {
        return (evt.Timestamp.Ticks % 6283) / 1000.0;
    }

    private static ExperimentSummary GenerateExperimentSummary(List<MeasurementResult> results)
    {
        var first = results.FirstOrDefault(r => r.ActionType == "quantum_measurement");
        var last = results.LastOrDefault(r => r.ActionType == "quantum_measurement");

        return new ExperimentSummary
        {
            InitialHeapSize = first?.After?.HeapSize ?? 0,
            FinalHeapSize = last?.After?.HeapSize ?? 0,
            TotalMemoryFreed = (first?.After?.HeapSize ?? 0) - (last?.After?.HeapSize ?? 0),
            GcCollectionsTriggered = results.Count(r => r.ActionType == "wave_collapse"),
            SuperpositionsCreated = results.Count(r => r.ActionType == "superposition_creation"),
            OrganizationsPerformed = results.Count(r => r.ActionType == "data_organization")
        };
    }
}

// Request/Response models
public record EntanglementRequest
{
    public string? Command { get; init; }
    public string? Arguments { get; init; }
}

public record SuperpositionRequest
{
    public int StateCount { get; init; } = 10;
    public int StateSize { get; init; } = 1024 * 1024;
}

public record ScanRequest
{
    public int MemoryThresholdMB { get; init; } = 2000;
    public string[]? SuspiciousNames { get; init; }
}

public record ExperimentRequest
{
    public required string Name { get; init; }
    public required ExperimentAction[] Actions { get; init; }
    public int DelayBetweenActionsMs { get; init; } = 100;
}

public record ExperimentAction
{
    public required string Type { get; init; }
    public int? Generation { get; init; }
    public int? StateCount { get; init; }
    public int? StateSize { get; init; }
}

public record ExperimentResult
{
    public required string Name { get; init; }
    public long TotalDurationMs { get; init; }
    public int ActionCount { get; init; }
    public required List<MeasurementResult> Results { get; init; }
    public required ExperimentSummary Summary { get; init; }
}

public record ExperimentSummary
{
    public long InitialHeapSize { get; init; }
    public long FinalHeapSize { get; init; }
    public long TotalMemoryFreed { get; init; }
    public int GcCollectionsTriggered { get; init; }
    public int SuperpositionsCreated { get; init; }
    public int OrganizationsPerformed { get; init; }
}
