# FAQ - For MVC Developers

Common questions from ASP.NET MVC developers about adopting HRCE.

---

## Do I need to change my existing Controllers?

**No.** Your controllers remain unchanged:

```csharp
// This code doesn't change
public class ProductsController : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }
}
```

**What's new:** You may want to add Presenters to transform data:

```csharp
public class ProductsController : Controller
{
    private readonly ProductCardPresenter _presenter;
    
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        var viewModel = _presenter.PresentMany(products); // New
        return View(viewModel);
    }
}
```

---

## Do I need to change my existing Views?

**No.** Existing Razor views work as-is:

```cshtml
@model IEnumerable<Product>

@foreach (var product in Model)
{
    <div class="product">
        <h3>@product.Name</h3>
        <p>@product.Price.ToString("C")</p>
    </div>
}
```

**What's new:** You can gradually convert to components:

```cshtml
@model IEnumerable<ProductCardModel>

@foreach (var product in Model)
{
    @await Html.PartialAsync("_ProductCard", product)
}
```

---

## What is a Presenter and why do I need it?

A **Presenter** transforms domain models into presentation models.

**Without Presenter (bad):**
```cshtml
@model Product

<div>
    Price: @Model.Price.ToString("C2")
    Created: @Model.CreatedAt.ToString("MMM dd, yyyy")
    Status: @(Model.Stock > 0 ? "In Stock" : "Out of Stock")
</div>
```

**With Presenter (good):**
```csharp
// Presenter
public class ProductCardPresenter
{
    public ProductCardModel Present(Product p) => new()
    {
        PriceDisplay = p.Price.ToString("C2"),
        CreatedDisplay = p.CreatedAt.ToString("MMM dd, yyyy"),
        StatusDisplay = p.Stock > 0 ? "In Stock" : "Out of Stock"
    };
}
```

```cshtml
@model ProductCardModel

<div>
    Price: @Model.PriceDisplay
    Created: @Model.CreatedDisplay
    Status: @Model.StatusDisplay
</div>
```

**Benefits:**
- ✅ View has zero logic
- ✅ Easy to test (unit test the Presenter)
- ✅ Reusable transformation
- ✅ Clear separation of concerns

---

## Do I need to learn Svelte/React/Vue?

**No, not necessarily.**

You can use HRCE with **Razor-only** components:

```cshtml
@model ProductCardModel

<div class="product-card">
    <h3>@Model.Title</h3>
    <p>@Model.Description</p>
</div>
```

**Add Svelte later** when you need interactivity:
- Let frontend developers handle Svelte
- You continue working with C# and Razor
- Clear separation of responsibilities

---

## How do I handle forms with HRCE?

**Standard MVC forms work unchanged:**

```cshtml
@model ProductFormModel

<form asp-action="Create" method="post">
    <input asp-for="Name" class="form-control" />
    <input asp-for="Price" class="form-control" />
    <button type="submit">Create</button>
</form>
```

**For interactive forms with validation:**
1. Use Razor for initial render (SSR)
2. Add Svelte component for client-side validation
3. Form still posts back to MVC controller

```cshtml
@model ProductFormModel
@svelte "_ProductForm"

<!-- Fallback for no-JS users -->
<form asp-action="Create" method="post">
    ...
</form>
```

---

## What happens to model validation?

**Server-side validation remains unchanged:**

```csharp
[HttpPost]
public async Task<IActionResult> Create(ProductFormModel model)
{
    if (!ModelState.IsValid)
        return View(model); // Standard MVC
    
    // Process...
}
```

**Client-side validation:**
- Use standard ASP.NET validation attributes
- Or add custom validation in Svelte component
- Both can coexist

---

## How do I pass data to components?

**Same as partial views:**

```cshtml
@* Single item *@
@await Html.PartialAsync("_ProductCard", productModel)

@* In a loop *@
@foreach (var item in Model.Products)
{
    @await Html.PartialAsync("_ProductCard", item)
}

@* With ViewData *@
@await Html.PartialAsync("_ProductCard", productModel, new ViewDataDictionary
{
    { "ShowActions", true }
})
```

---

## Can I still use Tag Helpers?

**Yes!** Tag Helpers work normally:

```cshtml
<a asp-controller="Products" asp-action="Details" asp-route-id="@product.Id">
    View Details
</a>
```

**Inside components:**
```cshtml
@model ProductCardModel

<div class="product-card">
    <h3>@Model.Title</h3>
    <a asp-controller="Products" asp-action="Details" asp-route-id="@Model.Id">
        Details
    </a>
</div>
```

---

## What about Layout pages and sections?

**Work exactly the same:**

```cshtml
@* _Layout.cshtml *@
<!DOCTYPE html>
<html>
<head>
    @RenderSection("Styles", required: false)
</head>
<body>
    @RenderBody()
    @RenderSection("Scripts", required: false)
</body>
</html>
```

**HRCE adds component styles automatically** - no need to manually include them.

---

## How do I debug when something goes wrong?

**Same MVC debugging:**
1. Set breakpoints in Controllers
2. Inspect Models in debugger
3. Check ModelState for validation errors

**HRCE-specific:**
1. Check logs for component errors
2. Verify Presenter output
3. Ensure component files exist
4. Check for typos in component names

**Common issues:**
```csharp
// Issue: Component not found
@svelte "_ProductCard"  // Component file: _ProductCard.svelte

// Issue: Model mismatch
@model ProductCardModel  // Ensure Presenter returns this type

// Issue: Null model
@await Html.PartialAsync("_ProductCard", null) // Will fail
```

---

## Does HRCE affect performance?

**Server-side:**
- Minimal overhead vs. pure Razor
- Svelte SSR: +10-30ms per component (first render)
- With caching: <1ms per component
- Net effect: Comparable to Razor-only

**Client-side:**
- Faster than traditional MVC (less JavaScript)
- Optional hydration (load JS only when needed)
- Scoped CSS (smaller file sizes)

**Database:**
- No change - same queries as before
- Presenters are in-memory transformations

---

## Can I use dependency injection in components?

**No, components don't have DI directly.**

**Instead, use DI in Presenters:**

```csharp
public class ProductCardPresenter
{
    private readonly IImageService _imageService;
    
    public ProductCardPresenter(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    public ProductCardModel Present(Product p) => new()
    {
        ImageUrl = _imageService.GetThumbnailUrl(p.ImageUrl),
        // ...
    };
}
```

**Register in DI:**
```csharp
services.AddScoped<ProductCardPresenter>();
```

**Inject in Controller:**
```csharp
public class ProductsController : Controller
{
    private readonly ProductCardPresenter _presenter;
    
    public ProductsController(ProductCardPresenter presenter)
    {
        _presenter = presenter;
    }
}
```

---

## What about Areas?

**Areas work normally:**

```
/Areas
  /Admin
    /Controllers
    /Views
      /Shared
        /Components
          /_AdminCard.cshtml
          /_AdminCard.svelte
```

**Usage:**
```cshtml
@await Html.PartialAsync("Components/_AdminCard", model)
```

---

## Can I use HRCE with API controllers?

**HRCE is for server-rendered views only.**

**For APIs:**
- Use standard API controllers
- Return JSON, not HTML
- HRCE doesn't intercept API responses

**Hybrid scenario:**
```csharp
// MVC Controller - uses HRCE
public class ProductsController : Controller
{
    public IActionResult Index() => View(...);
}

// API Controller - no HRCE
[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    public IActionResult Get() => Ok(...);
}
```

---

## What about ViewComponents?

**ViewComponents work alongside HRCE:**

```csharp
public class NavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View(model);
    }
}
```

**HRCE components vs. ViewComponents:**

| Feature | HRCE Component | ViewComponent |
|---------|----------------|---------------|
| Invocation | `Html.PartialAsync()` | `Component.InvokeAsync()` |
| Logic | Presenter (separate) | Inside ViewComponent |
| Isolation | Automatic | Manual |
| SSR | Svelte/React/etc. | Razor only |

---

## How do I migrate existing partial views?

**Step 1:** Identify the partial view
```cshtml
@* Old: _ProductItem.cshtml *@
@model Product

<div class="product">
    <h3>@Model.Name</h3>
    <p>@Model.Price</p>
</div>
```

**Step 2:** Create Presentation Model
```csharp
public record ProductCardModel(string Title, string PriceDisplay);
```

**Step 3:** Create Presenter
```csharp
public class ProductCardPresenter
{
    public ProductCardModel Present(Product p) => 
        new(p.Name, p.Price.ToString("C"));
}
```

**Step 4:** Update partial view
```cshtml
@* New: _ProductCard.cshtml *@
@model ProductCardModel

<div class="product">
    <h3>@Model.Title</h3>
    <p>@Model.PriceDisplay</p>
</div>
```

**Step 5:** Update usages
```cshtml
@* Old *@
@await Html.PartialAsync("_ProductItem", product)

@* New *@
@await Html.PartialAsync("_ProductCard", presenter.Present(product))
```

---

## Do I need to change my data access layer?

**No.** Your repositories, DbContext, and data access remain unchanged.

**HRCE only affects the presentation layer.**

---

## Can I still use Razor Pages?

**Yes!** HRCE works with Razor Pages:

```cshtml
@page
@model IndexModel

@foreach (var product in Model.Products)
{
    @await Html.PartialAsync("_ProductCard", Presenter.Present(product))
}
```

---

## What's the upgrade path?

1. **Install HRCE** - existing app works unchanged
2. **Add Presenters** - for new features
3. **Create new components** - using USCP pattern
4. **Gradually migrate** - old views to new components
5. **Add Svelte** - when interactivity is needed
6. **No deadline** - migrate at your own pace

---

## Is this just another framework I need to learn?

**No.** HRCE is a pattern, not a framework.

**You already know:**
- ✅ ASP.NET MVC
- ✅ Razor syntax
- ✅ Partial views

**What's new:**
- Presenters (simple transformation classes)
- `@svelte` directive (optional)
- USCP pattern (design pattern)

**Learning time:** A few hours to understand, days to master.

---

## Next Steps

- [Read Getting Started Guide](../getting-started/introduction.md)
- [Create Your First Component](../getting-started/first-component.md)
- [Learn the USCP Pattern](../concepts/uscp-pattern.md)
- [See FAQ for Frontend Developers](faq-frontend-devs.md)
