# HRCE Architecture

## Overview

HRCE (Hybrid Razor Component Engine) is a layered architecture that sits between ASP.NET MVC and the client browser, enabling seamless integration of server-rendered components from multiple rendering engines.

---

## Architectural Layers

```
┌───────────────────────────────────────────────────────┐
│                    Client Browser                     │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐  │
│  │    HTML     │  │     CSS     │  │ JavaScript   │  │
│  │  (Merged)   │  │  (Scoped)   │  │ (Sandboxed)  │  │
│  └─────────────┘  └─────────────┘  └──────────────┘  │
└───────────────────────────────────────────────────────┘
                         ↑
                         │ HTTP Response
                         │
┌───────────────────────────────────────────────────────┐
│              Isolation Layer (HRCE)                   │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │ CSS Hashing  │  │ JS Sandbox   │  │ AuthZ      │  │
│  └──────────────┘  └──────────────┘  └────────────┘  │
└───────────────────────────────────────────────────────┘
                         ↑
                         │ Merged HTML
                         │
┌───────────────────────────────────────────────────────┐
│           Hybrid View Engine (HRCE Core)              │
│  ┌─────────────────────────────────────────────────┐  │
│  │  1. Razor Processing                            │  │
│  │  2. Directive Detection (@svelte)               │  │
│  │  3. Component Resolution                        │  │
│  │  4. Parallel Rendering (Razor + Svelte)         │  │
│  │  5. HTML Merging                                │  │
│  └─────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────┘
                         ↑
                         │ View Request
                         │
┌───────────────────────────────────────────────────────┐
│              ASP.NET MVC Pipeline                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐    │
│  │Controller│→ │Presenter │→ │ View w/ Model    │    │
│  └──────────┘  └──────────┘  └──────────────────┘    │
└───────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. Hybrid View Engine

**Purpose:** Orchestrate the rendering pipeline and merge output from multiple renderers.

**Responsibilities:**
- Detect HRCE directives in Razor output
- Route components to appropriate renderers
- Merge HTML from multiple sources
- Apply isolation policies
- Cache rendered components

**Implementation:**
```csharp
public interface IHybridViewEngine
{
    Task<string> RenderAsync(
        string viewName, 
        object model, 
        ViewContext context
    );
}
```

---

### 2. Svelte SSR Renderer

**Purpose:** Execute Svelte components on the server and produce HTML.

**Responsibilities:**
- Load Svelte component code
- Execute in V8 engine
- Pass model data to component
- Extract rendered HTML and CSS
- Handle errors gracefully

**Implementation:**
```csharp
public interface ISvelteRenderer
{
    Task<string> RenderAsync(
        string componentName, 
        object? props = null
    );
}
```

---

### 3. Isolation Layer

**Purpose:** Ensure complete separation between components.

**Components:**

#### CSS Isolation
- Generates unique hash for each component
- Rewrites CSS selectors with hash suffix
- Prevents style bleeding

#### JavaScript Sandbox
- Wraps component JS in isolated scope
- Restricts access to global objects
- Prevents DOM manipulation outside component

#### Authorization Guard
- Checks user permissions before rendering
- Can hide, show, or replace components
- Integrates with ASP.NET Identity

---

## Request Lifecycle

### Phase 1: MVC Request Handling

```
1. Browser → HTTP Request
2. Routing → Controller
3. Controller → Service Layer (get domain data)
4. Controller → Presenter (transform to presentation model)
5. Controller → return View(model)
```

### Phase 2: View Processing

```
6. MVC → Razor View Engine
7. Razor processes .cshtml file
8. Razor outputs HTML with @svelte directives
```

### Phase 3: Hybrid Processing

```
9. HybridViewEngine intercepts Razor output
10. Parse HTML for @svelte directives
11. Extract component names and models
12. For each component:
    a. Check authorization
    b. Load component definition
    c. Call SvelteRenderer.RenderAsync()
    d. Get isolated HTML + CSS
13. Replace @svelte directive with rendered HTML
14. Merge all outputs
```

### Phase 4: Isolation Application

```
15. Apply CSS scoping
16. Apply JS sandboxing
17. Add hydration scripts (if enabled)
18. Add runtime guards
```

### Phase 5: Response

```
19. Return final HTML to client
20. Browser renders HTML
21. Browser applies scoped CSS
22. (Optional) Hydrate interactive components
```

---

## Data Flow

```
Domain Model (Product)
        ↓
    Presenter
        ↓
Presentation Model (ProductCardModel)
        ↓
    Controller
        ↓
      View
        ↓
   Razor Engine → HTML with @svelte directive
        ↓
HybridViewEngine
        ↓
  SvelteRenderer → Svelte SSR HTML
        ↓
   Merge HTML
        ↓
Isolation Layer → Scoped CSS + Sandboxed JS
        ↓
   Final HTML
        ↓
    Browser
```

---

## Component Resolution

HRCE uses convention-based resolution:

### File Structure Convention
```
/Views/Shared/Components/_ProductCard.cshtml  ← Razor template
/SvelteApp/src/_ProductCard.svelte           ← Svelte template
```

### Resolution Algorithm

1. **Parse directive:** `@svelte "_ProductCard"`
2. **Look for Svelte component:** `{componentDir}/_ProductCard.svelte`
3. **If found:** Use Svelte SSR
4. **If not found:** Fall back to Razor markup below directive
5. **If error:** Log error, use fallback

---

## Rendering Strategies

### Strategy 1: Razor Only (Default)
```cshtml
@model ProductCardModel

<div class="product-card">
    <h3>@Model.Title</h3>
    <p>@Model.Description</p>
</div>
```

**When to use:**
- No interactivity needed
- Simple display components
- Maximum compatibility

---

### Strategy 2: Svelte SSR Only
```cshtml
@model ProductCardModel
@svelte "_ProductCard"

<!-- Fallback content (optional) -->
<noscript>
    <div>@Model.Title</div>
</noscript>
```

**When to use:**
- Modern components
- Need client-side interactivity
- Progressive enhancement

---

### Strategy 3: Hybrid (Razor + Svelte)
```cshtml
@model ProductCardModel
@svelte "_ProductCard"

<!-- This Razor markup is used as fallback -->
<div class="product-card">
    <h3>@Model.Title</h3>
    <p>@Model.Description</p>
</div>
```

**When to use:**
- Graceful degradation
- A/B testing renderers
- Migration scenarios

---

## Caching Strategy

HRCE implements multi-level caching:

### Level 1: Component Template Cache
- Caches compiled Svelte components
- Invalidates on file change (dev mode)
- Permanent in production

### Level 2: Rendered Output Cache
- Caches rendered HTML per model
- Key: `{componentName}_{modelHash}`
- Configurable TTL

### Level 3: HTTP Response Cache
- Standard ASP.NET response caching
- Works at page level

---

## Error Handling

### Rendering Errors

When a Svelte component fails to render:

1. Log detailed error (component name, props, stack trace)
2. Fall back to Razor markup (if provided)
3. If no fallback, render error placeholder:
   ```html
   <div class="component-error" data-component="_ProductCard">
       Component failed to render
   </div>
   ```
4. In development, show detailed error
5. In production, show generic message

### Authorization Failures

When authorization check fails:

1. Log security event
2. Render placeholder or empty div
3. Optionally redirect to access denied page

---

## Performance Characteristics

### Server-Side Rendering
- **First Render:** 10-50ms per component
- **Cached Render:** < 1ms per component
- **Parallel Rendering:** Multiple components render simultaneously

### Memory Usage
- **V8 Pool:** ~50MB per instance
- **Template Cache:** ~1MB per 100 components
- **Output Cache:** Configurable (default 100MB)

### Scalability
- Horizontal: Add more app instances
- Vertical: Increase V8 pool size
- Caching: Reduces load by 90%+

---

## Security Model

### Component Isolation

Each component is isolated:
- **CSS:** Cannot affect other components
- **JS:** Cannot access global scope
- **DOM:** Cannot manipulate outside root element
- **Network:** Cannot make unauthorized requests (future)

### Authorization

Components can declare authorization requirements:

```csharp
[ComponentAuthorize(Roles = "Admin")]
public class AdminDashboardModel { }
```

HRCE enforces these at render time.

---

## Extensibility Points

### Custom Renderers

Implement `IComponentRenderer`:

```csharp
public interface IComponentRenderer
{
    Task<string> RenderAsync(string componentName, object? model);
    bool CanRender(string componentName);
}
```

Register in DI:

```csharp
services.AddSingleton<IComponentRenderer, ReactRenderer>();
```

### Custom Isolation Policies

Implement `IIsolationPolicy`:

```csharp
public interface IIsolationPolicy
{
    string ApplyToHtml(string html, string componentName);
    string ApplyToCss(string css, string componentName);
    string ApplyToJs(string js, string componentName);
}
```

---

## Integration Points

### With ASP.NET MVC
- Uses standard MVC pipeline
- No modifications to Controllers or Models
- Works with all MVC features (routing, filters, etc.)

### With Dependency Injection
- All HRCE services registered in DI
- Components can inject services (via Presenter)
- Standard service lifetime management

### With Authentication/Authorization
- Integrates with ASP.NET Identity
- Supports policy-based authorization
- Component-level and page-level auth

---

## Next Steps

- [Understand the Request Lifecycle](request-lifecycle.md)
- [Learn about the Security Model](security-model.md)
- [Explore UML Diagrams](uml-overview.md)
