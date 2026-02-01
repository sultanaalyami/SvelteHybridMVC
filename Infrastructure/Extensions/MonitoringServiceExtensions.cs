using HRCE.Core.Monitoring;
using HRCE.Core.Platform;

namespace HRCE.Infrastructure.Extensions;

/// <summary>
/// امتدادات لتسجيل خدمات المراقبة متعددة المنصات
/// </summary>
public static class MonitoringServiceExtensions
{
    /// <summary>
    /// إضافة خدمات المراقبة مع اكتشاف تلقائي للمنصة
    /// </summary>
    public static IServiceCollection AddPlatformMonitoring(
        this IServiceCollection services,
        Action<MonitoringOptions>? configure = null)
    {
        var options = new MonitoringOptions();
        configure?.Invoke(options);

        // تسجيل خدمة اكتشاف المنصة
        services.AddSingleton<IPlatformDetectionService, PlatformDetectionService>();

        // تسجيل أدوات المراقب الكمي
        services.AddSingleton<QuantumObserverTools>();

        // تسجيل IKernelMonitor بناءً على المنصة
        services.AddSingleton<IKernelMonitor>(provider =>
        {
            var platformService = provider.GetRequiredService<IPlatformDetectionService>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            
            // استخدام RealKernelMonitor للحصول على بيانات حقيقية
            if (options.UseRealMonitor)
            {
                return new RealKernelMonitor(loggerFactory.CreateLogger<RealKernelMonitor>());
            }
            
            var monitoringSystem = platformService.GetAvailableMonitoringSystem();

            return monitoringSystem switch
            {
                MonitoringSystemType.EbpfLinux => 
                    new EbpfLinuxMonitor(loggerFactory.CreateLogger<EbpfLinuxMonitor>()),
                
                MonitoringSystemType.EtwWindows => 
                    new EtwWindowsMonitor(loggerFactory.CreateLogger<EtwWindowsMonitor>()),
                
                MonitoringSystemType.DTraceMacOS => 
                    new NullMonitor(loggerFactory.CreateLogger<NullMonitor>()),
                
                _ => new NullMonitor(loggerFactory.CreateLogger<NullMonitor>())
            };
        });

        // تسجيل Hosted Service لبدء المراقبة تلقائياً
        if (options.AutoStart)
        {
            services.AddHostedService<MonitoringHostedService>();
        }

        return services;
    }
}

/// <summary>
/// خيارات المراقبة
/// </summary>
public class MonitoringOptions
{
    /// <summary>
    /// بدء المراقبة تلقائياً عند بدء التطبيق
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// تفعيل جمع الإحصائيات
    /// </summary>
    public bool EnableStats { get; set; } = true;

    /// <summary>
    /// تفعيل logging للأحداث
    /// </summary>
    public bool EnableEventLogging { get; set; } = false;

    /// <summary>
    /// استخدام المراقب الحقيقي (.NET 10 APIs)
    /// </summary>
    public bool UseRealMonitor { get; set; } = true;
}

/// <summary>
/// Hosted Service لإدارة دورة حياة المراقبة
/// </summary>
public class MonitoringHostedService : IHostedService
{
    private readonly IKernelMonitor _monitor;
    private readonly ILogger<MonitoringHostedService> _logger;

    public MonitoringHostedService(
        IKernelMonitor monitor,
        ILogger<MonitoringHostedService> logger)
    {
        _monitor = monitor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting kernel monitoring ({SystemType})...", _monitor.SystemType);
        
        try
        {
            await _monitor.StartAsync(cancellationToken);
            _logger.LogInformation("Kernel monitoring started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start kernel monitoring");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping kernel monitoring...");
        
        try
        {
            await _monitor.StopAsync(cancellationToken);
            _logger.LogInformation("Kernel monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while stopping kernel monitoring");
        }
    }
}
