# HRCE Documentation

Welcome to the comprehensive documentation for **HRCE (Hybrid Razor Component Engine)** - a hybrid rendering engine that brings modern component development to ASP.NET MVC applications.

---

## 📚 Documentation Structure

### [Getting Started](getting-started/introduction.md)
New to HRCE? Start here!
- [Introduction](getting-started/introduction.md) - What is HRCE and why use it?
- [Installation](getting-started/installation.md) - Step-by-step installation guide
- [Your First Component](getting-started/first-component.md) - Build a component from scratch

### [Core Concepts](concepts/uscp-pattern.md)
Understand the architecture and patterns:
- [USCP Pattern](concepts/uscp-pattern.md) - Unified Server Component Pattern explained
- [HRCE Architecture](concepts/hrce-architecture.md) - How HRCE works internally
- [Isolation and Security](concepts/isolation-and-security.md) - Component isolation mechanisms

### [Architecture](architecture/uml-overview.md)
Deep dive into system design:
- [UML Overview](architecture/uml-overview.md) - Class, sequence, and component diagrams
- [Request Lifecycle](architecture/request-lifecycle.md) - Complete request flow walkthrough
- [Security Model](architecture/security-model.md) - Comprehensive security architecture

### [API Reference](reference/interfaces.md)
Technical reference documentation:
- [Interfaces](reference/interfaces.md) - All HRCE interfaces and types
- [Configuration](reference/configuration.md) - Complete configuration guide

### [Examples](examples/product-card-example.md)
Complete, working examples:
- [Product Card](examples/product-card-example.md) - Full implementation example

### [FAQ](faq/faq-general.md)
Common questions answered:
- [General FAQ](faq/faq-general.md) - General questions about HRCE
- [For MVC Developers](faq/faq-mvc-devs.md) - Questions from backend developers
- [For Frontend Developers](faq/faq-frontend-devs.md) - Questions from frontend developers

### [Product Manual (Arabic)](product-manual-ar.md)
Complete product manual in Arabic - دفتر تعليمات المنتج الكامل بالعربية

---

## 🚀 Quick Start

### Installation (5 minutes)

```bash
# Install NuGet package
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation

# Install npm dependencies
npm install svelte

# Add HRCE to your app
```

```csharp
// Program.cs
builder.Services.AddSvelteHybrid(options =>
{
    options.EnableSSR = true;
    options.HydrationMode = HydrationMode.Selective;
});
```

### Create Your First Component (10 minutes)

1. **Create Presentation Model:**
```csharp
public record ProductCardModel(string Title, string Price);
```

2. **Create Presenter:**
```csharp
public class ProductCardPresenter
{
    public ProductCardModel Present(Product p) 
        => new(p.Name, p.Price.ToString("C"));
}
```

3. **Create Template:**
```cshtml
@model ProductCardModel
<div class="card">
    <h3>@Model.Title</h3>
    <p>@Model.Price</p>
</div>
```

4. **Use Component:**
```cshtml
@await Html.PartialAsync("_ProductCard", presenter.Present(product))
```

---

## 📖 Documentation Overview

### By Experience Level

#### Beginners
Start with these:
1. [Introduction](getting-started/introduction.md)
2. [Installation](getting-started/installation.md)
3. [Your First Component](getting-started/first-component.md)
4. [Product Card Example](examples/product-card-example.md)

#### Intermediate
Build on your knowledge:
1. [USCP Pattern](concepts/uscp-pattern.md)
2. [HRCE Architecture](concepts/hrce-architecture.md)
3. [Configuration Guide](reference/configuration.md)
4. [FAQ for MVC Developers](faq/faq-mvc-devs.md)

#### Advanced
Deep technical topics:
1. [Request Lifecycle](architecture/request-lifecycle.md)
2. [Security Model](architecture/security-model.md)
3. [UML Overview](architecture/uml-overview.md)
4. [API Interfaces](reference/interfaces.md)

### By Role

#### Backend Developers (C#/ASP.NET)
Focus on:
- [Installation](getting-started/installation.md)
- [USCP Pattern](concepts/uscp-pattern.md)
- [FAQ for MVC Developers](faq/faq-mvc-devs.md)
- [Configuration](reference/configuration.md)

#### Frontend Developers (JavaScript/Svelte)
Focus on:
- [Your First Component](getting-started/first-component.md)
- [FAQ for Frontend Developers](faq/faq-frontend-devs.md)
- [Product Card Example](examples/product-card-example.md)

#### Architects
Focus on:
- [HRCE Architecture](concepts/hrce-architecture.md)
- [UML Overview](architecture/uml-overview.md)
- [Request Lifecycle](architecture/request-lifecycle.md)
- [Security Model](architecture/security-model.md)

#### Security Engineers
Focus on:
- [Isolation and Security](concepts/isolation-and-security.md)
- [Security Model](architecture/security-model.md)
- [Configuration](reference/configuration.md)

---

## 🎯 Common Tasks

### I want to...

**...understand what HRCE is**
→ Read [Introduction](getting-started/introduction.md)

**...install HRCE in my project**
→ Follow [Installation Guide](getting-started/installation.md)

**...create my first component**
→ Follow [Your First Component](getting-started/first-component.md)

**...understand the architecture**
→ Read [HRCE Architecture](concepts/hrce-architecture.md)

**...see a complete example**
→ Check [Product Card Example](examples/product-card-example.md)

**...configure HRCE for production**
→ Read [Configuration Guide](reference/configuration.md)

**...understand component isolation**
→ Read [Isolation and Security](concepts/isolation-and-security.md)

**...learn the API**
→ Read [API Interfaces](reference/interfaces.md)

**...find answers to common questions**
→ Check [FAQ](faq/faq-general.md)

**...read in Arabic**
→ See [Product Manual (Arabic)](product-manual-ar.md)

---

## 🌟 Key Features

### For MVC Developers
- ✅ No changes to existing Controllers
- ✅ No changes to existing Views
- ✅ Familiar Razor syntax
- ✅ Gradual adoption
- ✅ Clear separation of concerns

### For Frontend Developers
- ✅ Modern component development
- ✅ Svelte (or React/Vue support)
- ✅ Hot reload in development
- ✅ Type-safe props
- ✅ Scoped CSS automatically

### For Everyone
- ✅ Server-side rendering (SSR)
- ✅ SEO-friendly
- ✅ Complete component isolation
- ✅ Component-level authorization
- ✅ Production-ready security

---

## 📊 Documentation Statistics

- **Total Pages:** 15+
- **Code Examples:** 100+
- **Diagrams:** 10+
- **Languages:** English, العربية
- **Last Updated:** December 2024

---

## 🤝 Contributing to Documentation

Found an error? Want to improve the docs?

1. Documentation is in `/docs` directory
2. Written in Markdown
3. Follow existing structure
4. Include code examples
5. Test all examples

---

## 📝 Documentation Conventions

### Code Blocks

**C# code:**
```csharp
public class Example { }
```

**Razor code:**
```cshtml
@model ExampleModel
```

**Svelte code:**
```svelte
<script>
  export let model;
</script>
```

**Configuration:**
```json
{
  "Svelte": { }
}
```

### Admonitions

✅ **DO:** Best practices and recommendations

❌ **DON'T:** Anti-patterns to avoid

⚠️ **WARNING:** Important security or performance considerations

💡 **TIP:** Helpful hints and tricks

---

## 🔗 Additional Resources

### External Links
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Svelte Documentation](https://svelte.dev)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)

### Related Projects
- ASP.NET MVC
- Svelte
- Razor Pages
- Blazor

---

## 📞 Getting Help

**Questions?**
- Read the [FAQ](faq/faq-general.md)
- Check [examples](examples/product-card-example.md)
- Review [architecture docs](architecture/uml-overview.md)

**Found a bug?**
- Check if it's documented
- Report on GitHub Issues
- Include minimal reproduction

**Need a feature?**
- Check roadmap
- Submit feature request
- Contribute a PR

---

## 📄 License

This documentation is licensed under MIT License - free to use, copy, modify, and distribute.

---

## 🎓 Learning Path

### Week 1: Basics
- Day 1: Read Introduction
- Day 2: Install HRCE
- Day 3: Create first component
- Day 4: Study USCP pattern
- Day 5: Review examples

### Week 2: Intermediate
- Day 1: Learn architecture
- Day 2: Understand isolation
- Day 3: Configure for production
- Day 4: Implement authorization
- Day 5: Build complex components

### Week 3: Advanced
- Day 1: Study request lifecycle
- Day 2: Review security model
- Day 3: Performance optimization
- Day 4: Custom renderers
- Day 5: Production deployment

---

## 📈 Version History

### Version 1.0 (Current)
- Complete documentation
- Arabic translation
- UML diagrams
- Code examples
- FAQ sections

---

**Start Learning:** [Introduction to HRCE →](getting-started/introduction.md)

**Quick Reference:** [API Interfaces →](reference/interfaces.md)

**العربية:** [دفتر التعليمات →](product-manual-ar.md)

---

*Documentation maintained by the HRCE team - Last updated: December 2024*
