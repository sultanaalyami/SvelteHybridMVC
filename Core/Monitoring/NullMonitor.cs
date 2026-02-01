using HRCE.Core.Platform;

namespace HRCE.Core.Monitoring;

/// <summary>
/// Monitor خالي من الوظائف للأنظمة غير المدعومة
/// </summary>
public class NullMonitor : IKernelMonitor
{
    public MonitoringSystemType SystemType => MonitoringSystemType.None;
    public bool IsActive => false;

    private readonly ILogger<NullMonitor> _logger;

    public NullMonitor(ILogger<NullMonitor> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Kernel monitoring is not supported on this platform");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TagProcessAsync(Guid ticketId, uint processId)
    {
        // لا شيء للقيام به
        return Task.CompletedTask;
    }

    public Task<MonitoringStats> GetStatsAsync()
    {
        return Task.FromResult(new MonitoringStats
        {
            EventsProcessed = 0,
            EventsDropped = 0,
            Uptime = TimeSpan.Zero,
            EventsPerSecond = 0,
            EventTypeCounts = new Dictionary<string, long>()
        });
    }

    public async IAsyncEnumerable<KernelEvent> StreamEventsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // لا توجد أحداث
        await Task.CompletedTask;
        yield break;
    }
}
