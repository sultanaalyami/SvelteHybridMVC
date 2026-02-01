namespace HRCE.Services.HRCE;

/// <summary>
/// خيارات تكوين محرك HRCE
/// </summary>
public class HRCEOptions
{
    /// <summary>
    /// تفعيل عرض Svelte من جانب الخادم
    /// </summary>
    public bool EnableSvelteSSR { get; set; } = true;

    /// <summary>
    /// المسار الأساسي للمكونات
    /// </summary>
    public string ComponentBasePath { get; set; } = "~/Components";

    /// <summary>
    /// وضع العزل للمكونات
    /// </summary>
    public IsolationMode IsolationMode { get; set; } = IsolationMode.Full;

    /// <summary>
    /// نوع العارض الافتراضي
    /// </summary>
    public RendererType DefaultRenderer { get; set; } = RendererType.Svelte;

    /// <summary>
    /// تفعيل إعادة التحميل السريع
    /// </summary>
    public bool EnableHotReload { get; set; } = false;

    /// <summary>
    /// إعدادات التفويض
    /// </summary>
    public AuthorizationOptions Authorization { get; set; } = new();
}

/// <summary>
/// أوضاع العزل المتاحة
/// </summary>
public enum IsolationMode
{
    /// <summary>
    /// عزل كامل (CSS, JS, Runtime)
    /// </summary>
    Full,

    /// <summary>
    /// عزل جزئي (CSS, JS فقط)
    /// </summary>
    Partial,

    /// <summary>
    /// بدون عزل
    /// </summary>
    None
}

/// <summary>
/// أنواع العارض المدعومة
/// </summary>
public enum RendererType
{
    Svelte,
    React,
    Vue,
    RazorOnly
}

/// <summary>
/// خيارات التفويض على مستوى المكونات
/// </summary>
public class AuthorizationOptions
{
    /// <summary>
    /// السياسة الافتراضية
    /// </summary>
    public string DefaultPolicy { get; set; } = "AuthenticatedUser";

    /// <summary>
    /// مكون الاحتياطي عند فشل التفويض
    /// </summary>
    public string FallbackComponent { get; set; } = "_Unauthorized";
}
