
```
/docs/HRCE-Developer-Instruction-Manual.md
```

هذه الوثيقة مصممة بحيث:

- **تعلّم GitHub Copilot كيف يتصرّف كمطوّر داخل المشروع**  
- **تفرض عليه كل المعايير الهندسية بمسمياتها الرسمية**  
- **تجعله يلتزم بالنمط USCP + HRCE + CSIL**  
- **تضمن أن كل كود يولّده Copilot آمن، معزول، ومتوافق مع الأمن السيبراني**  
- **تصلح للتقييم الأمني قبل النشر في مجتمع .NET**

سأكتبها بصياغة احترافية، منظمة، وبأسلوب وثائق GitHub الرسمية.

---

# 📘 **HRCE – Developer Instruction Manual for GitHub Copilot**  
## **Hybrid Razor Component Engine**  
### **Developer Guidance & Coding Standards**  
**Version 1.0**

---

# 1) Purpose of This Document

This document defines **how GitHub Copilot must behave** when generating code inside the  
**Hybrid Razor Component Engine (HRCE)** repository.

It ensures that Copilot:

- Writes code following the **Unified Server Component Pattern (USCP)**  
- Applies **Component Security Isolation Layer (CSIL)** rules  
- Respects **Razor + Svelte Hybrid Rendering** architecture  
- Produces **secure, isolated, maintainable** components  
- Avoids generating code that violates **cybersecurity requirements**  
- Documents all components using the **official HRCE terminology**

This document is intended for:

- GitHub Copilot  
- Human developers  
- Security auditors  
- .NET community reviewers

---

# 2) Core Architectural Principles Copilot Must Follow

Copilot must adhere to the following architectural principles:

## 2.1 Unified Server Component Pattern (USCP)

Every component must include:

1. **Presentation Model**  
2. **Presenter (Domain → Presentation mapping)**  
3. **Razor Template (.cshtml)**  
4. **Optional Svelte Template (.svelte)**  
5. **Component Invocation** via Razor

Copilot must never generate components that bypass this structure.

---

## 2.2 Hybrid Razor Rendering (HRCE)

Copilot must understand that:

- Razor is the **primary renderer**  
- Svelte is the **secondary renderer**  
- HybridViewEngine merges both outputs  
- Activation happens via directive:

```cshtml
@svelte "_ComponentName"
```

Copilot must always place this directive at the top of hybrid components.

---

## 2.3 Component Security Isolation Layer (CSIL)

Copilot must enforce:

### **CSS Isolation**
- All CSS must be local to the component folder  
- No global CSS  
- No shared selectors  
- No external frameworks unless explicitly allowed  

### **JS Isolation**
- No access to `window`, `document`, or global events  
- No global state  
- No DOM manipulation outside component root  
- No external JS libraries unless approved  

### **Runtime Isolation**
- No access to HttpContext  
- No access to Services/DI  
- No direct API calls  
- No business logic inside Razor/Svelte  

### **Authorization Isolation**
If a component is sensitive, Copilot must generate:

```csharp
[ComponentAuthorize(Roles = "Admin")]
```

or define a policy in metadata.

---

# 3) Folder Structure Copilot Must Follow

Copilot must generate components using this structure:

```
/Presentation
    /Components
        /ComponentName
            ComponentNameModel.cs
            ComponentNamePresenter.cs
            _ComponentName.cshtml
            _ComponentName.svelte
            ComponentName.css
```

Copilot must never place component files outside this structure.

---

# 4) Coding Standards for Copilot

## 4.1 Presentation Model Rules

Copilot must generate:

- Immutable records  
- No methods  
- No logic  

Example:

```csharp
public record CardModel(string Title, string Description, string ImageUrl);
```

---

## 4.2 Presenter Rules

Copilot must:

- Map Domain → Presentation Model  
- Never include business logic  
- Never call external services  
- Never return null  

Example:

```csharp
public class CardPresenter
{
    public CardModel Map(Product p)
        => new CardModel(p.Name, p.Description, p.ImageUrl);
}
```

---

## 4.3 Razor Template Rules

Copilot must:

- Start with `@svelte "_ComponentName"`  
- Use `@model ComponentNameModel`  
- Avoid loops, conditions, or logic  
- Avoid inline JS  
- Avoid inline CSS  

Example:

```cshtml
@svelte "_Card"
@model CardModel

<div class="card-razor">
    <h2>@Model.Title</h2>
</div>
```

---

## 4.4 Svelte Template Rules

Copilot must:

- Use `export let model;`  
- Never access global JS  
- Never modify DOM outside root  
- Use only component-scoped CSS  

Example:

```svelte
<script>
  export let model;
</script>

<div class="card-svelte">
  <img src={model.imageUrl} alt={model.title} />
  <p>{model.description}</p>
</div>
```

---

## 4.5 CSS Rules

Copilot must:

- Create a file named `ComponentName.css`  
- Use only local selectors  
- Avoid global resets  
- Avoid targeting HTML tags directly  

Example:

```css
.card-title {
    font-size: 1.2rem;
}
```

---

# 5) Security Requirements Copilot Must Enforce

Copilot must:

### 5.1 Prevent Security Violations

- No direct DB access  
- No HttpClient usage  
- No external script injection  
- No inline event handlers (`onclick`, etc.)  
- No dynamic script loading  
- No global CSS overrides  

### 5.2 Enforce Component Authorization

If a component is sensitive:

```csharp
[ComponentAuthorize(Roles = "Admin,Manager")]
```

Copilot must add this automatically when appropriate.

---

# 6) Documentation Requirements

Copilot must generate documentation for each component:

### 6.1 Component README.md

Each component folder must include:

```
/Presentation/Components/ComponentName/README.md
```

Containing:

- Component purpose  
- Inputs (Presentation Model)  
- Security policy  
- Rendering behavior  
- Example usage  

---

# 7) Commit Message Standards

Copilot must generate commit messages using:

```
feat(component): add Card component with USCP structure
fix(renderer): correct Svelte SSR merge logic
docs(security): update CSIL isolation rules
```

---

# 8) Pull Request Standards

Copilot must generate PR descriptions including:

- Summary  
- Component structure  
- Security considerations  
- Testing notes  
- Compliance with USCP + HRCE + CSIL  

---

# 9) Forbidden Actions for Copilot

Copilot must **never**:

- Generate business logic inside Razor/Svelte  
- Access HttpContext inside components  
- Generate global JS/CSS  
- Modify MVC routing  
- Create API endpoints  
- Use dynamic eval or unsafe JS  
- Break component isolation rules  

---

# 10) Final Statement for GitHub Copilot

> **GitHub Copilot must behave as a senior HRCE engineer.  
> It must generate code that strictly follows USCP, HRCE, and CSIL.  
> Every component must be isolated, secure, and compliant with cybersecurity standards.  
> No exceptions.**
