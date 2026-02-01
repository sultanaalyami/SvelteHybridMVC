using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HRCE.Core.Platform;

/// <summary>
/// تطبيق خدمة اكتشاف المنصة
/// </summary>
public class PlatformDetectionService : IPlatformDetectionService
{
    public OSPlatform CurrentPlatform { get; }

    public bool IsEbpfSupported => 
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && HasKernelHeaders();

    public bool IsEtwSupported => 
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public bool IsDTraceSupported => 
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public PlatformDetectionService()
    {
        CurrentPlatform = GetCurrentPlatform();
    }

    public MonitoringSystemType GetAvailableMonitoringSystem()
    {
        if (IsEbpfSupported)
        {
            return MonitoringSystemType.EbpfLinux;
        }

        if (IsEtwSupported)
        {
            // التحقق من دعم eBPF for Windows (تجريبي)
            if (IsEbpfForWindowsAvailable())
            {
                return MonitoringSystemType.EbpfWindows;
            }
            return MonitoringSystemType.EtwWindows;
        }

        if (IsDTraceSupported)
        {
            return MonitoringSystemType.DTraceMacOS;
        }

        return MonitoringSystemType.None;
    }

    public bool HasKernelHeaders()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return false;
        }

        try
        {
            var kernelVersion = GetKernelVersion();
            var headersPath = $"/usr/src/linux-headers-{kernelVersion}";
            return Directory.Exists(headersPath) || Directory.Exists("/usr/include/linux");
        }
        catch
        {
            return false;
        }
    }

    public PlatformInfo GetPlatformInfo()
    {
        return new PlatformInfo
        {
            OSDescription = RuntimeInformation.OSDescription,
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            Platform = CurrentPlatform,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            IsContainer = IsRunningInContainer(),
            KernelVersion = GetKernelVersion(),
            MonitoringSystem = GetAvailableMonitoringSystem()
        };
    }

    private static OSPlatform GetCurrentPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OSPlatform.Windows;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OSPlatform.OSX;

        return OSPlatform.Create("Unknown");
    }

    private static bool IsRunningInContainer()
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || File.Exists("/.dockerenv")
            || File.Exists("/run/.containerenv");
    }

    private static string GetKernelVersion()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var version = File.ReadAllText("/proc/version");
                var parts = version.Split(' ');
                return parts.Length > 2 ? parts[2] : "unknown";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.OSVersion.Version.ToString();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "uname",
                    Arguments = "-r",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return output;
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return "unknown";
    }

    private static bool IsEbpfForWindowsAvailable()
    {
        try
        {
            // التحقق من وجود eBPF for Windows driver
            var driverPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "drivers",
                "ebpf.sys"
            );
            return File.Exists(driverPath);
        }
        catch
        {
            return false;
        }
    }
}
