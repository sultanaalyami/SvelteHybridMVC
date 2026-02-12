namespace HRCE.Core.Platform;

public interface IPlatformDetectionService
{
    PlatformInfo GetPlatformInfo();
    MonitoringSystemType GetAvailableMonitoringSystem();
}

public class PlatformDetectionService : IPlatformDetectionService
{
    public PlatformInfo GetPlatformInfo() => new()
    {
        OSDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
        Architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),
        MonitoringSystem = GetAvailableMonitoringSystem().ToString(),
        IsContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true",
        KernelVersion = Environment.OSVersion.Version.ToString()
    };

    public MonitoringSystemType GetAvailableMonitoringSystem()
    {
        if (OperatingSystem.IsLinux()) return MonitoringSystemType.EbpfLinux;
        if (OperatingSystem.IsWindows()) return MonitoringSystemType.EtwWindows;
        if (OperatingSystem.IsMacOS()) return MonitoringSystemType.DTraceMacOS;
        return MonitoringSystemType.None;
    }
}

public class PlatformInfo
{
    public string OSDescription { get; init; } = "";
    public string Architecture { get; init; } = "";
    public string MonitoringSystem { get; init; } = "";
    public bool IsContainer { get; init; }
    public string KernelVersion { get; init; } = "";
}

public enum MonitoringSystemType
{
    None = 0,
    EbpfLinux = 1,
    EtwWindows = 2,
    DTraceMacOS = 3
}
