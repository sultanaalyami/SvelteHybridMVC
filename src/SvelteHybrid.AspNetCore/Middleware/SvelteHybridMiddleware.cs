using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SvelteHybrid.AspNetCore.Abstractions;
using System.Text;

namespace SvelteHybrid.AspNetCore.Middleware;

/// <summary>
/// Middleware that processes Svelte directives in Razor responses.
/// </summary>
public class SvelteHybridMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SvelteHybridMiddleware> _logger;

    public SvelteHybridMiddleware(
        RequestDelegate next, 
        ILogger<SvelteHybridMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IHybridViewEngine hybridEngine)
    {
        // Skip non-page requests
        if (!ShouldProcessRequest(context))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Only process HTML responses
            if (IsHtmlResponse(context))
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                // Check if there are any Svelte directives
                if (responseText.Contains("@svelte"))
                {
                    _logger.LogDebug("Processing Svelte directives in response");
                    
                    var processedHtml = await hybridEngine.RenderHybridAsync(
                        responseText, 
                        new { },
                        context.RequestAborted);

                    var processedBytes = Encoding.UTF8.GetBytes(processedHtml);
                    context.Response.ContentLength = processedBytes.Length;
                    
                    await originalBodyStream.WriteAsync(processedBytes, context.RequestAborted);
                }
                else
                {
                    // No directives, copy as-is
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
                }
            }
            else
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Request was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Svelte directives");
            context.Response.Body = originalBodyStream;
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static bool ShouldProcessRequest(HttpContext context)
    {
        // Skip API requests, static files, etc.
        var path = context.Request.Path.Value ?? "";
        
        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return false;
            
        if (path.StartsWith("/_", StringComparison.OrdinalIgnoreCase))
            return false;

        // Process only GET requests by default
        return HttpMethods.IsGet(context.Request.Method);
    }

    private static bool IsHtmlResponse(HttpContext context)
    {
        return context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true;
    }
}
