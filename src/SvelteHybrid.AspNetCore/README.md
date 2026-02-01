# SvelteHybrid.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/SvelteHybrid.AspNetCore.svg)](https://www.nuget.org/packages/SvelteHybrid.AspNetCore/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

?? **Professional library for integrating Svelte components with ASP.NET Core MVC and Razor Pages**

## Features

- ? **Server-Side Rendering (SSR)** - Render Svelte components on the server for SEO and performance
- ? **Component Isolation** - CSS and JavaScript isolation to prevent conflicts
- ? **Authorization Support** - Policy-based component rendering
- ? **Seamless Hydration** - Interactive components after initial render
- ? **Hot Reload** - Development-time hot reloading support
- ? **Multiple Renderers** - Support for Svelte, React, Vue, or Razor-only

## Installation

```bash
dotnet add package SvelteHybrid.AspNetCore
```

## Quick Start

### 1. Register Services

```csharp
// Program.cs
using SvelteHybrid.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add SvelteHybrid services
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSvelteSSR = true;
    options.IsolationMode = IsolationMode.Full;
    options.ComponentBasePath = "~/Components";
});

var app = builder.Build();

// Use SvelteHybrid middleware
app.UseSvelteHybrid();

app.Run();
```

### 2. Create a Svelte Component

Create a component in `Components/ProductCard/ProductCard.svelte`:

```svelte
<script>
  export let product;
</script>

<div class="product-card">
  <h3>{product.name}</h3>
  <p class="price">${product.price}</p>
  <button on:click={() => alert('Added!')}>Add to Cart</button>
</div>

<style>
  .product-card {
    border: 1px solid #ddd;
    padding: 1rem;
    border-radius: 8px;
  }
  .price {
    color: #2563eb;
    font-weight: bold;
  }
</style>
```

### 3. Use in Razor View

```html
<!-- Views/Products/Index.cshtml -->
@model ProductViewModel

<h1>Our Products</h1>

@foreach (var product in Model.Products)
{
    @svelte "ProductCard" @data="@product"
}
```

Or with authorization:

```html
@svelte "AdminPanel" @require-policy "AdminOnly"
```

## Configuration Options

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    // Enable/disable SSR
    options.EnableSvelteSSR = true;
    
    // Component isolation mode
    options.IsolationMode = IsolationMode.Full; // Full, Partial, None
    
    // Base path for components
    options.ComponentBasePath = "~/Components";
    
    // Default renderer type
    options.DefaultRenderer = RendererType.Svelte;
    
    // Enable hot reload in development
    options.EnableHotReload = true;
    
    // Authorization settings
    options.Authorization.DefaultPolicy = "AuthenticatedUser";
    options.Authorization.FallbackComponent = "Unauthorized";
});
```

## Advanced Usage

### Custom Component Data

```csharp
// In your controller
public IActionResult Products()
{
    var viewModel = new ProductsViewModel
    {
        Products = _productService.GetAll(),
        CurrentUser = User.Identity?.Name
    };
    return View(viewModel);
}
```

### Programmatic Rendering

```csharp
public class MyController : Controller
{
    private readonly IHybridViewEngine _hybridEngine;
    
    public MyController(IHybridViewEngine hybridEngine)
    {
        _hybridEngine = hybridEngine;
    }
    
    public async Task<IActionResult> Render()
    {
        var html = await _hybridEngine.RenderHybridAsync(
            "<div>@svelte \"MyComponent\"</div>",
            new { data = "value" }
        );
        return Content(html, "text/html");
    }
}
```

### Direct Svelte Rendering

```csharp
public class ApiController : Controller
{
    private readonly ISvelteRenderer _svelteRenderer;
    
    public ApiController(ISvelteRenderer svelteRenderer)
    {
        _svelteRenderer = svelteRenderer;
    }
    
    public async Task<IActionResult> GetComponent(string name)
    {
        var html = await _svelteRenderer.RenderAsync(name, new { });
        return Content(html, "text/html");
    }
}
```

## Node.js SSR Server (Optional)

For full SSR support, set up the Node.js server:

```bash
cd node-ssr
npm install
npm start
```

The package includes a fallback simulation when Node.js is not available.

## Isolation Modes

| Mode | CSS Isolation | JS Isolation | Performance |
|------|--------------|--------------|-------------|
| **Full** | ? Scoped classes | ? Scoped selectors | Good |
| **Partial** | ? Scoped classes | ? Global | Better |
| **None** | ? Global | ? Global | Best |

## Requirements

- .NET 8.0, 9.0, or 10.0
- ASP.NET Core MVC or Razor Pages
- Node.js 18+ (optional, for full SSR)

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md).

## Support

- ?? [Documentation](https://github.com/sultanaalyami/SvelteHybridMVC/wiki)
- ?? [Issues](https://github.com/sultanaalyami/SvelteHybridMVC/issues)
- ?? [Discussions](https://github.com/sultanaalyami/SvelteHybridMVC/discussions)
