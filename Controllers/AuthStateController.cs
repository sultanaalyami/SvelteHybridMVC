using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthStateController : ControllerBase
{
    /// <summary>
    /// SSE endpoint - streams auth state changes to Svelte components
    /// </summary>
    [HttpGet("state")]
    public async Task StreamState(CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var lastState = "";

        while (!cancellationToken.IsCancellationRequested)
        {
            var state = System.Text.Json.JsonSerializer.Serialize(new
            {
                signedIn = User.Identity?.IsAuthenticated ?? false,
                userName = User.Identity?.Name ?? ""
            });

            if (state != lastState)
            {
                await Response.WriteAsync($"data: {state}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                lastState = state;
            }

            await Task.Delay(3000, cancellationToken);
        }
    }

    /// <summary>
    /// One-time check fallback for browsers that don't support SSE
    /// </summary>
    [HttpGet("state/check")]
    public IActionResult CheckState()
    {
        return Ok(new
        {
            signedIn = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name ?? ""
        });
    }
}
