namespace HRCE.Services.HRCE;

/// <summary>
/// خدمة العزل للمكونات
/// </summary>
public interface IIsolationService
{
    /// <summary>
    /// تطبيق العزل على محتوى المكون
    /// </summary>
    string ApplyIsolation(string componentHtml, string componentName);

    /// <summary>
    /// توليد hash فريد للمكون
    /// </summary>
    string GenerateComponentHash(string componentName);

    /// <summary>
    /// عزل CSS classes
    /// </summary>
    string IsolateCSS(string css, string hash);

    /// <summary>
    /// عزل JavaScript selectors
    /// </summary>
    string IsolateJavaScript(string js, string hash);
}
