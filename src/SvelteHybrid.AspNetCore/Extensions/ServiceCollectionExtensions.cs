using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SvelteHybrid.AspNetCore.Abstractions;
using SvelteHybrid.AspNetCore.Middleware;
using SvelteHybrid.AspNetCore.Services;

namespace SvelteHybrid.AspNetCore;

/// <summary>
/// Extension methods for registering SvelteHybrid services.
/// </summary>
public static class SvelteHybridServiceCollectionExtensions
{
    /// <summary>
    /// Adds SvelteHybrid services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSvelteHybrid(
        this IServiceCollection services,
        Action<SvelteHybridOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<SvelteHybridOptions>(_ => { });
        }

        // Register core services
        services.AddSingleton<IIsolationService, IsolationService>();
        services.AddScoped<ISvelteRenderer, SvelteRenderer>();
        services.AddScoped<IHybridViewEngine, HybridViewEngine>();

        // Register HTTP client for Node service
        services.AddHttpClient<INodeService, NodeService>();

        // Ensure required services
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds SvelteHybrid services with configuration from IConfiguration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSvelteHybrid(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<SvelteHybridOptions>(configuration);
        
        // Register core services
        services.AddSingleton<IIsolationService, IsolationService>();
        services.AddScoped<ISvelteRenderer, SvelteRenderer>();
        services.AddScoped<IHybridViewEngine, HybridViewEngine>();
        services.AddHttpClient<INodeService, NodeService>();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddAuthorization();

        return services;
    }
}

/// <summary>
/// Extension methods for adding SvelteHybrid middleware.
/// </summary>
public static class SvelteHybridApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the SvelteHybrid middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSvelteHybrid(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SvelteHybridMiddleware>();
    }

    /// <summary>
    /// Adds the SvelteHybrid middleware with custom configuration.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSvelteHybrid(
        this IApplicationBuilder app,
        Action<SvelteHybridMiddlewareOptions> configure)
    {
        var options = new SvelteHybridMiddlewareOptions();
        configure(options);

        if (options.PathFilter != null)
        {
            return app.UseWhen(
                context => options.PathFilter(context.Request.Path),
                builder => builder.UseMiddleware<SvelteHybridMiddleware>());
        }

        return app.UseMiddleware<SvelteHybridMiddleware>();
    }
}

/// <summary>
/// Options for SvelteHybrid middleware.
/// </summary>
public class SvelteHybridMiddlewareOptions
{
    /// <summary>
    /// Filter function to determine which paths should be processed.
    /// </summary>
    public Func<PathString, bool>? PathFilter { get; set; }

    /// <summary>
    /// Enable processing for specific path prefixes only.
    /// </summary>
    public List<string> IncludePaths { get; set; } = [];

    /// <summary>
    /// Exclude specific path prefixes from processing.
    /// </summary>
    public List<string> ExcludePaths { get; set; } = ["/api", "/_framework", "/_blazor"];
}
