# ?? SvelteHybrid.AspNetCore NuGet Package

Êã ÅäÔÇÁ ÍÒãÉ NuGet ÇÍÊÑÇİíÉ áÏãÌ Svelte ãÚ ASP.NET Core!

## ?? ãæŞÚ ÇáÍÒãÉ

```
packages/
??? SvelteHybrid.AspNetCore.1.0.0.nupkg      # ÇáÍÒãÉ ÇáÑÆíÓíÉ
??? SvelteHybrid.AspNetCore.1.0.0.snupkg     # ÑãæÒ ÇáÊÕÍíÍ
```

## ?? ÇáÊËÈíÊ

### ãä ÇáãÌáÏ ÇáãÍáí:
```bash
dotnet add package SvelteHybrid.AspNetCore --source ./packages
```

### äÔÑ Åáì NuGet.org:
```bash
dotnet nuget push packages/SvelteHybrid.AspNetCore.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## ?? ÇáÇÓÊÎÏÇã ÇáÓÑíÚ

### 1. ÅÖÇİÉ ÇáÎÏãÇÊ

```csharp
// Program.cs
using SvelteHybrid.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ÅÖÇİÉ ÎÏãÇÊ SvelteHybrid
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSvelteSSR = true;
    options.IsolationMode = IsolationMode.Full;
    options.ComponentBasePath = "~/Components";
});

var app = builder.Build();

// ÇÓÊÎÏÇã Middleware
app.UseSvelteHybrid();

app.Run();
```

### 2. ÇÓÊÎÏÇã Tag Helper

```html
<!-- Views/_ViewImports.cshtml -->
@addTagHelper *, SvelteHybrid.AspNetCore
```

```html
<!-- Views/Products/Index.cshtml -->
<svelte component="ProductCard" props="@Model.Product" />

<!-- ãÚ ÓíÇÓÉ ÊİæíÖ -->
<svelte component="AdminPanel" require-policy="AdminOnly" />

<!-- ÚÑÖ ãä ÌÇäÈ ÇáÚãíá İŞØ -->
<svelte component="Chart" props="@Model.Data" client-only />
```

### 3. ÇÓÊÎÏÇã HTML Helper

```csharp
@await Html.SvelteAsync("ProductCard", Model.Product)
```

### 4. ÇÓÊÎÏÇã Directive İí Razor

```html
@svelte "ProductCard"
@svelte "AdminPanel" @require-policy "AdminOnly"
```

## ? ÇáãíÒÇÊ

| ÇáãíÒÉ | ÇáæÕİ |
|--------|--------|
| ??? SSR | ÚÑÖ ãä ÌÇäÈ ÇáÎÇÏã ááÃÏÇÁ æSEO |
| ?? ÇáÚÒá | ÚÒá CSS æJavaScript ááãßæäÇÊ |
| ?? ÇáÊİæíÖ | ÏÚã ÓíÇÓÇÊ ÇáÊİæíÖ |
| ?? ÇáÊÑØíÈ | ÊÑØíÈ ÓáÓ ááÊİÇÚá |
| ? ÇáÊÎÒíä ÇáãÄŞÊ | ÊÎÒíä ãÄŞÊ ááÃÏÇÁ |
| ?? Tag Helpers | ÏÚã Tag Helpers |
| ?? Hot Reload | ÅÚÇÏÉ ÊÍãíá ÓÑíÚ İí ÇáÊØæíÑ |

## ?? åíßá ÇáÍÒãÉ

```
src/SvelteHybrid.AspNetCore/
??? Abstractions/              # ÇáæÇÌåÇÊ
?   ??? ISvelteRenderer.cs
?   ??? IHybridViewEngine.cs
?   ??? IIsolationService.cs
?   ??? INodeService.cs
??? Services/                  # ÇáÊäİíĞÇÊ
?   ??? SvelteRenderer.cs
?   ??? HybridViewEngine.cs
?   ??? IsolationService.cs
?   ??? NodeService.cs
??? Middleware/                # Middleware
?   ??? SvelteHybridMiddleware.cs
??? Extensions/                # ÇãÊÏÇÏÇÊ
?   ??? ServiceCollectionExtensions.cs
?   ??? HtmlHelperExtensions.cs
??? TagHelpers/                # Tag Helpers
?   ??? SvelteTagHelper.cs
??? content/                   # ãÍÊæì ÇáÍÒãÉ
?   ??? wwwroot/js/           # JavaScript Runtime
?   ??? node-ssr/             # ÎÇÏã Node.js
?   ??? Components/           # ãßæäÇÊ äãæĞÌíÉ
??? SvelteHybridOptions.cs    # ÎíÇÑÇÊ ÇáÊßæíä
```

## ?? ÇáÃäÙãÉ ÇáãÏÚæãÉ

- ? .NET 8.0
- ? .NET 9.0
- ? .NET 10.0

## ?? ÇáÊÑÎíÕ

MIT License
