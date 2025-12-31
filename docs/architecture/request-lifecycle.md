# Request Lifecycle

This document provides a detailed walkthrough of a complete HTTP request through the HRCE system.

---

## Overview

Every request flows through these stages:

1. **HTTP Request** → Browser sends request
2. **Routing** → ASP.NET routes to controller
3. **Controller** → Processes request, builds model
4. **Presenter** → Transforms domain to presentation
5. **View** → Razor processes template
6. **HRCE** → Detects and renders components
7. **Isolation** → Applies security and scoping
8. **HTTP Response** → Returns HTML to browser
9. **Hydration** → (Optional) Client-side activation

---

## Detailed Flow

### 1. HTTP Request

```
Browser:
GET /products/123 HTTP/1.1
Host: localhost:5000
Accept: text/html
```

**What happens:**
- Browser sends GET request for product details
- Request includes cookies (authentication)
- ASP.NET receives request

---

### 2. Routing & Middleware Pipeline

```csharp
// ASP.NET Pipeline
Request
  → UseRouting()
  → UseAuthentication()
  → UseAuthorization()
  → MapControllerRoute()
```

**What happens:**
- Routing matches: `{controller=Products}/{action=Details}/{id=123}`
- Authentication middleware validates user session
- Authorization middleware checks permissions
- Route to `ProductsController.Details(123)`

---

### 3. Controller Processing

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ProductCardPresenter _presenter;

    public async Task<IActionResult> Details(int id)
    {
        // 3a. Fetch domain data
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();

        // 3b. Transform to presentation model
        var viewModel = new ProductDetailsViewModel
        {
            MainCard = _presenter.Present(product),
            RelatedCards = _presenter.PresentMany(
                await _productService.GetRelatedAsync(product.Category)
            )
        };

        // 3c. Return view with model
        return View(viewModel);
    }
}
```

**What happens:**
- Controller calls service layer
- Service retrieves `Product` from database
- Presenter transforms `Product` → `ProductCardModel`
- Controller returns view name and model

---

### 4. Razor View Engine - Initial Processing

**File:** `Views/Products/Details.cshtml`

```cshtml
@model ProductDetailsViewModel

<div class="product-details">
    <h1>Product Details</h1>
    
    <!-- Main product card -->
    @await Html.PartialAsync("Components/_ProductCard", Model.MainCard)
    
    <!-- Related products -->
    <div class="related-products">
        @foreach (var card in Model.RelatedCards)
        {
            @await Html.PartialAsync("Components/_ProductCard", card)
        }
    </div>
</div>
```

**What happens:**
- Razor engine locates `Details.cshtml`
- Processes model binding
- Encounters `Html.PartialAsync()`
- Loads partial view `Components/_ProductCard.cshtml`

---

### 5. Component Template Processing

**File:** `Views/Shared/Components/_ProductCard.cshtml`

```cshtml
@model ProductCardModel
@svelte "_ProductCard"

<div class="product-card">
    <h3>@Model.Title</h3>
    <p>@Model.Description</p>
    <span>@Model.PriceDisplay</span>
</div>
```

**What happens:**
- Razor processes `@model` directive → Type checking
- Razor sees `@svelte "_ProductCard"` → Passes to HRCE
- Razor processes HTML markup → Generates fallback HTML

**Razor Output:**
```html
@svelte "_ProductCard"
<div class="product-card">
    <h3>Wireless Headphones</h3>
    <p>High-quality wireless headphones</p>
    <span>$79.99</span>
</div>
```

---

### 6. HRCE - Hybrid Processing

#### 6.1 Directive Detection

```csharp
// HybridViewEngine pseudo-code
var html = await razorEngine.RenderAsync(viewName, model);
var directives = ParseDirectives(html); // Finds @svelte directives

foreach (var directive in directives)
{
    var componentName = directive.ComponentName; // "_ProductCard"
    var componentModel = directive.Model;        // ProductCardModel instance
    
    // ... continue to rendering
}
```

#### 6.2 Component Resolution

```csharp
// Look for Svelte component
var componentPath = Path.Combine(
    _options.ComponentDir, 
    $"{componentName}.svelte"
);

if (File.Exists(componentPath))
{
    // Use Svelte renderer
    var svelteHtml = await _svelteRenderer.RenderAsync(
        componentName, 
        componentModel
    );
}
else
{
    // Use Razor fallback (HTML after @svelte directive)
    var svelteHtml = ExtractFallbackHtml(html, directive);
}
```

#### 6.3 Authorization Check

```csharp
// Check if user can see this component
var authResult = await _authService.AuthorizeComponentAsync(
    componentName, 
    User
);

if (!authResult.Succeeded)
{
    svelteHtml = "<div class=\"component-unauthorized\"></div>";
}
```

---

### 7. Svelte SSR Rendering

**File:** `SvelteApp/src/_ProductCard.svelte`

```svelte
<script>
  export let model;
</script>

<div class="product-card-svelte">
  <img src={model.imageUrl} alt={model.title} />
  <h3>{model.title}</h3>
  <p>{model.description}</p>
  <span class="price">{model.priceDisplay}</span>
  <button on:click={() => addToCart(model)}>
    Add to Cart
  </button>
</div>

<style>
  .product-card-svelte {
    border: 1px solid #ccc;
    padding: 16px;
  }
  .price {
    font-weight: bold;
    color: #2563eb;
  }
</style>
```

**SSR Execution:**

```javascript
// Inside V8 engine
const { render } = require('./_ProductCard.svelte');

const result = render({
  model: {
    title: "Wireless Headphones",
    description: "High-quality wireless headphones",
    imageUrl: "/images/headphones.jpg",
    priceDisplay: "$79.99"
  }
});

// result.html contains:
// <div class="product-card-svelte">...</div>

// result.css contains:
// .product-card-svelte { border: 1px solid #ccc; ... }
```

**Output:**
```html
<div class="product-card-svelte">
  <img src="/images/headphones.jpg" alt="Wireless Headphones" />
  <h3>Wireless Headphones</h3>
  <p>High-quality wireless headphones</p>
  <span class="price">$79.99</span>
  <button>Add to Cart</button>
</div>
```

---

### 8. Isolation Layer

#### 8.1 CSS Scoping

```csharp
var hash = GenerateHash(componentName); // e.g., "h7x9k2"

var scopedCss = css
    .Replace(".product-card-svelte", $".product-card-svelte-{hash}")
    .Replace(".price", $".price-{hash}");

var scopedHtml = html
    .Replace("class=\"product-card-svelte\"", 
             $"class=\"product-card-svelte-{hash}\"")
    .Replace("class=\"price\"", 
             $"class=\"price-{hash}\"");
```

**Result:**
```html
<div class="product-card-svelte-h7x9k2">
  ...
  <span class="price-h7x9k2">$79.99</span>
  ...
</div>

<style>
  .product-card-svelte-h7x9k2 { border: 1px solid #ccc; }
  .price-h7x9k2 { font-weight: bold; color: #2563eb; }
</style>
```

#### 8.2 JavaScript Sandboxing

```javascript
// Wrap component JS in IIFE with restricted scope
(function(componentScope) {
  'use strict';
  
  // Component code runs here
  // Cannot access window, document directly
  // Must use componentScope.emit() for events
  
})(createComponentScope('_ProductCard_h7x9k2'));
```

---

### 9. HTML Merging

```csharp
// Replace @svelte directive with rendered output
var finalHtml = originalHtml.Replace(
    directive.FullText, // "@svelte \"_ProductCard\""
    scopedHtml          // Rendered and isolated component
);
```

**Before:**
```html
<div class="product-details">
    @svelte "_ProductCard"
    <div class="product-card">...</div>
</div>
```

**After:**
```html
<div class="product-details">
    <div class="product-card-svelte-h7x9k2">
        <img src="/images/headphones.jpg" alt="Wireless Headphones" />
        <h3>Wireless Headphones</h3>
        <p>High-quality wireless headphones</p>
        <span class="price-h7x9k2">$79.99</span>
        <button>Add to Cart</button>
    </div>
</div>

<style>
  .product-card-svelte-h7x9k2 { border: 1px solid #ccc; }
  .price-h7x9k2 { font-weight: bold; color: #2563eb; }
</style>
```

---

### 10. HTTP Response

```
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Content-Length: 4521

<!DOCTYPE html>
<html>
<head>
    <title>Product Details</title>
    <style>
        .product-card-svelte-h7x9k2 { ... }
    </style>
</head>
<body>
    <div class="product-details">
        <div class="product-card-svelte-h7x9k2">...</div>
    </div>
</body>
</html>
```

---

### 11. Browser Rendering

1. Browser receives HTML
2. Parses HTML and builds DOM
3. Applies scoped CSS
4. Renders visible page
5. User sees product card

---

### 12. Hydration (Optional)

If hydration is enabled:

```html
<script type="module">
  import { hydrate } from '/_svelte/runtime.js';
  
  hydrate('_ProductCard_h7x9k2', {
    model: { /* server model */ },
    target: document.querySelector('.product-card-svelte-h7x9k2')
  });
</script>
```

**What happens:**
- Svelte client runtime loads
- Finds server-rendered component
- Attaches event listeners
- Button becomes interactive
- Component is now fully functional

---

## Performance Timeline

| Stage | Time (typical) | Notes |
|-------|----------------|-------|
| 1. HTTP Request | <1ms | Network latency not included |
| 2. Routing | 1-2ms | ASP.NET pipeline |
| 3. Controller | 10-50ms | Database query dominant |
| 4. Presenter | <1ms | Pure transformation |
| 5. Razor Processing | 5-10ms | Template compilation cached |
| 6. HRCE Detection | <1ms | Regex parsing |
| 7. Svelte SSR | 10-30ms | V8 execution |
| 8. Isolation | 1-2ms | String manipulation |
| 9. Merging | <1ms | String replacement |
| **Total** | **30-100ms** | Server-side only |

---

## Caching Impact

With caching enabled:

| Stage | Without Cache | With Cache | Improvement |
|-------|---------------|------------|-------------|
| Controller | 50ms | 50ms | - |
| Svelte SSR | 30ms | <1ms | **97%** |
| Total | 100ms | 70ms | **30%** |

---

## Error Handling Flow

If component rendering fails:

```
Component Error
    ↓
Log Error (with stack trace)
    ↓
Check for Fallback HTML
    ↓
├─ If Fallback Exists → Use Razor markup
└─ If No Fallback → Use Error Placeholder
    ↓
Apply Isolation
    ↓
Continue with Response
```

---

## Summary

The complete request lifecycle ensures:
- ✅ Type-safe data flow from domain to presentation
- ✅ Seamless integration of Razor and Svelte
- ✅ Component isolation and security
- ✅ Graceful error handling
- ✅ Optimal performance with caching

---

## Next Steps

- [Understand HRCE Architecture](hrce-architecture.md)
- [Learn about Security Model](security-model.md)
- [Explore Performance Optimization](../advanced/performance-and-caching.md)
