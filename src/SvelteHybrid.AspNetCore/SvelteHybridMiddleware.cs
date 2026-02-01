using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace SvelteHybrid.AspNetCore;

/// <summary>
/// Middleware that automatically injects SvelteHybrid runtime into HTML responses
/// </summary>
public partial class SvelteHybridMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SvelteHybridOptions _options;

    private static readonly string ScriptTag = """
        <!-- SvelteHybrid Runtime -->
        <link rel="stylesheet" href="/_sveltehybrid/css/svelte-hybrid.css" />
        <script src="/_sveltehybrid/js/svelte-hybrid.js" defer></script>
        """;

    private static readonly string ScriptTagNoStyles = """
        <!-- SvelteHybrid Runtime -->
        <script src="/_sveltehybrid/js/svelte-hybrid.js" defer></script>
        """;

    public SvelteHybridMiddleware(RequestDelegate next, SvelteHybridOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip excluded paths
        var path = context.Request.Path.Value ?? "";
        if (_options.ExcludePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Only process GET requests
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Capture response
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        try
        {
            await _next(context);

            // Only process HTML responses
            if (context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
            {
                newBody.Seek(0, SeekOrigin.Begin);
                var html = await new StreamReader(newBody).ReadToEndAsync();

                // Inject script before </head> or </body>
                var injectedHtml = InjectRuntime(html);

                var bytes = Encoding.UTF8.GetBytes(injectedHtml);
                context.Response.ContentLength = bytes.Length;
                await originalBody.WriteAsync(bytes);
            }
            else
            {
                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private string InjectRuntime(string html)
    {
        var scriptToInject = _options.AutoInjectStyles ? ScriptTag : ScriptTagNoStyles;

        // Try to inject before </head>
        if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
        {
            return HeadEndRegex().Replace(html, $"{scriptToInject}\n</head>", 1);
        }

        // Fallback: inject before </body>
        if (html.Contains("</body>", StringComparison.OrdinalIgnoreCase))
        {
            return BodyEndRegex().Replace(html, $"{scriptToInject}\n</body>", 1);
        }

        // Last resort: append to end
        return html + scriptToInject;
    }

    [GeneratedRegex(@"</head>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadEndRegex();

    [GeneratedRegex(@"</body>", RegexOptions.IgnoreCase)]
    private static partial Regex BodyEndRegex();
}
