# Introduction to HRCE

## What is HRCE?

**HRCE (Hybrid Razor Component Engine)** is a hybrid rendering engine that enables seamless integration of traditional Razor views with modern frontend component frameworks (like Svelte) in ASP.NET MVC applications.

## The Problem HRCE Solves

Traditional ASP.NET MVC applications face several challenges when trying to modernize their frontend:

### 1. **All-or-Nothing Migration**
- Migrating to modern frameworks often requires rewriting the entire application
- No gradual migration path
- High risk and cost

### 2. **Lack of Component Isolation**
- Shared CSS causes style conflicts
- JavaScript pollution of global scope
- No runtime sandboxing

### 3. **Server-Side vs Client-Side Dilemma**
- Server-side rendering provides SEO and performance
- Client-side frameworks provide interactivity
- Difficult to get both benefits

### 4. **Component Authorization**
- No standard way to apply security policies at component level
- All-or-nothing page authorization

## How HRCE Solves These Problems

### ✅ Gradual Migration
- Integrate modern components one at a time
- No need to rewrite existing views
- MVC remains in full control

### ✅ Complete Isolation
- **CSS Isolation**: Scoped styles with automatic hashing
- **JS Isolation**: Runtime sandbox for each component
- **DOM Isolation**: Components can't affect external elements
- **Identity Isolation**: Component-level authorization

### ✅ Best of Both Worlds
- Server-Side Rendering (SSR) for initial load
- Optional client-side hydration for interactivity
- SEO-friendly by default

### ✅ Unified Architecture
- Single pattern for all components
- Clear separation of concerns
- Backend and frontend developers work independently

## Who Should Use HRCE?

HRCE is ideal for:

### 🎯 Legacy MVC Applications
- Applications that need modernization without full rewrite
- Teams that want to adopt modern frontend gradually

### 🎯 Large Enterprise Systems
- Systems requiring strict component isolation
- Applications with complex authorization requirements

### 🎯 Teams with Separated Roles
- Backend developers focused on C# and Razor
- Frontend developers focused on modern JS frameworks
- Need clear boundaries and contracts

## Key Features

### 🚀 Performance
- Server-side rendering for fast initial load
- Selective hydration to minimize JavaScript
- Built-in caching strategies

### 🔒 Security
- Component-level authorization
- Runtime sandboxing prevents malicious code
- Isolation prevents cross-component attacks

### 🎨 Developer Experience
- Familiar Razor syntax
- Modern component development
- Hot reload in development mode

### 📦 Framework Agnostic
- Currently supports Svelte
- Extensible to support React, Vue, etc.
- Easy to add custom renderers

## Core Concepts

### The USCP Pattern

HRCE follows the **Unified Server Component Pattern (USCP)**:

1. **Component**: The visual and interactive unit
2. **Presentation Model**: Data structure for the component
3. **Presenter**: Transforms domain data to presentation data
4. **Renderer**: Converts model + template to HTML
5. **Invocation**: Standard way to use the component

### The Hybrid Approach

```
Traditional Razor View
        ↓
@svelte "ComponentName" ← HRCE Directive
        ↓
HRCE Processing
        ↓
Razor HTML + Svelte SSR HTML
        ↓
Final Isolated Output
```

## Architecture Overview

```
┌─────────────────────────────────────┐
│         ASP.NET MVC                 │
│  (Controllers, Services, Models)    │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│      Hybrid View Engine             │
│  - Razor Processing                 │
│  - Directive Detection              │
│  - Component Resolution             │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│      Svelte Renderer (SSR)          │
│  - Server-Side Rendering            │
│  - Component Isolation              │
│  - HTML Generation                  │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│      Isolation Layer                │
│  - CSS Scoping                      │
│  - JS Sandboxing                    │
│  - Authorization Check              │
└──────────────┬──────────────────────┘
               ↓
          Final HTML
```

## What HRCE is NOT

### ❌ Not a Full SPA Framework
HRCE doesn't replace MVC - it enhances it. MVC remains in control.

### ❌ Not Client-Side Only
HRCE focuses on server-side rendering with optional hydration.

### ❌ Not a Build Tool
HRCE works with your existing build pipeline.

### ❌ Not Framework-Specific
While it currently uses Svelte, the architecture supports any SSR-capable framework.

## Next Steps

Ready to get started?

1. [Install HRCE](installation.md) in your ASP.NET MVC project
2. [Create your first component](first-component.md)
3. Explore the [core concepts](../concepts/uscp-pattern.md)

## Additional Resources

- [Product Manual (Arabic)](../product-manual-ar.md)
- [Architecture Overview](../architecture/uml-overview.md)
- [FAQ](../faq/faq-general.md)
