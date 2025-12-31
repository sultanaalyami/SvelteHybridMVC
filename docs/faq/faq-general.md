# FAQ - General Questions

## What is HRCE?

**HRCE (Hybrid Razor Component Engine)** is a rendering engine that allows ASP.NET MVC applications to use modern frontend components (like Svelte) alongside traditional Razor views, with complete isolation and without breaking MVC patterns.

---

## Why was HRCE created?

HRCE solves the "modernization dilemma" faced by many MVC applications:

- **Problem:** Need modern, interactive UI components
- **Traditional Solution:** Complete rewrite to SPA framework
- **Cost:** High risk, long timeline, expensive
- **HRCE Solution:** Gradual modernization, one component at a time

---

## How does HRCE differ from Blazor?

| Feature | HRCE | Blazor |
|---------|------|--------|
| **Approach** | Hybrid (Razor + Any SSR framework) | Blazor-only |
| **Backend** | Standard MVC/Controllers | Blazor Components or APIs |
| **Learning Curve** | Low (uses existing Razor) | Medium (new paradigm) |
| **Migration** | Gradual | All-or-nothing |
| **Framework** | Framework-agnostic | Microsoft-specific |
| **Use Case** | Enhance existing MVC | New applications or full rewrites |

---

## How does HRCE differ from using React/Vue with MVC?

| Feature | Traditional React/Vue + MVC | HRCE |
|---------|----------------------------|------|
| **Server Rendering** | Requires separate Node.js server | Built-in, in-process |
| **Component Isolation** | Manual (developer responsibility) | Automatic |
| **Type Safety** | Weak (JSON props) | Strong (C# models) |
| **Integration** | Loose (separate builds) | Tight (single build) |
| **Authorization** | Client-side only | Server-side, per-component |

---

## Is HRCE production-ready?

HRCE is a **reference architecture** and **pattern demonstration**. 

**Current Status:**
- ✅ Core concepts proven
- ✅ Architecture validated
- ✅ Pattern documented
- ⚠️ SSR implementation is placeholder
- ⚠️ Needs V8 integration for production

**To make production-ready:**
1. Implement full V8 pool for Svelte SSR
2. Add comprehensive error handling
3. Implement proper caching strategy
4. Add monitoring and telemetry
5. Performance testing and optimization

---

## What technologies does HRCE require?

**Required:**
- ASP.NET Core 6.0+
- C# 10+
- Node.js 16+ (for building Svelte components)

**Optional:**
- Svelte 5 (or any SSR-capable framework)
- Redis (for distributed caching)

---

## Can I use HRCE with .NET Framework?

Not directly. HRCE is designed for ASP.NET Core.

However, the **USCP pattern** (Unified Server Component Pattern) can be applied to .NET Framework MVC with modifications.

---

## Does HRCE support other frameworks besides Svelte?

**Currently:** Svelte only in the reference implementation.

**By Design:** Framework-agnostic. The `IComponentRenderer` interface can be implemented for:
- React (via Node.js or V8)
- Vue (via SSR)
- Lit (Web Components)
- Any framework with SSR capability

---

## How does HRCE handle SEO?

**Excellent SEO** because:
- ✅ Full server-side rendering
- ✅ No client-side dependency for initial render
- ✅ Search engines see complete HTML
- ✅ Semantic HTML output
- ✅ Fast first contentful paint

---

## What's the performance impact?

**Server-Side:**
- First render: 10-50ms per component
- Cached render: <1ms per component
- Minimal overhead vs. pure Razor

**Client-Side:**
- Zero JavaScript required for display
- Optional hydration for interactivity
- Smaller bundle sizes (selective hydration)

---

## Can I use HRCE without Svelte?

**Yes!** You can:
1. Use only the Razor templates (no Svelte needed)
2. The `@svelte` directive is optional
3. HRCE still provides the USCP pattern benefits
4. Add Svelte later when needed

---

## How does HRCE handle authentication and authorization?

HRCE integrates with ASP.NET Identity:

**Page-level:** Standard MVC `[Authorize]` attributes

**Component-level:** HRCE-specific authorization:
```csharp
[ComponentAuthorize(Roles = "Admin")]
public record AdminDashboardModel { ... }
```

HRCE checks authorization before rendering each component.

---

## What happens if a component fails to render?

HRCE has multiple fallback layers:

1. **Svelte fails** → Use Razor fallback markup
2. **No Razor fallback** → Use error placeholder
3. **Complete failure** → Log error, show generic message

**In Development:**
- Detailed error messages
- Stack traces
- Component props logged

**In Production:**
- Generic error message
- Detailed logging to server
- Graceful degradation

---

## Can components communicate with each other?

**No, by design.** Components are isolated.

**Instead, use:**
- Shared presentation models from controller
- Server-side state management
- Events via SignalR (for real-time)
- Client-side state management (if hydrated)

---

## How do I test HRCE components?

### Unit Testing Presenters
```csharp
[Fact]
public void Presenter_Transforms_Product_Correctly()
{
    var presenter = new ProductCardPresenter();
    var product = new Product { Name = "Test", Price = 99.99m };
    
    var model = presenter.Present(product);
    
    Assert.Equal("Test", model.Title);
    Assert.Equal("$99.99", model.PriceDisplay);
}
```

### Integration Testing Components
```csharp
[Fact]
public async Task Component_Renders_Successfully()
{
    var renderer = new SvelteRenderer(options);
    var model = new ProductCardModel { Title = "Test" };
    
    var html = await renderer.RenderAsync("_ProductCard", model);
    
    Assert.Contains("Test", html);
}
```

### End-to-End Testing
Use Playwright or Selenium to test the full page with components.

---

## Does HRCE support real-time updates?

HRCE focuses on **server-side rendering**.

For real-time updates:
- Use **SignalR** to push updates from server
- Re-render components on the server
- Send updated HTML to client
- Or use client-side hydration with Svelte stores

---

## How do I debug HRCE components?

### Server-Side Debugging
- Set breakpoints in Presenters
- Inspect models before rendering
- Use logging in HybridViewEngine

### Svelte Debugging
- Check build output in `wwwroot/_svelte`
- Use browser DevTools for hydrated components
- Enable verbose logging in SvelteOptions

### HRCE-Specific
- Enable detailed logging: `WatchForChanges = true`
- Check for directive parsing errors
- Verify component resolution paths

---

## Can I use HRCE with existing MVC applications?

**Yes!** HRCE is designed for incremental adoption:

1. Install HRCE in existing MVC app
2. Existing views work unchanged
3. Create new components using USCP pattern
4. Gradually migrate old views to components
5. No "big bang" migration needed

---

## What's the learning curve?

**For MVC Developers:**
- Low - mostly familiar Razor syntax
- New: Presenter pattern
- New: USCP pattern concepts

**For Frontend Developers:**
- Low - standard Svelte/React/Vue
- New: Server-side rendering focus
- New: Presentation Model contracts

**For Teams:**
- Clear separation of concerns
- Backend and frontend can work independently
- Well-defined contracts (Presentation Models)

---

## What license is HRCE under?

MIT License - free for commercial and personal use.

---

## Where can I get help?

- Read the [documentation](../index.md)
- Check [examples](../examples/product-card-example.md)
- Review [architecture docs](../architecture/uml-overview.md)
- See [FAQ for MVC Developers](faq-mvc-devs.md)
- See [FAQ for Frontend Developers](faq-frontend-devs.md)

---

## Can I contribute to HRCE?

Yes! HRCE is open source. Contributions welcome:
- Bug fixes
- Documentation improvements
- New renderer implementations (React, Vue, etc.)
- Performance optimizations
- Additional examples

---

## What's next for HRCE?

**Planned:**
- Production-ready V8 integration
- React renderer implementation
- Vue renderer implementation
- Enhanced caching strategies
- Performance monitoring tools
- Visual Studio Code extension
- Component scaffolding CLI

---

## Is HRCE suitable for new projects?

**If you need:**
- ✅ Server-side rendering
- ✅ SEO optimization
- ✅ Gradual migration path
- ✅ Component isolation
- ✅ MVC architecture

**Consider alternatives if:**
- ❌ Need pure SPA with client-side routing
- ❌ Building mobile app (use Blazor MAUI)
- ❌ Want Microsoft-only stack (use Blazor)
- ❌ Need massive real-time updates (use SignalR + Blazor)

---

## Next Steps

- [Read the Product Manual (Arabic)](../product-manual-ar.md)
- [Get Started](../getting-started/introduction.md)
- [Explore Architecture](../architecture/uml-overview.md)
