# Product Card Example

This example demonstrates a complete Product Card component implementation using the HRCE pattern.

---

## Component Overview

**Purpose:** Display product information in a reusable card format

**Features:**
- Product image
- Title and description
- Price display
- Category badge
- Add to cart button
- Hover effects

**Technologies:**
- C# Presentation Model and Presenter
- Razor template (fallback)
- Svelte component (enhanced)

---

## 1. Domain Model

First, we have our domain model (already exists in the application):

```csharp
namespace MyApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 2. Presentation Model

Create a simple, display-ready model:

**File:** `Presentation/Models/ProductCardModel.cs`

```csharp
namespace MyApp.Presentation.Models;

/// <summary>
/// Presentation model for product card component.
/// Immutable and ready for display.
/// </summary>
public record ProductCardModel
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public required string PriceDisplay { get; init; }
    public required string Category { get; init; }
    public required bool InStock { get; init; }
    public required bool IsFeatured { get; init; }
    public string? BadgeText { get; init; }
    public string? BadgeColor { get; init; }
}
```

**Key Points:**
- `record` type ensures immutability
- `required` properties ensure all data is provided
- All display formatting done in Presenter
- No logic or behavior

---

## 3. Presenter

Transform domain data to presentation data:

**File:** `Presentation/Presenters/ProductCardPresenter.cs`

```csharp
using MyApp.Models;
using MyApp.Presentation.Models;

namespace MyApp.Presentation.Presenters;

public class ProductCardPresenter
{
    private const int DescriptionMaxLength = 100;

    public ProductCardModel Present(Product product)
    {
        return new ProductCardModel
        {
            Id = product.Id,
            Title = product.Name,
            Description = TruncateDescription(product.Description),
            ImageUrl = GetImageUrl(product.ImageUrl),
            PriceDisplay = FormatPrice(product.Price),
            Category = product.Category,
            InStock = product.Stock > 0,
            IsFeatured = product.IsFeatured,
            BadgeText = GetBadgeText(product),
            BadgeColor = GetBadgeColor(product)
        };
    }

    public IEnumerable<ProductCardModel> PresentMany(IEnumerable<Product> products)
    {
        return products.Select(Present);
    }

    private static string TruncateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "No description available";

        if (description.Length <= DescriptionMaxLength)
            return description;

        return description.Substring(0, DescriptionMaxLength) + "...";
    }

    private static string GetImageUrl(string? imageUrl)
    {
        return string.IsNullOrWhiteSpace(imageUrl) 
            ? "/images/placeholder.png" 
            : imageUrl;
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("C2"); // $99.99
    }

    private static string? GetBadgeText(Product product)
    {
        if (product.IsFeatured)
            return "Featured";
        
        if (product.Stock == 0)
            return "Out of Stock";
        
        if (product.Stock < 5)
            return "Low Stock";
        
        return null;
    }

    private static string? GetBadgeColor(Product product)
    {
        if (product.IsFeatured)
            return "blue";
        
        if (product.Stock == 0)
            return "red";
        
        if (product.Stock < 5)
            return "orange";
        
        return null;
    }
}
```

**Key Points:**
- All formatting logic in one place
- Handles null/missing data gracefully
- Easy to unit test
- No business logic (no database calls, no validation)

---

## 4. Razor Template

**File:** `Views/Shared/Components/_ProductCard.cshtml`

```cshtml
@model MyApp.Presentation.Models.ProductCardModel

@* HRCE directive - use Svelte if available *@
@* @svelte "_ProductCard" *@

<div class="product-card" data-product-id="@Model.Id">
    <div class="product-card-image">
        <img src="@Model.ImageUrl" alt="@Model.Title" />
        @if (Model.BadgeText != null)
        {
            <span class="product-badge badge-@Model.BadgeColor">@Model.BadgeText</span>
        }
    </div>
    
    <div class="product-card-content">
        <div class="product-card-header">
            <h3 class="product-title">@Model.Title</h3>
            <span class="product-category">@Model.Category</span>
        </div>
        
        <p class="product-description">@Model.Description</p>
        
        <div class="product-card-footer">
            <span class="product-price">@Model.PriceDisplay</span>
            
            @if (Model.InStock)
            {
                <button class="btn-add-cart" type="button">
                    Add to Cart
                </button>
            }
            else
            {
                <button class="btn-add-cart" disabled>
                    Out of Stock
                </button>
            }
        </div>
    </div>
</div>

<style>
    .product-card {
        display: flex;
        flex-direction: column;
        background: white;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        transition: transform 0.2s, box-shadow 0.2s;
    }

    .product-card:hover {
        transform: translateY(-4px);
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
    }

    .product-card-image {
        position: relative;
        height: 200px;
        overflow: hidden;
    }

    .product-card-image img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .product-badge {
        position: absolute;
        top: 12px;
        right: 12px;
        padding: 4px 12px;
        border-radius: 4px;
        font-size: 0.75rem;
        font-weight: 600;
        color: white;
    }

    .badge-blue { background: #2563eb; }
    .badge-red { background: #dc2626; }
    .badge-orange { background: #ea580c; }

    .product-card-content {
        padding: 16px;
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .product-card-header {
        margin-bottom: 12px;
    }

    .product-title {
        margin: 0 0 4px 0;
        font-size: 1.25rem;
        font-weight: 600;
        color: #1f2937;
    }

    .product-category {
        display: inline-block;
        padding: 2px 8px;
        background: #f3f4f6;
        border-radius: 4px;
        font-size: 0.75rem;
        color: #6b7280;
    }

    .product-description {
        margin: 0 0 16px 0;
        color: #6b7280;
        font-size: 0.875rem;
        line-height: 1.5;
        flex: 1;
    }

    .product-card-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding-top: 16px;
        border-top: 1px solid #e5e7eb;
    }

    .product-price {
        font-size: 1.5rem;
        font-weight: 700;
        color: #2563eb;
    }

    .btn-add-cart {
        padding: 8px 20px;
        background: #2563eb;
        color: white;
        border: none;
        border-radius: 6px;
        font-weight: 500;
        cursor: pointer;
        transition: background 0.2s;
    }

    .btn-add-cart:hover:not(:disabled) {
        background: #1d4ed8;
    }

    .btn-add-cart:disabled {
        background: #9ca3af;
        cursor: not-allowed;
    }
</style>
```

---

## 5. Svelte Component (Optional)

**File:** `SvelteApp/src/_ProductCard.svelte`

```svelte
<script>
  export let model;
  
  let quantity = 1;
  let isAdding = false;
  let isAdded = false;
  
  async function addToCart() {
    if (isAdding || !model.inStock) return;
    
    isAdding = true;
    
    try {
      const response = await fetch('/api/cart/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          productId: model.id,
          quantity: quantity
        })
      });
      
      if (response.ok) {
        isAdded = true;
        setTimeout(() => {
          isAdded = false;
        }, 2000);
      }
    } catch (error) {
      console.error('Failed to add to cart:', error);
    } finally {
      isAdding = false;
    }
  }
  
  function viewDetails() {
    window.location.href = `/products/${model.id}`;
  }
</script>

<div class="product-card" data-product-id={model.id}>
  <div class="product-card-image" on:click={viewDetails}>
    <img src={model.imageUrl} alt={model.title} />
    {#if model.badgeText}
      <span class="product-badge badge-{model.badgeColor}">
        {model.badgeText}
      </span>
    {/if}
  </div>
  
  <div class="product-card-content">
    <div class="product-card-header">
      <h3 class="product-title">{model.title}</h3>
      <span class="product-category">{model.category}</span>
    </div>
    
    <p class="product-description">{model.description}</p>
    
    <div class="product-card-footer">
      <span class="product-price">{model.priceDisplay}</span>
      
      {#if model.inStock}
        <div class="add-to-cart-actions">
          <input 
            type="number" 
            bind:value={quantity} 
            min="1" 
            max="10"
            class="quantity-input"
          />
          <button 
            class="btn-add-cart" 
            on:click={addToCart}
            disabled={isAdding}
            class:added={isAdded}
          >
            {#if isAdding}
              Adding...
            {:else if isAdded}
              ✓ Added!
            {:else}
              Add to Cart
            {/if}
          </button>
        </div>
      {:else}
        <button class="btn-add-cart" disabled>
          Out of Stock
        </button>
      {/if}
    </div>
  </div>
</div>

<style>
  /* Same styles as Razor version, plus: */
  
  .product-card-image {
    cursor: pointer;
  }
  
  .add-to-cart-actions {
    display: flex;
    gap: 8px;
    align-items: center;
  }
  
  .quantity-input {
    width: 60px;
    padding: 6px;
    border: 1px solid #e5e7eb;
    border-radius: 4px;
    text-align: center;
  }
  
  .btn-add-cart.added {
    background: #10b981;
  }
  
  /* ... (rest of styles from Razor version) ... */
</style>
```

---

## 6. Controller Usage

**File:** `Controllers/ProductsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using MyApp.Presentation.Presenters;

namespace MyApp.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ProductCardPresenter _cardPresenter;

    public ProductsController(
        IProductService productService,
        ProductCardPresenter cardPresenter)
    {
        _productService = productService;
        _cardPresenter = cardPresenter;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        var viewModel = _cardPresenter.PresentMany(products);
        
        return View(viewModel);
    }

    public async Task<IActionResult> Category(string category)
    {
        var products = await _productService.GetByCategoryAsync(category);
        var viewModel = _cardPresenter.PresentMany(products);
        
        return View("Index", viewModel);
    }
}
```

---

## 7. View Usage

**File:** `Views/Products/Index.cshtml`

```cshtml
@model IEnumerable<MyApp.Presentation.Models.ProductCardModel>

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
    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 24px;
    }
    
    .product-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
        gap: 24px;
        margin-top: 24px;
    }
    
    @media (max-width: 768px) {
        .product-grid {
            grid-template-columns: 1fr;
        }
    }
</style>
```

---

## 8. Dependency Injection Setup

**File:** `Program.cs`

```csharp
// Register the presenter
builder.Services.AddScoped<ProductCardPresenter>();
```

---

## Complete File Structure

```
MyApp/
├── Controllers/
│   └── ProductsController.cs
├── Models/
│   └── Product.cs
├── Presentation/
│   ├── Models/
│   │   └── ProductCardModel.cs
│   └── Presenters/
│       └── ProductCardPresenter.cs
├── Views/
│   ├── Products/
│   │   └── Index.cshtml
│   └── Shared/
│       └── Components/
│           └── _ProductCard.cshtml
└── SvelteApp/
    └── src/
        └── _ProductCard.svelte
```

---

## Testing the Component

### Unit Test for Presenter

```csharp
public class ProductCardPresenterTests
{
    [Fact]
    public void Present_TransformsProductCorrectly()
    {
        // Arrange
        var presenter = new ProductCardPresenter();
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 99.99m,
            Description = "Test description",
            Category = "Electronics",
            Stock = 10
        };

        // Act
        var result = presenter.Present(product);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Title);
        Assert.Equal("$99.99", result.PriceDisplay);
        Assert.True(result.InStock);
    }

    [Fact]
    public void Present_TruncatesLongDescription()
    {
        // Arrange
        var presenter = new ProductCardPresenter();
        var longDescription = new string('a', 150);
        var product = new Product
        {
            Description = longDescription
        };

        // Act
        var result = presenter.Present(product);

        // Assert
        Assert.True(result.Description.Length <= 103); // 100 + "..."
        Assert.EndsWith("...", result.Description);
    }
}
```

---

## Summary

This complete example demonstrates:

✅ **USCP Pattern**: Model → Presenter → View → Component  
✅ **Type Safety**: Strong typing throughout  
✅ **Separation of Concerns**: Clear responsibilities  
✅ **Reusability**: Component works anywhere  
✅ **Progressive Enhancement**: Razor fallback + Svelte enhancement  
✅ **Isolation**: Scoped styles and sandboxed JS  
✅ **Testability**: Easy to unit test all parts  

---

## Next Steps

- [Create More Examples](list-and-grid-example.md)
- [Learn About Testing](../guides/create-component.md)
- [Understand Architecture](../concepts/hrce-architecture.md)
