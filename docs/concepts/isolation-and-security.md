# Component Isolation and Security

This document explains how HRCE enforces complete isolation between components and ensures security.

---

## The Isolation Problem

Traditional web applications face several isolation challenges:

### 1. CSS Pollution
```html
<!-- Component A -->
<style>
  .title { color: red; }
</style>

<!-- Component B -->
<style>
  .title { color: blue; } /* Conflicts with Component A! */
</style>
```

### 2. JavaScript Global Scope
```javascript
// Component A
var count = 0;

// Component B
var count = 0; // Name collision!
```

### 3. DOM Manipulation
```javascript
// Component A can accidentally affect Component B
document.querySelector('.button').addEventListener('click', ...);
```

---

## How HRCE Solves Isolation

HRCE enforces **complete isolation** through multiple layers:

```
┌─────────────────────────────────────────┐
│        Component Instance               │
│  ┌───────────────────────────────────┐  │
│  │  CSS Isolation (Scoped Hashing)   │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  JS Isolation (Sandboxing)        │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  DOM Isolation (Root Element)     │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │  Authorization (Access Control)   │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

---

## CSS Isolation

### How It Works

HRCE automatically scopes all CSS to prevent conflicts:

**Original Component CSS:**
```css
.product-card {
  border: 1px solid #ccc;
  padding: 16px;
}

.title {
  font-size: 1.5rem;
  color: #333;
}
```

**HRCE Transforms To:**
```css
.product-card-a7b3f9 {
  border: 1px solid #ccc;
  padding: 16px;
}

.title-a7b3f9 {
  font-size: 1.5rem;
  color: #333;
}
```

**Corresponding HTML:**
```html
<div class="product-card-a7b3f9">
  <h3 class="title-a7b3f9">Product Name</h3>
</div>
```

### Hash Generation

The hash is generated from:
- Component name
- Component version
- Content hash (optional)

```csharp
private string GenerateHash(string componentName)
{
    var input = $"{componentName}_{Version}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(hash)[..6]; // First 6 chars
}
```

### Benefits

✅ **No CSS Conflicts**
- Each component's styles are unique
- Styles can't leak to other components
- Global CSS still works for layout

✅ **Predictable Styling**
- Components look the same everywhere
- No unexpected style overrides
- Easy to debug CSS issues

---

## JavaScript Isolation

### How It Works

HRCE wraps component JavaScript in a sandbox:

**Original Component Code:**
```javascript
let count = 0;

function increment() {
  count++;
  updateUI();
}
```

**HRCE Transforms To:**
```javascript
(function(componentId, componentScope) {
  'use strict';
  
  // Component code runs in isolated scope
  let count = 0;
  
  function increment() {
    count++;
    componentScope.emit('update', count);
  }
  
  // Restricted API
  const window = undefined;
  const document = componentScope.document;
  
})('_ProductCard_a7b3f9', createComponentScope('_ProductCard_a7b3f9'));
```

### Restrictions

Component JavaScript **cannot**:
- ❌ Access global `window` object
- ❌ Access global `document` object
- ❌ Modify DOM outside component root
- ❌ Define global variables
- ❌ Pollute global scope

Component JavaScript **can**:
- ✅ Access component-scoped `document` (limited to component root)
- ✅ Use component-scoped state
- ✅ Emit events via `componentScope.emit()`
- ✅ Use standard JavaScript features
- ✅ Import npm packages

### Example: Restricted Access

```javascript
// ❌ This will throw an error
window.myGlobal = 'value';

// ❌ This will throw an error  
document.querySelector('body').style.background = 'red';

// ✅ This works - scoped to component
const button = document.querySelector('.add-to-cart');
button.addEventListener('click', handleClick);

// ✅ This works - emit event to parent
componentScope.emit('productAdded', productId);
```

---

## DOM Isolation

### Root Element Boundary

Each component has a **root element** that defines its boundary:

```html
<div data-component="_ProductCard" data-component-id="a7b3f9">
  <!-- Component owns everything inside this div -->
  <h3>Product Title</h3>
  <button>Add to Cart</button>
</div>
<!-- Component cannot access anything outside -->
```

### Enforced Boundaries

```javascript
// Component's document is limited to root element
const componentDocument = {
  querySelector: (selector) => {
    const root = document.querySelector(`[data-component-id="${componentId}"]`);
    return root.querySelector(selector); // Scoped to root
  },
  
  querySelectorAll: (selector) => {
    const root = document.querySelector(`[data-component-id="${componentId}"]`);
    return root.querySelectorAll(selector); // Scoped to root
  }
  
  // Other DOM methods similarly scoped
};
```

### Protection Against DOM Manipulation

```javascript
// ❌ Cannot manipulate outside component
document.querySelector('.other-component').remove(); // Error!

// ❌ Cannot add global event listeners
window.addEventListener('click', handler); // Error!

// ✅ Can manipulate own DOM
document.querySelector('.product-title').textContent = 'New Title';

// ✅ Can listen to own events
document.querySelector('button').addEventListener('click', handler);
```

---

## Authorization Isolation

### Component-Level Authorization

HRCE can enforce authorization at the component level:

```csharp
[ComponentAuthorize(Roles = "Admin")]
public record AdminDashboardModel : IPresentationModel
{
    // Only users with Admin role can see this
}
```

### How It Works

```csharp
public async Task<string> RenderComponentAsync(
    string componentName, 
    object model,
    ClaimsPrincipal user)
{
    // 1. Check authorization
    var authResult = await _authService.AuthorizeComponentAsync(
        componentName, 
        model, 
        user
    );
    
    if (!authResult.Succeeded)
    {
        // 2. Return placeholder or hide component
        return RenderUnauthorizedPlaceholder(componentName);
    }
    
    // 3. Render normally
    return await RenderAsync(componentName, model);
}
```

### Authorization Strategies

#### Strategy 1: Hide Component
```csharp
if (!authorized)
{
    return string.Empty; // Render nothing
}
```

#### Strategy 2: Show Placeholder
```csharp
if (!authorized)
{
    return @"<div class='component-unauthorized'>
        You do not have permission to view this content.
    </div>";
}
```

#### Strategy 3: Redirect
```csharp
if (!authorized)
{
    return @"<div class='component-unauthorized'>
        <a href='/login'>Please log in to continue</a>
    </div>";
}
```

### Policy-Based Authorization

```csharp
// Define policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewPremiumContent", policy =>
    {
        policy.RequireClaim("Subscription", "Premium", "Enterprise");
        policy.RequireAuthenticatedUser();
    });
});

// Apply to component
[ComponentAuthorize(Policy = "CanViewPremiumContent")]
public record PremiumContentModel : IPresentationModel
{
    // Only premium subscribers can see this
}
```

---

## Runtime Security

### Content Security Policy (CSP)

HRCE works with CSP headers:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline';"
    );
    await next();
});
```

### XSS Protection

HRCE automatically escapes model data:

```csharp
// Model with potentially dangerous content
var model = new ProductCardModel
{
    Title = "<script>alert('XSS')</script>"
};

// HRCE escapes during rendering
// Output: &lt;script&gt;alert('XSS')&lt;/script&gt;
```

### CSRF Protection

Use standard ASP.NET anti-forgery tokens:

```cshtml
<form method="post">
    @Html.AntiForgeryToken()
    
    @await Html.PartialAsync("_ProductForm", model)
</form>
```

---

## Isolation Best Practices

### ✅ DO:

1. **Keep components small and focused**
   ```csharp
   // Good: Single responsibility
   public record ProductCardModel { ... }
   
   // Bad: Too many responsibilities
   public record ProductPageModel { ... } // Too big!
   ```

2. **Use scoped styles**
   ```css
   /* Good: Component-specific */
   .product-card { ... }
   
   /* Bad: Too generic */
   .button { ... }
   ```

3. **Emit events for communication**
   ```javascript
   // Good: Emit event
   componentScope.emit('productSelected', productId);
   
   // Bad: Direct access
   window.selectedProduct = productId;
   ```

4. **Test in isolation**
   ```csharp
   [Fact]
   public void Component_Renders_Independently()
   {
       var model = new ProductCardModel { ... };
       var html = await RenderAsync("_ProductCard", model);
       Assert.NotEmpty(html);
   }
   ```

### ❌ DON'T:

1. **Don't rely on global state**
2. **Don't use overly generic CSS selectors**
3. **Don't access DOM outside component**
4. **Don't assume component order or presence**

---

## Monitoring Isolation Violations

### Enable Isolation Logging

```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.LogIsolationViolations = true;
});
```

### Violation Examples

```
[Warning] Component '_ProductCard' attempted to access global window object
[Warning] Component '_UserProfile' attempted to query selector outside root
[Error] Component '_AdminPanel' unauthorized access attempt by user 'guest'
```

---

## Testing Isolation

### Unit Test: CSS Isolation

```csharp
[Fact]
public void CSS_Is_Scoped_To_Component()
{
    var css = ".product-card { color: red; }";
    var scoped = _isolationEngine.ScopeCss(css, "_ProductCard");
    
    Assert.Contains("product-card-", scoped);
    Assert.DoesNotContain(".product-card {", scoped);
}
```

### Unit Test: JS Isolation

```csharp
[Fact]
public void JavaScript_Cannot_Access_Window()
{
    var js = "window.alert('test');";
    
    Assert.Throws<IsolationViolationException>(() =>
    {
        _isolationEngine.ExecuteInSandbox(js, "_ProductCard");
    });
}
```

### Integration Test: Authorization

```csharp
[Fact]
public async Task Unauthorized_User_Cannot_See_Component()
{
    var user = CreateUnauthorizedUser();
    var model = new AdminDashboardModel();
    
    var html = await _engine.RenderAsync("_AdminDashboard", model, user);
    
    Assert.Empty(html); // Component hidden
}
```

---

## Summary

HRCE provides **complete isolation** through:

1. **CSS Scoping** - Unique hashes prevent style conflicts
2. **JS Sandboxing** - Restricted API prevents global pollution
3. **DOM Boundaries** - Components can't affect each other's DOM
4. **Authorization** - Fine-grained access control per component

This ensures:
- ✅ Components are truly reusable
- ✅ No unexpected side effects
- ✅ Security by default
- ✅ Easier debugging and testing

---

## Next Steps

- [Learn about Security Model](../architecture/security-model.md)
- [Explore Component Examples](../examples/product-card-example.md)
- [Read Best Practices](../guides/create-component.md)
