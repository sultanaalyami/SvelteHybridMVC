using HRCE.Services.HRCE;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace HRCE.Middleware;

/// <summary>
/// Middleware لمعالجة توجيهات Svelte في استجابات Razor
/// </summary>
public class HRCEMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HRCEMiddleware> _logger;

    public HRCEMiddleware(RequestDelegate next, ILogger<HRCEMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IHybridViewEngine hybridEngine)
    {
        // تخزين الاستجابة الأصلية
        var originalBodyStream = context.Response.Body;

        try
        {
            // إنشاء stream جديد لالتقاط الاستجابة
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // تنفيذ الـ middleware التالي
            await _next(context);

            // التحقق من نوع المحتوى
            if (context.Response.ContentType?.Contains("text/html") == true)
            {
                // قراءة محتوى الاستجابة
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                // معالجة المحتوى بواسطة HRCE
                var processedHtml = await hybridEngine.RenderHybridAsync(responseText, new { });

                // كتابة المحتوى المعالج
                var processedBytes = Encoding.UTF8.GetBytes(processedHtml);
                context.Response.ContentLength = processedBytes.Length;
                
                await originalBodyStream.WriteAsync(processedBytes);
            }
            else
            {
                // نسخ الاستجابة كما هي للمحتوى غير HTML
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HRCE middleware");
            
            // إعادة الاستجابة الأصلية في حالة الخطأ
            context.Response.Body = originalBodyStream;
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

/// <summary>
/// امتدادات لإضافة HRCE Middleware
/// </summary>
public static class HRCEMiddlewareExtensions
{
    /// <summary>
    /// إضافة HRCE Middleware إلى pipeline
    /// </summary>
    public static IApplicationBuilder UseHRCE(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HRCEMiddleware>();
    }
}
