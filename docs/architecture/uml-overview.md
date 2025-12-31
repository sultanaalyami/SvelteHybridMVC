# UML Overview - HRCE Architecture

This document provides UML diagrams representing the HRCE (Hybrid Razor Component Engine) architecture.

---

## 1. Class Diagram - Core Components

The following text-based representation shows the relationships between core classes in HRCE:

```text
+---------------------+
|   Product           |
+---------------------+
| Id                  |
| Name                |
| Description         |
| ImageUrl            |
| Price               |
+---------------------+

        |
        |  (Input)
        v

+-----------------------------+
|   ProductCardPresenter      |
+-----------------------------+
| + Map(product: Product)     |
|       : CardModel           |
+-----------------------------+

        |
        |  (creates)
        v

+-----------------------------+
|         CardModel           |
+-----------------------------+
| Title        : string       |
| Description  : string       |
| ImageUrl     : string       |
| PriceDisplay : string       |
+-----------------------------+

        |
        |  (passed to)
        v

+-----------------------------+          +---------------------------+
|      Razor View (_Card)     |          |   Svelte Template        |
|       (.cshtml)             |          |     (_Card.svelte)       |
+-----------------------------+          +---------------------------+
| @model CardModel            |          | export let model;        |
| @svelte "_Card"             |          | <div>...{model}...</div> |
+-----------------------------+          +---------------------------+

        |
        |  (rendered by)
        v

+----------------------------------+
|        IRazorViewEngine          |
+----------------------------------+
| + FindView(...)                  |
| + RenderView(...)                |
+----------------------------------+

        |
        | (wrapped by)
        v

+----------------------------------+
|       HybridViewEngine           |
+----------------------------------+
| - _razor: IRazorViewEngine       |
| - _svelte: ISvelteRenderer       |
+----------------------------------+
| + RenderAsync(viewName, model,   |
|              context) : string   |
+----------------------------------+

        |
        | (delegates)
        v

+----------------------------------+
|        ISvelteRenderer           |
+----------------------------------+
| + RenderAsync(                   |
|     componentName: string,       |
|     model: object                |
|   ) : Task<string>               |
+----------------------------------+
```

### Key Relationships:

- `ProductCardPresenter` depends on `Product` and creates `CardModel`
- `Razor View (_Card.cshtml)` and `_Card.svelte` both consume `CardModel`
- `HybridViewEngine` wraps `IRazorViewEngine` and delegates to `ISvelteRenderer`
- All components maintain strict separation of concerns

---

## 2. Sequence Diagram - Request Lifecycle

The following diagram shows the complete request flow through HRCE:

```text
Client           Controller        RazorViewEngine      HybridViewEngine       ISvelteRenderer
  |                  |                    |                     |                    |
  |  GET /Details    |                    |                     |                    |
  |----------------->|                    |                     |                    |
  |                  |  Get Product       |                     |                    |
  |                  |------------------> |                     |                    |
  |                  |        ...         |                     |                    |
  |                  |  Build ViewModel   |                     |                    |
  |                  |--------------------|                     |                    |
  |                  | return View(...)   |                     |                    |
  |                  |------------------->|                     |                    |
  |                  |                    |  RenderWithRazor    |                    |
  |                  |                    |-------------------->|                    |
  |                  |                    |   HTML + @svelte    |                    |
  |                  |                    |<--------------------|                    |
  |                  |                    |                     | ProcessHybrid      |
  |                  |                    |                     |------------------> |
  |                  |                    |                     |  RenderAsync(      |
  |                  |                    |                     |    "_Card",        |
  |                  |                    |                     |     CardModel)     |
  |                  |                    |                     |------------------>|
  |                  |                    |                     |  svelteHtml        |
  |                  |                    |                     |<------------------|
  |                  |                    |                     | merge + return     |
  |                  |                    |<--------------------|                    |
  |                  | final HTML         |                     |                    |
  |<-----------------|                    |                     |                    |
  |  Render page     |                    |                     |                    |
```

### Flow Description:

1. Client sends HTTP request to Controller
2. Controller retrieves Product data
3. Controller builds ViewModel and returns Razor view
4. RazorViewEngine processes the `.cshtml` file
5. HybridViewEngine intercepts the output
6. HybridViewEngine detects `@svelte` directives
7. ISvelteRenderer performs SSR for Svelte components
8. HybridViewEngine merges Razor and Svelte output
9. Final HTML is sent to client

---

## 3. Component Diagram - System Architecture

This diagram shows the high-level component structure:

```text
+---------------------- Application Layer ----------------------+
|                                                              |
|  +----------------+      +---------------------------+       |
|  | Controllers    |      |   Services / Domain       |       |
|  +----------------+      +---------------------------+       |
|                                                              |
+---------------------- Presentation Layer --------------------+
|                                                              |
|  +-----------------+      +------------------------------+   |
|  | Razor Views     |----->|  HybridViewEngine           |   |
|  | (.cshtml)       |      |  (wraps IRazorViewEngine)   |   |
|  +-----------------+      +------------------------------+   |
|                              |                             |
|                              v                             |
|                      +------------------+                  |
|                      | ISvelteRenderer  |                  |
|                      +------------------+                  |
|                              |                             |
|                              v                             |
|                      +------------------+                  |
|                      | Svelte Runtime   |                  |
|                      | (SSR In-Process) |                  |
|                      +------------------+                  |
|                                                              |
+--------------------------------------------------------------+

+---------------------- Isolation Layer -----------------------+
|                                                              |
|  +------------------+  +------------------+  +-------------+ |
|  | CSS Isolation    |  | JS Sandbox       |  | Component   | |
|  | (Scoped Hashing) |  | (Runtime Guard)  |  | AuthZ       | |
|  +------------------+  +------------------+  +-------------+ |
|                                                              |
+--------------------------------------------------------------+
```

### Component Responsibilities:

**Application Layer:**
- Controllers handle HTTP requests
- Services contain business logic
- Domain models represent business entities

**Presentation Layer:**
- Razor Views define the page structure
- HybridViewEngine orchestrates rendering
- ISvelteRenderer performs SSR for Svelte components
- Svelte Runtime executes component logic

**Isolation Layer:**
- CSS Isolation prevents style conflicts
- JS Sandbox restricts component capabilities
- Component AuthZ enforces access control

---

## 4. Deployment Diagram

```text
+----------------------------------------------------------+
|                    Web Browser (Client)                  |
|                                                          |
|  +--------------------+    +-------------------------+   |
|  | HTML/CSS/JS        |    | Hydrated Components     |   |
|  +--------------------+    +-------------------------+   |
+----------------------------------------------------------+
                             |
                             | HTTP/HTTPS
                             v
+----------------------------------------------------------+
|                   ASP.NET Core Web Server                |
|                                                          |
|  +--------------------+    +-------------------------+   |
|  | MVC Controllers    |    | HybridViewEngine        |   |
|  +--------------------+    +-------------------------+   |
|            |                         |                   |
|            v                         v                   |
|  +--------------------+    +-------------------------+   |
|  | Razor Engine       |    | SvelteRenderer (SSR)    |   |
|  +--------------------+    +-------------------------+   |
|                                                          |
+----------------------------------------------------------+
                             |
                             | File System
                             v
+----------------------------------------------------------+
|                    Component Storage                     |
|                                                          |
|  /Views/Shared/_Card.cshtml                              |
|  /SvelteApp/src/_Card.svelte                             |
|  /Presentation/Models/CardModel.cs                       |
|  /Presentation/Presenters/CardPresenter.cs               |
+----------------------------------------------------------+
```

---

## 5. State Diagram - Component Lifecycle

```text
                    [Component Request]
                            |
                            v
                    +---------------+
                    |  Uninitialized|
                    +---------------+
                            |
                            v
                    +---------------+
                    |  Model Created|
                    |  by Presenter |
                    +---------------+
                            |
                            v
                    +---------------+
                    | Razor Rendered|
                    | (with @svelte)|
                    +---------------+
                            |
                            v
                    +---------------+
                    |  Svelte SSR   |
                    |   Executed    |
                    +---------------+
                            |
                            v
                    +---------------+
                    | HTML Merged & |
                    |   Isolated    |
                    +---------------+
                            |
                            v
                    +---------------+
                    |   Sent to     |
                    |    Client     |
                    +---------------+
                            |
                            v
                    +---------------+
                    |  Hydrated     |
                    | (if enabled)  |
                    +---------------+
```

---

## Notes on UML Representations

These diagrams are represented in text format for clarity and portability. They can be converted to visual UML diagrams using tools such as:

- PlantUML
- Draw.io
- Lucidchart
- Visual Paradigm
- Enterprise Architect

The text-based format ensures the documentation remains version-control friendly and can be easily updated alongside code changes.
