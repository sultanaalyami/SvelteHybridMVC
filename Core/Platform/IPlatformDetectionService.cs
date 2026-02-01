using System.Runtime.InteropServices;

namespace HRCE.Core.Platform;

/// <summary>
/// خدمة اكتشاف المنصة والتحقق من القدرات المتاحة
/// </summary>
public interface IPlatformDetectionService
{
    /// <summary>
    /// نظام التشغيل الحالي
    /// </summary>
    OSPlatform CurrentPlatform { get; }

    /// <summary>
    /// هل eBPF مدعوم على هذا النظام؟
    /// </summary>
    bool IsEbpfSupported { get; }

    /// <summary>
    /// هل ETW (Windows) مدعوم؟
    /// </summary>
    bool IsEtwSupported { get; }

    /// <summary>
    /// هل DTrace (macOS) مدعوم؟
    /// </summary>
    bool IsDTraceSupported { get; }

    /// <summary>
    /// الحصول على نوع نظام المراقبة المتاح
    /// </summary>
    MonitoringSystemType GetAvailableMonitoringSystem();

    /// <summary>
    /// التحقق من وجود Kernel Headers (لـ eBPF)
    /// </summary>
    bool HasKernelHeaders();

    /// <summary>
    /// الحصول على معلومات النظام
    /// </summary>
    PlatformInfo GetPlatformInfo();
}

/// <summary>
/// نوع نظام المراقبة المتاح
/// </summary>
public enum MonitoringSystemType
{
    /// <summary>
    /// لا يوجد نظام مراقبة متاح
    /// </summary>
    None,

    /// <summary>
    /// eBPF (Linux)
    /// </summary>
    EbpfLinux,

    /// <summary>
    /// ETW - Event Tracing for Windows
    /// </summary>
    EtwWindows,

    /// <summary>
    /// DTrace (macOS)
    /// </summary>
    DTraceMacOS,

    /// <summary>
    /// eBPF for Windows (تجريبي)
    /// </summary>
    EbpfWindows
}

/// <summary>
/// معلومات المنصة
/// </summary>
public record PlatformInfo
{
    public required string OSDescription { get; init; }
    public required string RuntimeIdentifier { get; init; }
    public required OSPlatform Platform { get; init; }
    public required string Architecture { get; init; }
    public required bool IsContainer { get; init; }
    public required string KernelVersion { get; init; }
    public required MonitoringSystemType MonitoringSystem { get; init; }
}
