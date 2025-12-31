# Installation Guide

This guide will walk you through installing and configuring HRCE in your ASP.NET MVC application.

## Prerequisites

Before installing HRCE, ensure you have:

- ✅ ASP.NET Core 6.0 or later
- ✅ .NET SDK installed
- ✅ Node.js 16+ and npm installed
- ✅ An existing ASP.NET MVC project (or create a new one)

## Step 1: Create or Prepare Your MVC Project

If you don't have an existing MVC project:

```bash
dotnet new mvc -n MyHybridApp
cd MyHybridApp
```

## Step 2: Install NuGet Packages

Add the required NuGet packages to your project:

```bash
# Add Razor Runtime Compilation (for development)
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation

# Add any additional dependencies
dotnet restore
```

## Step 3: Install NPM Dependencies

Initialize npm and install Svelte dependencies:

```bash
# Initialize package.json if not exists
npm init -y

# Install Svelte and build tools
npm install --save svelte
npm install --save-dev vite @sveltejs/vite-plugin-svelte

# Install development dependencies
npm install --save-dev rollup @rollup/plugin-node-resolve
```

## Step 4: Add HRCE Core Files

### 4.1 Create Core Directory Structure

```bash
mkdir -p Core
mkdir -p Infrastructure/Extensions
mkdir -p Presentation/Models
mkdir -p Presentation/Presenters
mkdir -p SvelteApp/src
```

### 4.2 Create SvelteOptions.cs

Create `Core/SvelteOptions.cs`:

```csharp
namespace YourApp.Core;

public class SvelteOptions
{
    public bool EnableSSR { get; set; } = true;
    public HydrationMode HydrationMode { get; set; } = HydrationMode.Selective;
    public bool WatchForChanges { get; set; } = false;
    public string OutputPath { get; set; } = "wwwroot/_svelte";
    public int V8PoolSize { get; set; } = 4;
}

public enum HydrationMode
{
    Full,
    Selective,
    None
}

public interface ISvelteRenderer
{
    Task<string> RenderAsync(string componentName, object? props = null);
}

public class SvelteRenderer : ISvelteRenderer
{
    private readonly SvelteOptions _options;

    public SvelteRenderer(SvelteOptions options)
    {
        _options = options;
    }

    public async Task<string> RenderAsync(string componentName, object? props = null)
    {
        // SSR implementation placeholder
        // In production, this would use V8 engine to execute Svelte SSR
        await Task.CompletedTask;
        return $"<div data-svelte-component=\"{componentName}\"></div>";
    }
}
```

### 4.3 Create Service Extensions

Create `Infrastructure/Extensions/SvelteServiceExtensions.cs`:

```csharp
using YourApp.Core;

namespace YourApp.Infrastructure.Extensions;

public static class SvelteServiceExtensions
{
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
}
```

## Step 5: Configure Program.cs

Update your `Program.cs` to enable HRCE:

```csharp
using YourApp.Core;
using YourApp.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure MVC with Razor Runtime Compilation
var mvcBuilder = builder.Services.AddControllersWithViews();

#if DEBUG
mvcBuilder.AddRazorRuntimeCompilation();
#endif

// 2. Add Svelte Hybrid services
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
    options.WatchForChanges = builder.Environment.IsDevelopment();
});

var app = builder.Build();

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
app.UseRouting();
app.UseAuthorization();

// 3. Optional: Svelte SSR endpoint for debugging
app.MapGet("/_svelte/ssr/{component}", async (string component, ISvelteRenderer renderer) =>
{
    var result = await renderer.RenderAsync(component);
    return Results.Content(result, "text/html");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

## Step 6: Create Svelte Configuration

Create `sveltekit.config.json`:

```json
{
  "ssr": true,
  "outputDir": "wwwroot/_svelte",
  "componentDir": "SvelteApp/src"
}
```

Create `vite.config.js` (optional, for development):

```javascript
import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';

export default defineConfig({
  plugins: [svelte({
    compilerOptions: {
      hydratable: true,
      generate: 'ssr'
    }
  })],
  build: {
    outDir: 'wwwroot/_svelte',
    emptyOutDir: true,
    ssr: true
  }
});
```

## Step 7: Update .gitignore

Add HRCE-specific ignores:

```gitignore
# Node modules
node_modules/

# Svelte build output
wwwroot/_svelte/

# NPM debug logs
npm-debug.log*
```

## Step 8: Verify Installation

Run the application:

```bash
dotnet run
```

You should see output similar to:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

Navigate to `http://localhost:5000` - your MVC application should run normally.

## Step 9: Test HRCE Endpoint (Optional)

Visit `http://localhost:5000/_svelte/ssr/TestComponent` to verify the Svelte renderer is registered.

## Configuration Options

### SvelteOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableSSR` | bool | true | Enable server-side rendering |
| `HydrationMode` | enum | Selective | Full, Selective, or None |
| `WatchForChanges` | bool | false | Watch for file changes (dev mode) |
| `OutputPath` | string | "wwwroot/_svelte" | Build output directory |
| `V8PoolSize` | int | 4 | Number of V8 instances for SSR |

### Example Configurations

**Production:**
```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
    options.WatchForChanges = false;
    options.V8PoolSize = 8; // Higher for production
});
```

**Development:**
```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Full;
    options.WatchForChanges = true;
    options.V8PoolSize = 2; // Lower for dev
});
```

**Client-Only (No SSR):**
```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = false;
    options.HydrationMode = HydrationMode.Full;
});
```

## Troubleshooting

### Issue: "ISvelteRenderer not found"

**Solution:** Ensure you've called `AddSvelteHybrid()` in `Program.cs` before `builder.Build()`.

### Issue: Node modules not found

**Solution:** Run `npm install` in the project root directory.

### Issue: Razor Runtime Compilation not working

**Solution:** Ensure you've added the NuGet package and called `AddRazorRuntimeCompilation()`.

## Next Steps

Now that HRCE is installed, you're ready to:

1. [Create your first component](first-component.md)
2. Learn about [the USCP pattern](../concepts/uscp-pattern.md)
3. Explore [component examples](../examples/product-card-example.md)

## Additional Resources

- [HRCE Architecture](../concepts/hrce-architecture.md)
- [Configuration Reference](../reference/configuration.md)
- [FAQ for MVC Developers](../faq/faq-mvc-devs.md)
