# Configuration Guide

This guide covers all configuration options for HRCE.

---

## Basic Configuration

### Minimal Setup

```csharp
// Program.cs
builder.Services.AddSvelteHybrid();
```

This uses all default values:
- SSR enabled
- Selective hydration
- No file watching
- Default paths

---

## SvelteOptions Reference

### EnableSSR

**Type:** `bool`  
**Default:** `true`

Enables server-side rendering of Svelte components.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = false; // Disable SSR, use client-only rendering
});
```

**Use Cases:**
- `true`: SEO-friendly, fast initial load (recommended)
- `false`: SPA-like behavior, all rendering on client

---

### HydrationMode

**Type:** `HydrationMode` enum  
**Default:** `HydrationMode.Selective`

Controls how components become interactive on the client.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.HydrationMode = HydrationMode.Full;
});
```

**Options:**

| Mode | Description | Bundle Size | Use Case |
|------|-------------|-------------|----------|
| `None` | No hydration | Smallest | Static content only |
| `Selective` | Hydrate on-demand | Medium | Most applications |
| `Full` | Hydrate everything | Largest | Highly interactive apps |

---

### WatchForChanges

**Type:** `bool`  
**Default:** `false`

Watch for file changes and auto-rebuild components.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.WatchForChanges = builder.Environment.IsDevelopment();
});
```

**Recommended:**
- Development: `true`
- Production: `false`

---

### OutputPath

**Type:** `string`  
**Default:** `"wwwroot/_svelte"`

Directory for built Svelte components.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.OutputPath = "wwwroot/dist/components";
});
```

**Important:**
- Must be under `wwwroot` for static file serving
- Should be added to `.gitignore`

---

### ComponentDir

**Type:** `string`  
**Default:** `"SvelteApp/src"`

Directory containing Svelte source files.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.ComponentDir = "ClientApp/components";
});
```

---

### V8PoolSize

**Type:** `int`  
**Default:** `4`

Number of V8 engine instances for SSR.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.V8PoolSize = Environment.ProcessorCount; // Match CPU cores
});
```

**Guidelines:**
- Development: 2-4
- Production: 4-8 or match CPU cores
- More instances = more memory, better concurrency

---

### VerboseLogging

**Type:** `bool`  
**Default:** `false`

Enable detailed logging for debugging.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.VerboseLogging = builder.Environment.IsDevelopment();
});
```

**Logs include:**
- Component resolution paths
- Render timings
- Model serialization
- Error stack traces

---

### EnableCaching

**Type:** `bool`  
**Default:** `true`

Cache rendered components.

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableCaching = !builder.Environment.IsDevelopment();
});
```

**Impact:**
- `true`: 97% faster renders (after first)
- `false`: Always fresh, slower

---

### CacheDurationMinutes

**Type:** `int`  
**Default:** `60`

How long to cache rendered components (in minutes).

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.CacheDurationMinutes = 5; // 5 minutes for frequently changing data
});
```

---

## Environment-Specific Configuration

### Development

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSvelteHybrid(options =>
    {
        options.EnableSSR = true;
        options.HydrationMode = HydrationMode.Full;
        options.WatchForChanges = true;
        options.VerboseLogging = true;
        options.EnableCaching = false; // Always fresh in dev
        options.V8PoolSize = 2;
    });
}
```

### Production

```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddSvelteHybrid(options =>
    {
        options.EnableSSR = true;
        options.HydrationMode = HydrationMode.Selective;
        options.WatchForChanges = false;
        options.VerboseLogging = false;
        options.EnableCaching = true;
        options.CacheDurationMinutes = 60;
        options.V8PoolSize = 8;
    });
}
```

### Staging

```csharp
if (builder.Environment.IsStaging())
{
    builder.Services.AddSvelteHybrid(options =>
    {
        options.EnableSSR = true;
        options.HydrationMode = HydrationMode.Selective;
        options.WatchForChanges = false;
        options.VerboseLogging = true; // Verbose for testing
        options.EnableCaching = true;
        options.CacheDurationMinutes = 30;
        options.V8PoolSize = 4;
    });
}
```

---

## Configuration File Support

### appsettings.json

```json
{
  "Svelte": {
    "EnableSSR": true,
    "HydrationMode": "Selective",
    "WatchForChanges": false,
    "OutputPath": "wwwroot/_svelte",
    "ComponentDir": "SvelteApp/src",
    "V8PoolSize": 4,
    "VerboseLogging": false,
    "EnableCaching": true,
    "CacheDurationMinutes": 60
  }
}
```

### Load from Configuration

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    builder.Configuration.GetSection("Svelte").Bind(options);
});
```

### Environment Overrides

**appsettings.Development.json:**
```json
{
  "Svelte": {
    "WatchForChanges": true,
    "VerboseLogging": true,
    "EnableCaching": false
  }
}
```

**appsettings.Production.json:**
```json
{
  "Svelte": {
    "V8PoolSize": 8,
    "CacheDurationMinutes": 120
  }
}
```

---

## Logging Configuration

### Configure Logging Levels

```csharp
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Set HRCE-specific log level
builder.Logging.AddFilter("HRCE", LogLevel.Information);
```

### appsettings.json Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "HRCE.Rendering": "Debug",
      "HRCE.Authorization": "Warning",
      "HRCE.Caching": "Information"
    }
  }
}
```

---

## Caching Configuration

### In-Memory Cache (Default)

```csharp
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // MB
});
```

### Distributed Cache (Redis)

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "HRCE:";
});

builder.Services.AddSvelteHybrid(options =>
{
    options.EnableCaching = true;
    // HRCE will automatically use distributed cache if registered
});
```

---

## Static Files Configuration

### Serve Svelte Output

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/_svelte")
    ),
    RequestPath = "/_svelte"
});
```

### Cache Headers for Production

```csharp
if (app.Environment.IsProduction())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache for 1 year
            ctx.Context.Response.Headers.Append(
                "Cache-Control", 
                "public,max-age=31536000"
            );
        }
    });
}
```

---

## Conventions Configuration

### Component Naming Convention

By default, HRCE looks for components with `_` prefix:
- `_ProductCard.cshtml`
- `_ProductCard.svelte`

**Custom Convention:**

```csharp
// Future feature - custom component resolver
builder.Services.Configure<SvelteOptions>(options =>
{
    options.ComponentResolver = (name) =>
    {
        // Custom logic to resolve component paths
        return Path.Combine(options.ComponentDir, $"{name}.component.svelte");
    };
});
```

---

## Security Configuration

### Component Authorization

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAdminComponents", policy =>
        policy.RequireRole("Admin", "Manager"));
        
    options.AddPolicy("CanViewPremiumContent", policy =>
        policy.RequireClaim("Subscription", "Premium"));
});
```

---

## Performance Configuration

### Optimize for High Traffic

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.EnableCaching = true;
    options.CacheDurationMinutes = 120;
    options.V8PoolSize = Environment.ProcessorCount * 2;
});

// Add response caching
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

### Optimize for Development Speed

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.WatchForChanges = true;
    options.VerboseLogging = true;
    options.EnableCaching = false; // Always fresh
    options.V8PoolSize = 2; // Lower memory usage
});
```

---

## Troubleshooting Configuration

### Enable Maximum Logging

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.VerboseLogging = true;
});

builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddFilter("HRCE", LogLevel.Trace);
```

### Disable Features for Testing

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = false; // Test without SSR
    options.EnableCaching = false; // Test without cache
});
```

---

## Complete Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// HRCE Configuration
builder.Services.AddSvelteHybrid(options =>
{
    // Bind from config file
    builder.Configuration.GetSection("Svelte").Bind(options);
    
    // Environment-specific overrides
    if (builder.Environment.IsDevelopment())
    {
        options.WatchForChanges = true;
        options.VerboseLogging = true;
        options.EnableCaching = false;
    }
    else if (builder.Environment.IsProduction())
    {
        options.V8PoolSize = 8;
        options.CacheDurationMinutes = 120;
    }
});

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCaching();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## Next Steps

- [Learn about Directives](directives-and-tags.md)
- [Explore API Interfaces](interfaces.md)
- [Performance Tuning](../advanced/performance-and-caching.md)
