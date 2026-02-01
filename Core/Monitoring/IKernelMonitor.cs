using System.Runtime.InteropServices;
using HRCE.Core.Platform;

namespace HRCE.Core.Monitoring;

/// <summary>
/// واجهة موحدة لجميع أنظمة المراقبة (eBPF, ETW, DTrace)
/// </summary>
public interface IKernelMonitor
{
    /// <summary>
    /// نوع نظام المراقبة
    /// </summary>
    MonitoringSystemType SystemType { get; }

    /// <summary>
    /// هل النظام مفعل ويعمل؟
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// تفعيل المراقبة
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// إيقاف المراقبة
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ربط معرف تذكرة التنفيذ بعملية النواة
    /// </summary>
    /// <param name="ticketId">معرف التذكرة</param>
    /// <param name="processId">معرف العملية (Thread ID أو Process ID)</param>
    Task TagProcessAsync(Guid ticketId, uint processId);

    /// <summary>
    /// الحصول على إحصائيات المراقبة
    /// </summary>
    Task<MonitoringStats> GetStatsAsync();

    /// <summary>
    /// الحصول على الأحداث من النواة
    /// </summary>
    IAsyncEnumerable<KernelEvent> StreamEventsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// حدث من نواة النظام
/// </summary>
public record KernelEvent
{
    public required Guid TicketId { get; init; }
    public required uint ProcessId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; }
    public required Dictionary<string, object> Data { get; init; }
}

/// <summary>
/// إحصائيات المراقبة
/// </summary>
public record MonitoringStats
{
    public required long EventsProcessed { get; init; }
    public required long EventsDropped { get; init; }
    public required TimeSpan Uptime { get; init; }
    public required double EventsPerSecond { get; init; }
    public required Dictionary<string, long> EventTypeCounts { get; init; }
}
