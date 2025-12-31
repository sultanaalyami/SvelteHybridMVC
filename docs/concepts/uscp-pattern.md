# The USCP Pattern

## Unified Server Component Pattern

HRCE implements the **Unified Server Component Pattern (USCP)**, a architectural pattern designed to standardize component development across server-rendered applications.

---

## The Five Pillars of USCP

### 1. Component
The visual and interactive unit that users see and interact with.

**Characteristics:**
- Self-contained UI element
- Isolated from other components
- Reusable across views
- Can be composed with other components

**Example:**
```
ProductCard component displays product information
```

---

### 2. Presentation Model
A simple, immutable data structure designed specifically for display.

**Characteristics:**
- Immutable (use `record` types in C#)
- Display-ready data only (no raw domain data)
- No logic or behavior
- Serializable for SSR

**Example:**
```csharp
public record ProductCardModel
{
    public required string Title { get; init; }
    public required string PriceDisplay { get; init; } // "$99.99" not decimal
    public required string ImageUrl { get; init; }
}
```

**Why Presentation Models?**
- ✅ Separation of concerns (domain vs. presentation)
- ✅ Stable contract between backend and frontend
- ✅ Easy to test and mock
- ✅ Safe to serialize and pass to client

---

### 3. Presenter
The transformation layer that converts domain models to presentation models.

**Characteristics:**
- Pure transformation logic
- No business rules (those stay in services)
- Handles formatting, null checks, defaults
- Stateless and testable

**Example:**
```csharp
public class ProductCardPresenter
{
    public ProductCardModel Present(Product product)
    {
        return new ProductCardModel
        {
            Title = product.Name,
            PriceDisplay = FormatPrice(product.Price),
            ImageUrl = product.ImageUrl ?? "/images/placeholder.png"
        };
    }

    private string FormatPrice(decimal price) 
        => $"${price:F2}";
}
```

**Presenter Responsibilities:**
- ✅ Data transformation
- ✅ Formatting (dates, currency, numbers)
- ✅ Null handling and defaults
- ✅ Aggregating data from multiple sources
- ❌ NOT business logic
- ❌ NOT data validation
- ❌ NOT data persistence

---

### 4. Renderer
The engine that converts a template + model into HTML.

**In HRCE:**
- Primary: Razor View Engine
- Secondary: Svelte SSR Renderer
- Future: React, Vue, etc.

**Characteristics:**
- Takes a Presentation Model as input
- Executes template logic
- Produces isolated HTML output
- Applies security and isolation policies

**Example Flow:**
```
ProductCardModel + _ProductCard.cshtml
            ↓
     Razor Renderer
            ↓
        HTML Output
```

---

### 5. Invocation
The standard mechanism for using a component.

**In HRCE:**
```cshtml
@await Html.PartialAsync("ComponentName", presentationModel)
```

**Characteristics:**
- Consistent across all components
- Type-safe (strongly typed models)
- Easy to learn and use
- Works with existing MVC knowledge

---

## The USCP Workflow

```
┌─────────────────────────────────────────────────────┐
│ 1. CONTROLLER                                       │
│    Gets domain data (Product)                       │
└──────────────────┬──────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────┐
│ 2. PRESENTER                                        │
│    Transforms Product → ProductCardModel            │
└──────────────────┬──────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────┐
│ 3. VIEW                                             │
│    Passes model to component via Html.PartialAsync  │
└──────────────────┬──────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────┐
│ 4. RENDERER (Razor + Svelte)                        │
│    Generates isolated HTML                          │
└──────────────────┬──────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────┐
│ 5. OUTPUT                                           │
│    Final HTML sent to client                        │
└─────────────────────────────────────────────────────┘
```

---

## Benefits of USCP

### 🎯 Clear Separation of Concerns
Each layer has a single, well-defined responsibility.

### 🔄 Testability
Each piece can be tested in isolation:
- Presenters: Unit tests with mock domain objects
- Models: Structure validation
- Components: Visual regression tests

### 📦 Reusability
Components can be used anywhere that needs the same presentation.

### 🛡️ Type Safety
Strong typing throughout the pipeline prevents runtime errors.

### 👥 Team Collaboration
- Backend devs: Focus on Domain → Presenter → Model
- Frontend devs: Focus on Model → Template → Styling
- Clear contract: the Presentation Model

---

## USCP vs Traditional Patterns

### vs. MVC (Model-View-Controller)

| Aspect | Traditional MVC | USCP |
|--------|-----------------|------|
| Model | Domain model directly | Presentation model |
| View | Tightly coupled | Component isolated |
| Reusability | Page-level | Component-level |
| Type Safety | Model-based | Presentation-model-based |

### vs. MVVM (Model-View-ViewModel)

| Aspect | MVVM | USCP |
|--------|------|------|
| ViewModel | Usually in View layer | Presenter in app layer |
| Binding | Two-way binding | Server-side only (optional client hydration) |
| State | Client-side state | Server-side state |

### vs. Component-Driven Design

| Aspect | Pure Component | USCP Component |
|--------|----------------|----------------|
| Data | Props directly | Presentation Model |
| Transformation | In component | In Presenter |
| Server Rendering | Often separate | Built-in |
| Isolation | Developer-managed | Automatic |

---

## Real-World Example

Let's build a User Profile component using USCP:

### 1. Domain Model (exists in your app)
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<Role> Roles { get; set; }
}
```

### 2. Presentation Model
```csharp
public record UserProfileModel
{
    public required string DisplayName { get; init; }
    public required string Email { get; init; }
    public required string MemberSince { get; init; }
    public required string LastActive { get; init; }
    public required IEnumerable<string> Badges { get; init; }
}
```

### 3. Presenter
```csharp
public class UserProfilePresenter
{
    public UserProfileModel Present(User user)
    {
        return new UserProfileModel
        {
            DisplayName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            MemberSince = FormatDate(user.CreatedAt),
            LastActive = FormatLastActive(user.LastLoginAt),
            Badges = user.Roles.Select(r => r.Name)
        };
    }

    private string FormatDate(DateTime date)
        => date.ToString("MMMM yyyy");

    private string FormatLastActive(DateTime? lastLogin)
        => lastLogin.HasValue 
            ? $"Active {GetRelativeTime(lastLogin.Value)}"
            : "Never logged in";

    private string GetRelativeTime(DateTime date)
    {
        var span = DateTime.UtcNow - date;
        return span.TotalDays switch
        {
            < 1 => "today",
            < 7 => $"{(int)span.TotalDays} days ago",
            < 30 => $"{(int)(span.TotalDays / 7)} weeks ago",
            _ => $"{(int)(span.TotalDays / 30)} months ago"
        };
    }
}
```

### 4. Component Template
```cshtml
@model UserProfileModel
@svelte "_UserProfile"

<div class="user-profile">
    <h2>@Model.DisplayName</h2>
    <p>@Model.Email</p>
    <div class="user-meta">
        <span>Member since @Model.MemberSince</span>
        <span>@Model.LastActive</span>
    </div>
    <div class="user-badges">
        @foreach (var badge in Model.Badges)
        {
            <span class="badge">@badge</span>
        }
    </div>
</div>
```

### 5. Invocation
```cshtml
@inject UserProfilePresenter _presenter

@{
    var currentUser = await UserService.GetCurrentUser();
    var profileModel = _presenter.Present(currentUser);
}

@await Html.PartialAsync("_UserProfile", profileModel)
```

---

## USCP Best Practices

### ✅ DO:
- Keep Presentation Models simple and flat
- Make Presenters pure functions (no side effects)
- Use immutable records for models
- Format all display data in the Presenter
- Write unit tests for Presenters

### ❌ DON'T:
- Put business logic in Presenters
- Pass domain models directly to components
- Make Presenters stateful
- Do data access in Presenters
- Mix presentation and domain concerns

---

## Summary

USCP provides:
- **Component**: The UI element
- **Presentation Model**: Display-ready data
- **Presenter**: Transformation logic
- **Renderer**: HTML generation
- **Invocation**: Standard usage pattern

Together, these five pillars create a robust, testable, and maintainable component architecture for server-rendered applications.

---

## Next Steps

- [Learn about HRCE Architecture](hrce-architecture.md)
- [Understand Component Isolation](isolation-and-security.md)
- [Build your first component](../getting-started/first-component.md)
