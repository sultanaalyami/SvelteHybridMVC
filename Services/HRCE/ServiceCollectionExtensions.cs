namespace HRCE.Services.HRCE;

/// <summary>
/// امتدادات لتسجيل خدمات HRCE
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// إضافة خدمات HRCE إلى DI Container
    /// </summary>
    public static IServiceCollection AddHRCE(
        this IServiceCollection services,
        Action<HRCEOptions>? configureOptions = null)
    {
        // تسجيل الخيارات
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<HRCEOptions>(options => { });
        }

        // تسجيل الخدمات الأساسية
        services.AddSingleton<IIsolationService, IsolationService>();
        services.AddScoped<ISvelteRenderer, SvelteRenderer>();
        services.AddScoped<IHybridViewEngine, HybridViewEngine>();

        return services;
    }
}
