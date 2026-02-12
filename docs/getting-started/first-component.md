# Your First Component

This tutorial will guide you through creating your first HRCE component from scratch - a simple Product Card.

## What You'll Build

A reusable product card component that displays:
- Product image
- Product name
- Product price
- Product description

The component will use Razor for server-side structure and Svelte for interactive elements.

---

## Step 1: Create the Presentation Model

First, create a simple, immutable data structure for the component.

Create `Presentation/Models/ProductCardModel.cs`:

```csharp
namespace YourApp.Presentation.Models;

/// <summary>
/// Presentation model for a product card component.
/// Immutable record type ensures data integrity.
/// </summary>
public record ProductCardModel
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public required string PriceDisplay { get; init; }
    public string? Category { get; init; }
}
```

**Key Points:**
- ✅ Use `record` for immutability
- ✅ Simple properties only (no logic)
- ✅ Display-ready data (e.g., `PriceDisplay` not `decimal Price`)

---

## Step 2: Create the Presenter

The Presenter transforms domain models into presentation models.

Create `Presentation/Presenters/ProductCardPresenter.cs`:

```csharp
using YourApp.Models;
using YourApp.Presentation.Models;

namespace YourApp.Presentation.Presenters;

/// <summary>
/// Transforms Product domain model to ProductCardModel presentation model.
/// </summary>
public class ProductCardPresenter
{
    public ProductCardModel Present(Product product)
    {
        return new ProductCardModel
        {
            Title = product.Name,
            Description = product.Description ?? "No description available",
            ImageUrl = product.ImageUrl ?? "/images/placeholder.png",
            PriceDisplay = FormatPrice(product.Price),
            Category = product.Category
        };
    }

    public IEnumerable<ProductCardModel> PresentMany(IEnumerable<Product> products)
    {
        return products.Select(Present);
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("C2"); // $99.99 format
    }
}
```

**Key Points:**
- ✅ Single responsibility: transform data only
- ✅ No business logic (that stays in services/domain)
- ✅ Handles null/missing data gracefully
- ✅ Formats data for display

---

## Step 3: Create the Razor Template

Create `Views/Shared/Components/_ProductCard.cshtml`:

```cshtml
@model YourApp.Presentation.Models.ProductCardModel

@*
    HRCE Directive - tells the engine to look for a Svelte component
    If the Svelte component exists, it will be used for SSR
    Otherwise, this Razor markup will be used as fallback
*@
@* @svelte "_ProductCard" *@

<div class="product-card">
    <div class="product-card-image">
        <img src="@Model.ImageUrl" alt="@Model.Title" />
    </div>
    <div class="product-card-content">
        <h3 class="product-card-title">@Model.Title</h3>
        @if (!string.IsNullOrEmpty(Model.Category))
        {
            <span class="product-card-category">@Model.Category</span>
        }
        <p class="product-card-description">@Model.Description</p>
        <div class="product-card-footer">
            <span class="product-card-price">@Model.PriceDisplay</span>
            <button class="product-card-button">Add to Cart</button>
        </div>
    </div>
</div>

<style>
    .product-card {
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        overflow: hidden;
        transition: box-shadow 0.3s ease;
    }

    .product-card:hover {
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .product-card-image img {
        width: 100%;
        height: 200px;
        object-fit: cover;
    }

    .product-card-content {
        padding: 16px;
    }

    .product-card-title {
        margin: 0 0 8px 0;
        font-size: 1.25rem;
        font-weight: 600;
    }

    .product-card-category {
        display: inline-block;
        padding: 4px 8px;
        background: #f0f0f0;
        border-radius: 4px;
        font-size: 0.75rem;
        margin-bottom: 8px;
    }

    .product-card-description {
        color: #666;
        margin: 8px 0;
        font-size: 0.875rem;
    }

    .product-card-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 16px;
    }

    .product-card-price {
        font-size: 1.5rem;
        font-weight: bold;
        color: #2563eb;
    }

    .product-card-button {
        padding: 8px 16px;
        background: #2563eb;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-weight: 500;
    }

    .product-card-button:hover {
        background: #1d4ed8;
    }
</style>
```

**Key Points:**
- ✅ The `@svelte` directive is commented out initially
- ✅ Strong typing with `@model`
- ✅ Scoped styles (will be automatically isolated by HRCE)
- ✅ Works as a standard Razor partial view

---

## Step 4: Create the Svelte Template (Optional)

Create `SvelteApp/src/_ProductCard.svelte`:

```svelte
<script>
  // Model is passed from the server
  export let model;

  // Local state for interactivity
  let quantity = 1;
  let isAdded = false;

  function addToCart() {
    // Client-side interactivity
    isAdded = true;
    setTimeout(() => {
      isAdded = false;
    }, 2000);

    // In production, this would call an API
    console.log(`Added ${quantity}x ${model.title} to cart`);
  }
</script>

<div class="product-card">
  <div class="product-card-image">
    <img src={model.imageUrl} alt={model.title} />
  </div>
  <div class="product-card-content">
    <h3 class="product-card-title">{model.title}</h3>
    {#if model.category}
      <span class="product-card-category">{model.category}</span>
    {/if}
    <p class="product-card-description">{model.description}</p>
    <div class="product-card-footer">
      <span class="product-card-price">{model.priceDisplay}</span>
      <div class="product-card-actions">
        <input 
          type="number" 
          bind:value={quantity} 
          min="1" 
          max="10"
          class="quantity-input"
        />
        <button 
          class="product-card-button"
          on:click={addToCart}
          class:added={isAdded}
        >
          {isAdded ? '✓ Added' : 'Add to Cart'}
        </button>
      </div>
    </div>
  </div>
</div>

<style>
  .product-card {
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    overflow: hidden;
    transition: box-shadow 0.3s ease;
  }

  .product-card:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .product-card-image img {
    width: 100%;
    height: 200px;
    object-fit: cover;
  }

  .product-card-content {
    padding: 16px;
  }

  .product-card-title {
    margin: 0 0 8px 0;
    font-size: 1.25rem;
    font-weight: 600;
  }

  .product-card-category {
    display: inline-block;
    padding: 4px 8px;
    background: #f0f0f0;
    border-radius: 4px;
    font-size: 0.75rem;
    margin-bottom: 8px;
  }

  .product-card-description {
    color: #666;
    margin: 8px 0;
    font-size: 0.875rem;
  }

  .product-card-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 16px;
  }

  .product-card-actions {
    display: flex;
    gap: 8px;
    align-items: center;
  }

  .quantity-input {
    width: 60px;
    padding: 6px;
    border: 1px solid #e0e0e0;
    border-radius: 4px;
  }

  .product-card-price {
    font-size: 1.5rem;
    font-weight: bold;
    color: #2563eb;
  }

  .product-card-button {
    padding: 8px 16px;
    background: #2563eb;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 500;
    transition: background 0.3s ease;
  }

  .product-card-button:hover {
    background: #1d4ed8;
  }

  .product-card-button.added {
    background: #10b981;
  }
</style>
```

**Key Points:**
- ✅ Receives `model` as a prop from server
- ✅ Can add client-side state and interactivity
- ✅ Same styles as Razor version for consistency
- ✅ Progressive enhancement (works without JS)

---

## Step 5: Use the Component in a View

Update your controller to use the presenter:

```csharp
using Microsoft.AspNetCore.Mvc;
using YourApp.Presentation.Presenters;
using YourApp.Models;

namespace YourApp.Controllers;

public class ProductsController : Controller
{
    private readonly ProductCardPresenter _cardPresenter;

    public ProductsController()
    {
        _cardPresenter = new ProductCardPresenter();
    }

    public IActionResult Index()
    {
        // In a real app, this would come from a service/repository
        var products = GetSampleProducts();
        
        var viewModel = _cardPresenter.PresentMany(products);
        
        return View(viewModel);
    }

    private List<Product> GetSampleProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Wireless Headphones",
                Price = 79.99m,
                Description = "High-quality wireless headphones with noise cancellation",
                Category = "Electronics"
            },
            new Product
            {
                Id = 2,
                Name = "Smart Watch",
                Price = 199.99m,
                Description = "Track your fitness and stay connected",
                Category = "Wearables"
            }
        };
    }
}
```

Create `Views/Products/Index.cshtml`:

```cshtml
@model IEnumerable<YourApp.Presentation.Models.ProductCardModel>

@{
    ViewData["Title"] = "Products";
}

<div class="container">
    <h1>Our Products</h1>
    
    <div class="product-grid">
        @foreach (var product in Model)
        {
            @await Html.PartialAsync("Components/_ProductCard", product)
        }
    </div>
</div>

<style>
    .product-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
        gap: 24px;
        margin-top: 24px;
    }
</style>
```

---

## Step 6: Run and Test

```bash
dotnet run
```

Navigate to `http://localhost:5000/Products`

You should see:
- Grid of product cards
- Proper styling
- Hover effects
- (If Svelte is enabled) Interactive quantity selector

---

## Step 7: Enable Svelte SSR (Optional)

To enable the Svelte version:

1. Uncomment the `@svelte` directive in `_ProductCard.cshtml`:
   ```cshtml
   @svelte "_ProductCard"
   ```

2. Build the Svelte component:
   ```bash
   npm run build
   ```

3. Restart the application

Now HRCE will:
- Render the Svelte component on the server
- Include the component's isolated CSS
- Optionally hydrate for client-side interactivity

---

## Understanding the Flow

```
1. Controller
   └─> ProductCardPresenter.PresentMany(products)
       └─> Creates ProductCardModel[]

2. View (Index.cshtml)
   └─> @foreach loop
       └─> Html.PartialAsync("_ProductCard", model)

3. HRCE Processing
   ├─> If @svelte directive found
   │   └─> SvelteRenderer.RenderAsync("_ProductCard", model)
   │       └─> Server-side Svelte SSR
   └─> Else
       └─> Standard Razor rendering

4. Output
   └─> Isolated HTML + CSS + (optional) JS
```

---

## What You've Learned

✅ How to create a Presentation Model  
✅ How to build a Presenter that transforms data  
✅ How to create a Razor template  
✅ How to create an optional Svelte template  
✅ How to use the component in a view  
✅ How HRCE processes components  

---

## Next Steps

- [Learn about component isolation](../concepts/isolation-and-security.md)
- [Explore the USCP pattern](../concepts/uscp-pattern.md)
- [Add component authorization](../guides/create-component.md#component-authorization)
- [Build more complex components](../examples/dashboard-widgets-example.md)

---

## Troubleshooting

**Issue: Component not rendering**
- Verify the model is being passed correctly
- Check that the partial view path is correct
- Ensure the model types match

**Issue: Styles not applied**
- Check for CSS class name conflicts
- Verify styles are inside the component file
- HRCE will automatically scope them

**Issue: Svelte component not found**
- Ensure the file name matches exactly
- Check the `componentDir` in `sveltekit.config.json`
- Verify the build output directory

For more help, see the [FAQ](../faq/faq-general.md).

---

## Extended Examples

### Example A: Inject a Svelte Component in `_Layout`

Use the `@svelte` directive inside `Views/Shared/_Layout.cshtml` to render a component globally:

```cshtml
@* Views/Shared/_Layout.cshtml *@
@svelte "_HeaderWidget"
```

Create the Razor fallback in `Components/HeaderWidget/_HeaderWidget.cshtml`:

```cshtml
@model HeaderWidgetModel

<div class="header-widget">
    <strong>@Model.Title</strong>
    <span>@Model.Subtitle</span>
</div>
```

Optional Svelte version in `Components/HeaderWidget/_HeaderWidget.svelte`:

```svelte
<script>
  export let model;
</script>

<div class="header-widget">
  <strong>{model.title}</strong>
  <span>{model.subtitle}</span>
</div>
```

### Example B: Component With Authorization

```cshtml
@* Views/Shared/Components/_SecurePanel.cshtml *@
@svelte "_SecurePanel"
@require-policy "AdminOnly"
```

If policy fails, HRCE renders the fallback component configured in `Program.cs`.

### Example C: Use Component in Razor Pages

```cshtml
@* Pages/Index.cshtml *@
@model IndexModel

@await Html.PartialAsync("~/Components/ProductCard/_ProductCard.cshtml", Model.Card)
```

### Example D: Layout Builder Output (Drag & Drop)

Layout blocks are saved as JSON and then rendered in `_Layout.cshtml`. Example payload:

```json
{
  "blocks": [
    { "id": "1", "type": "header", "label": "Main Header" },
    { "id": "2", "type": "section", "label": "Content" },
    { "id": "3", "type": "footer", "label": "Footer" }
  ]
}
```

### Example E: Svelte Hybrid Directives (SvelteHybrid.AspNetCore)

```html
<div s-reactive s-data="{ count: 0 }">
  <button s-click="count++">Clicked {count}</button>
</div>
```

This works without extra JavaScript and upgrades to client-side interactivity automatically.
