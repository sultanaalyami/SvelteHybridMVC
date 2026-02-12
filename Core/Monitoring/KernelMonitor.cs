namespace HRCE.Core.Monitoring;

public interface IKernelMonitor
{
    string SystemType { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<MonitoringStats> GetStatsAsync();
}

public class MonitoringStats
{
    public string SystemType { get; init; } = "";
    public bool IsRunning { get; init; }
    public long EventsProcessed { get; init; }
    public DateTime StartedAt { get; init; }
}

public class QuantumObserverTools;

public class EbpfLinuxMonitor(ILogger<EbpfLinuxMonitor> logger) : NullMonitor(logger)
{
    public override string SystemType => "eBPF/Linux";
}

public class EtwWindowsMonitor(ILogger<EtwWindowsMonitor> logger) : NullMonitor(logger)
{
    public override string SystemType => "ETW/Windows";
}

public class RealKernelMonitor(ILogger<RealKernelMonitor> logger) : NullMonitor(logger)
{
    public override string SystemType => "Real/.NET";
}

public class NullMonitor(ILogger logger) : IKernelMonitor
{
    protected readonly ILogger _logger = logger;
    private bool _running;
    private DateTime _startedAt;

    public virtual string SystemType => "Null";

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _running = true;
        _startedAt = DateTime.UtcNow;
        _logger.LogInformation("Monitor ({Type}) started", SystemType);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        _logger.LogInformation("Monitor ({Type}) stopped", SystemType);
        return Task.CompletedTask;
    }

    public Task<MonitoringStats> GetStatsAsync() => Task.FromResult(new MonitoringStats
    {
        SystemType = SystemType,
        IsRunning = _running,
        StartedAt = _startedAt
    });
}
