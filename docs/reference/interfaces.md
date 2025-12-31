# API Interfaces Reference

This document provides detailed reference for all HRCE interfaces and types.

---

## Core Interfaces

### ISvelteRenderer

Main interface for rendering Svelte components on the server.

```csharp
namespace SvelteHybridMVC.Core;

public interface ISvelteRenderer
{
    /// <summary>
    /// Renders a Svelte component with the provided props.
    /// </summary>
    /// <param name="componentName">Name of the component (e.g., "_ProductCard")</param>
    /// <param name="props">Props to pass to the component (typically a Presentation Model)</param>
    /// <returns>Rendered HTML string</returns>
    Task<string> RenderAsync(string componentName, object? props = null);
}
```

**Parameters:**
- `componentName`: Component file name without extension
- `props`: Any serializable object (typically a Presentation Model)

**Returns:**
- HTML string with rendered component

**Exceptions:**
- `ComponentNotFoundException`: Component file not found
- `RenderException`: Component failed to render
- `ArgumentNullException`: componentName is null

**Example:**
```csharp
var model = new ProductCardModel { Title = "Product" };
var html = await _svelteRenderer.RenderAsync("_ProductCard", model);
```

---

### IHybridViewEngine

Interface for the hybrid view engine (future implementation).

```csharp
namespace SvelteHybridMVC.Core;

public interface IHybridViewEngine
{
    /// <summary>
    /// Renders a view with hybrid processing.
    /// </summary>
    Task<string> RenderAsync(
        string viewName, 
        object model, 
        ViewContext context
    );
    
    /// <summary>
    /// Checks if a component exists.
    /// </summary>
    bool ComponentExists(string componentName);
}
```

---

### IComponentRenderer

Generic interface for component renderers (extensibility point).

```csharp
namespace SvelteHybridMVC.Core;

public interface IComponentRenderer
{
    /// <summary>
    /// Renders a component.
    /// </summary>
    Task<string> RenderAsync(string componentName, object? model);
    
    /// <summary>
    /// Checks if this renderer can render the given component.
    /// </summary>
    bool CanRender(string componentName);
    
    /// <summary>
    /// The name of this renderer (e.g., "Svelte", "React", "Vue").
    /// </summary>
    string RendererName { get; }
}
```

**Usage:**
```csharp
public class ReactRenderer : IComponentRenderer
{
    public string RendererName => "React";
    
    public bool CanRender(string componentName)
    {
        var path = Path.Combine(_options.ComponentDir, $"{componentName}.jsx");
        return File.Exists(path);
    }
    
    public async Task<string> RenderAsync(string componentName, object? model)
    {
        // React SSR implementation
    }
}
```

---

## Configuration Types

### SvelteOptions

Configuration options for the Svelte renderer.

```csharp
namespace SvelteHybridMVC.Core;

public class SvelteOptions
{
    /// <summary>
    /// Enable server-side rendering. Default: true.
    /// </summary>
    public bool EnableSSR { get; set; } = true;
    
    /// <summary>
    /// Hydration mode for client-side interactivity.
    /// </summary>
    public HydrationMode HydrationMode { get; set; } = HydrationMode.Selective;
    
    /// <summary>
    /// Watch for file changes and auto-rebuild (dev mode). Default: false.
    /// </summary>
    public bool WatchForChanges { get; set; } = false;
    
    /// <summary>
    /// Output path for built Svelte files. Default: "wwwroot/_svelte".
    /// </summary>
    public string OutputPath { get; set; } = "wwwroot/_svelte";
    
    /// <summary>
    /// Directory containing Svelte components. Default: "SvelteApp/src".
    /// </summary>
    public string ComponentDir { get; set; } = "SvelteApp/src";
    
    /// <summary>
    /// Size of the V8 engine pool for SSR. Default: 4.
    /// </summary>
    public int V8PoolSize { get; set; } = 4;
    
    /// <summary>
    /// Enable detailed logging. Default: false.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
    
    /// <summary>
    /// Cache rendered components. Default: true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// Cache duration in minutes. Default: 60.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;
}
```

---

### HydrationMode

Enum for controlling client-side hydration.

```csharp
namespace SvelteHybridMVC.Core;

public enum HydrationMode
{
    /// <summary>
    /// No hydration - components are static HTML only.
    /// Smallest bundle size, no client-side interactivity.
    /// </summary>
    None,
    
    /// <summary>
    /// Selective hydration - only components that need interactivity are hydrated.
    /// Recommended for most applications.
    /// </summary>
    Selective,
    
    /// <summary>
    /// Full hydration - all components become interactive on client.
    /// Largest bundle size, maximum interactivity.
    /// </summary>
    Full
}
```

---

## Presentation Models

### Base Interfaces

```csharp
namespace SvelteHybridMVC.Presentation;

/// <summary>
/// Marker interface for presentation models.
/// </summary>
public interface IPresentationModel
{
    // Marker interface - no methods
}

/// <summary>
/// Base interface for cacheable presentation models.
/// </summary>
public interface ICacheablePresentationModel : IPresentationModel
{
    /// <summary>
    /// Gets the cache key for this model.
    /// </summary>
    string GetCacheKey();
}
```

**Example:**
```csharp
public record ProductCardModel : ICacheablePresentationModel
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    
    public string GetCacheKey() => $"ProductCard_{Id}";
}
```

---

## Authorization Attributes

### ComponentAuthorizeAttribute

Attribute for component-level authorization (future implementation).

```csharp
namespace SvelteHybridMVC.Core.Authorization;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentAuthorizeAttribute : Attribute
{
    /// <summary>
    /// Roles that can view this component.
    /// </summary>
    public string? Roles { get; set; }
    
    /// <summary>
    /// Policy that must be satisfied to view this component.
    /// </summary>
    public string? Policy { get; set; }
    
    /// <summary>
    /// Users that can view this component.
    /// </summary>
    public string? Users { get; set; }
    
    /// <summary>
    /// Authentication schemes to use.
    /// </summary>
    public string? AuthenticationSchemes { get; set; }
}
```

**Usage:**
```csharp
[ComponentAuthorize(Roles = "Admin,Manager")]
public record AdminDashboardModel : IPresentationModel
{
    // Only Admin and Manager roles can see this component
}
```

---

## Extension Methods

### SvelteServiceExtensions

Extension methods for registering HRCE services.

```csharp
namespace SvelteHybridMVC.Infrastructure.Extensions;

public static class SvelteServiceExtensions
{
    /// <summary>
    /// Adds Svelte Hybrid services to the service collection.
    /// </summary>
    public static IServiceCollection AddSvelteHybrid(
        this IServiceCollection services,
        Action<SvelteOptions>? configure = null)
    {
        var options = new SvelteOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ISvelteRenderer, SvelteRenderer>();

        return services;
    }
    
    /// <summary>
    /// Adds a custom component renderer.
    /// </summary>
    public static IServiceCollection AddComponentRenderer<T>(
        this IServiceCollection services)
        where T : class, IComponentRenderer
    {
        services.AddSingleton<IComponentRenderer, T>();
        return services;
    }
}
```

**Usage:**
```csharp
// Basic setup
services.AddSvelteHybrid();

// With configuration
services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
});

// Add custom renderer
services.AddComponentRenderer<ReactRenderer>();
```

---

## Helper Classes

### ComponentResult

Result type for component rendering operations.

```csharp
namespace SvelteHybridMVC.Core;

public class ComponentResult
{
    /// <summary>
    /// The rendered HTML.
    /// </summary>
    public required string Html { get; init; }
    
    /// <summary>
    /// The component CSS (scoped).
    /// </summary>
    public string? Css { get; init; }
    
    /// <summary>
    /// The component JavaScript (sandboxed).
    /// </summary>
    public string? JavaScript { get; init; }
    
    /// <summary>
    /// Whether rendering was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Error message if rendering failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Additional metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
```

---

## Exceptions

### HRCE-Specific Exceptions

```csharp
namespace SvelteHybridMVC.Core.Exceptions;

/// <summary>
/// Base exception for all HRCE-related errors.
/// </summary>
public class HRCEException : Exception
{
    public HRCEException(string message) : base(message) { }
    public HRCEException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a component is not found.
/// </summary>
public class ComponentNotFoundException : HRCEException
{
    public string ComponentName { get; }
    
    public ComponentNotFoundException(string componentName)
        : base($"Component '{componentName}' not found.")
    {
        ComponentName = componentName;
    }
}

/// <summary>
/// Thrown when component rendering fails.
/// </summary>
public class RenderException : HRCEException
{
    public string ComponentName { get; }
    public object? Model { get; }
    
    public RenderException(string componentName, Exception inner)
        : base($"Failed to render component '{componentName}'.", inner)
    {
        ComponentName = componentName;
    }
}

/// <summary>
/// Thrown when component authorization fails.
/// </summary>
public class ComponentAuthorizationException : HRCEException
{
    public string ComponentName { get; }
    public string? RequiredRole { get; }
    
    public ComponentAuthorizationException(string componentName, string? requiredRole)
        : base($"User not authorized to view component '{componentName}'.")
    {
        ComponentName = componentName;
        RequiredRole = requiredRole;
    }
}
```

---

## Constants

### HRCE Constants

```csharp
namespace SvelteHybridMVC.Core;

public static class HRCEConstants
{
    /// <summary>
    /// The directive used to mark Svelte components in Razor.
    /// </summary>
    public const string SvelteDirective = "@svelte";
    
    /// <summary>
    /// Default component directory.
    /// </summary>
    public const string DefaultComponentDir = "SvelteApp/src";
    
    /// <summary>
    /// Default output directory.
    /// </summary>
    public const string DefaultOutputDir = "wwwroot/_svelte";
    
    /// <summary>
    /// Placeholder component name for errors.
    /// </summary>
    public const string ErrorComponentName = "_Error";
    
    /// <summary>
    /// Cache key prefix for components.
    /// </summary>
    public const string CacheKeyPrefix = "HRCE:Component:";
}
```

---

## Type Aliases

Common type aliases for convenience:

```csharp
namespace SvelteHybridMVC.Core;

/// <summary>
/// Function type for transforming models.
/// </summary>
public delegate TPresentationModel PresenterFunc<in TDomain, out TPresentationModel>(TDomain domain)
    where TPresentationModel : IPresentationModel;

/// <summary>
/// Function type for custom component resolution.
/// </summary>
public delegate string? ComponentResolver(string componentName);
```

---

## Logging Categories

```csharp
namespace SvelteHybridMVC.Core;

public static class LogCategories
{
    public const string Rendering = "HRCE.Rendering";
    public const string Authorization = "HRCE.Authorization";
    public const string Caching = "HRCE.Caching";
    public const string Performance = "HRCE.Performance";
}
```

**Usage:**
```csharp
private readonly ILogger<SvelteRenderer> _logger;

_logger.LogInformation(
    LogCategories.Rendering,
    "Rendering component {ComponentName}",
    componentName
);
```

---

## Version Information

```csharp
namespace SvelteHybridMVC.Core;

public static class HRCEVersion
{
    public const string Version = "1.0.0";
    public const string ApiVersion = "1.0";
    public const string MinimumDotNetVersion = "6.0";
}
```

---

## Next Steps

- [Configuration Guide](configuration.md)
- [Directives and Tags](directives-and-tags.md)
- [Architecture Overview](../concepts/hrce-architecture.md)
