# FAQ - For Frontend Developers

Common questions from frontend developers about working with HRCE.

---

## Do I need to know ASP.NET MVC?

**For component development: No.**

Your workflow:
1. Receive Presentation Model spec (TypeScript interface)
2. Build Svelte component that accepts that model
3. Test component in isolation
4. Done!

**Example:**

You receive this spec:
```typescript
interface ProductCardModel {
  title: string;
  description: string;
  priceDisplay: string;
  imageUrl: string;
}
```

You build:
```svelte
<script>
  export let model;
</script>

<div class="card">
  <img src={model.imageUrl} alt={model.title} />
  <h3>{model.title}</h3>
  <p>{model.description}</p>
  <span>{model.priceDisplay}</span>
</div>
```

---

## How do I get data into my components?

**You don't!** The server passes data to your component via the `model` prop.

**Server-side (backend team):**
```csharp
var model = new ProductCardModel 
{
    Title = "Headphones",
    PriceDisplay = "$79.99",
    ImageUrl = "/images/headphones.jpg"
};
```

**Your component:**
```svelte
<script>
  export let model; // Automatically populated by server
</script>

<div>{model.title}</div>
```

---

## Can I fetch data from APIs in my component?

**Generally no.** HRCE focuses on server-side rendering.

**Instead:**
- Server fetches all data
- Server passes complete model to component
- Component renders with that data

**Exception:** For client-side enhancements:
```svelte
<script>
  export let model;
  
  let reviews = [];
  
  onMount(async () => {
    // Load additional data after hydration
    reviews = await fetch(`/api/reviews/${model.productId}`).then(r => r.json());
  });
</script>
```

---

## Do my components need to be SSR-compatible?

**Yes.** All components must support server-side rendering.

**This means:**

✅ **DO:**
```svelte
<script>
  export let model;
  
  // Safe - works on server and client
  let count = 0;
</script>
```

❌ **DON'T:**
```svelte
<script>
  // Unsafe - window doesn't exist on server
  const width = window.innerWidth;
  
  // Unsafe - localStorage doesn't exist on server
  const saved = localStorage.getItem('key');
</script>
```

**Instead, use lifecycle hooks:**
```svelte
<script>
  import { onMount } from 'svelte';
  
  let width = 0;
  
  onMount(() => {
    // Safe - only runs on client
    width = window.innerWidth;
  });
</script>
```

---

## What about component state?

**Local state is fine:**
```svelte
<script>
  export let model;
  
  let quantity = 1; // Local state
  let isAdded = false;
  
  function addToCart() {
    isAdded = true;
    // Handle add to cart
  }
</script>

<button on:click={addToCart}>
  {isAdded ? 'Added!' : 'Add to Cart'}
</button>
```

**For shared state:**
- Use Svelte stores (client-side only)
- Or use server-side state management
- Components are isolated - can't share state directly

---

## Can I use CSS frameworks like Tailwind?

**Yes, but with considerations:**

**Global CSS (applied to entire page):**
```html
<!-- _Layout.cshtml -->
<link rel="stylesheet" href="/css/tailwind.css">
```

**Component-scoped CSS:**
```svelte
<style>
  /* This CSS is automatically scoped to this component */
  .card {
    @apply rounded-lg shadow-lg p-4;
  }
</style>
```

**Best practice:**
- Use Tailwind classes directly in markup
- Use `<style>` for component-specific overrides
- HRCE will scope component styles automatically

---

## How do I handle user events?

**Standard Svelte event handling:**

```svelte
<script>
  export let model;
  
  function handleClick() {
    console.log('Clicked!');
    // Handle event
  }
  
  async function handleSubmit(event) {
    event.preventDefault();
    
    // Post to server
    await fetch('/api/products', {
      method: 'POST',
      body: JSON.stringify(formData)
    });
  }
</script>

<button on:click={handleClick}>Click Me</button>
<form on:submit={handleSubmit}>...</form>
```

**Note:** Events only work after hydration. For no-JS scenarios, use forms that post back to MVC.

---

## What's this "hydration" thing?

**Hydration** = Making server-rendered HTML interactive.

**Without hydration:**
- Component renders on server
- Browser displays static HTML
- No interactivity

**With hydration:**
- Component renders on server
- Browser displays HTML
- JavaScript loads and "hydrates" component
- Buttons, inputs become interactive

**You don't control hydration** - it's configured by the backend:
```csharp
builder.Services.AddSvelteHybrid(options =>
{
    options.HydrationMode = HydrationMode.Selective; // Backend decides
});
```

---

## Can I use npm packages?

**Yes!** Install normally:

```bash
npm install lodash-es
npm install dayjs
npm install @sveltejs/kit
```

**Use in components:**
```svelte
<script>
  import { format } from 'dayjs';
  import { debounce } from 'lodash-es';
  
  export let model;
  
  const formattedDate = format(model.createdAt, 'MMM DD, YYYY');
</script>
```

**SSR Consideration:**
- Package must work in Node.js/V8
- Avoid packages that require `window` or `document`

---

## How do I style components?

**Option 1: Scoped Styles**
```svelte
<div class="card">
  <h3>{model.title}</h3>
</div>

<style>
  /* Automatically scoped - won't affect other components */
  .card {
    border: 1px solid #ccc;
    padding: 16px;
  }
  
  h3 {
    color: #333;
    font-size: 1.5rem;
  }
</style>
```

**Option 2: CSS Modules**
```svelte
<script>
  import styles from './Card.module.css';
</script>

<div class={styles.card}>
  <h3 class={styles.title}>{model.title}</h3>
</div>
```

**Option 3: Utility Classes**
```svelte
<div class="rounded-lg shadow-lg p-4 bg-white">
  <h3 class="text-2xl font-bold">{model.title}</h3>
</div>
```

---

## Can I use TypeScript?

**Yes!** Svelte has excellent TypeScript support:

```svelte
<script lang="ts">
  interface ProductCardModel {
    title: string;
    description: string;
    priceDisplay: string;
    imageUrl: string;
  }
  
  export let model: ProductCardModel;
  
  let quantity: number = 1;
  
  function addToCart(): void {
    // Type-safe code
  }
</script>
```

**Setup:**
```bash
npm install --save-dev typescript
npm install --save-dev @tsconfig/svelte
```

---

## How do I test my components?

**Unit Testing:**
```javascript
import { render } from '@testing-library/svelte';
import ProductCard from './ProductCard.svelte';

test('renders product title', () => {
  const model = {
    title: 'Test Product',
    description: 'Test desc',
    priceDisplay: '$99.99',
    imageUrl: '/test.jpg'
  };
  
  const { getByText } = render(ProductCard, { props: { model } });
  
  expect(getByText('Test Product')).toBeInTheDocument();
});
```

**Tools:**
- Vitest
- Testing Library
- Jest
- Playwright

---

## Can components communicate with each other?

**No, components are isolated.**

**Alternatives:**

**1. Shared model from server:**
```cshtml
@* Backend provides all data *@
@await Html.PartialAsync("_ProductCard", Model.Product)
@await Html.PartialAsync("_ProductActions", Model.Product)
```

**2. Events up to parent (after hydration):**
```svelte
<script>
  import { createEventDispatcher } from 'svelte';
  const dispatch = createEventDispatcher();
  
  function handleClick() {
    dispatch('productClicked', { id: model.id });
  }
</script>
```

**3. Stores (client-side only):**
```javascript
// stores.js
import { writable } from 'svelte/store';
export const cart = writable([]);
```

---

## What about routing?

**HRCE doesn't handle routing.** MVC handles all routing.

**Navigation:**
```svelte
<!-- Use standard links -->
<a href="/products/{model.id}">View Details</a>

<!-- Or programmatic navigation -->
<script>
  function goToProduct() {
    window.location.href = `/products/${model.id}`;
  }
</script>
```

**No client-side routing** (no SvelteKit routing, no React Router).

---

## Can I use animations?

**Yes!** Svelte animations work:

```svelte
<script>
  import { fade, slide } from 'svelte/transition';
  export let model;
  
  let visible = false;
</script>

<button on:click={() => visible = !visible}>
  Toggle
</button>

{#if visible}
  <div transition:fade>
    {model.description}
  </div>
{/if}
```

**Note:** Animations only work after hydration.

---

## How do I handle forms?

**Option 1: Server-side form handling (recommended)**
```svelte
<script>
  export let model;
</script>

<!-- Standard form posts to MVC controller -->
<form action="/products/create" method="post">
  <input name="Name" value={model.title} />
  <input name="Price" value={model.price} />
  <button type="submit">Save</button>
</form>
```

**Option 2: Client-side with API**
```svelte
<script>
  async function handleSubmit(event) {
    event.preventDefault();
    
    const response = await fetch('/api/products', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData)
    });
    
    if (response.ok) {
      window.location.href = '/products';
    }
  }
</script>

<form on:submit={handleSubmit}>
  ...
</form>
```

---

## What's the component file structure?

```
/SvelteApp
  /src
    /_ProductCard.svelte        ← Your component
    /_ProductList.svelte
    /_UserProfile.svelte
    /shared
      /Button.svelte            ← Shared components
      /Icon.svelte
```

**Naming convention:**
- Components used by HRCE: Start with `_` (e.g., `_ProductCard.svelte`)
- Internal/shared components: No `_` (e.g., `Button.svelte`)

---

## How do I debug my components?

**Server-Side (SSR):**
```svelte
<script>
  export let model;
  
  // Logs appear in server console
  console.log('Rendering with model:', model);
</script>
```

**Client-Side (After Hydration):**
```svelte
<script>
  import { onMount } from 'svelte';
  
  onMount(() => {
    // Logs appear in browser console
    console.log('Component hydrated');
  });
</script>
```

**Browser DevTools:**
- Svelte DevTools extension
- Standard browser DevTools
- React DevTools (for React renderer)

---

## Can I use web components?

**Yes!** Svelte can compile to web components:

```svelte
<svelte:options tag="product-card" />

<script>
  export let model;
</script>

<div class="card">
  {model.title}
</div>
```

**But:** HRCE currently expects Svelte components, not web components. This is a future enhancement.

---

## What about accessibility (a11y)?

**Svelte has built-in a11y warnings:**

```svelte
<!-- Svelte will warn about missing alt text -->
<img src={model.imageUrl} />

<!-- Better -->
<img src={model.imageUrl} alt={model.title} />
```

**Best practices:**
- Use semantic HTML
- Add ARIA labels where needed
- Test with screen readers
- Ensure keyboard navigation works

---

## How do I work with the backend team?

**Contract: Presentation Model**

Backend defines:
```csharp
public record ProductCardModel
{
    public string Title { get; init; }
    public string Description { get; init; }
    public string PriceDisplay { get; init; }
    public string ImageUrl { get; init; }
}
```

You receive TypeScript equivalent:
```typescript
interface ProductCardModel {
  title: string;
  description: string;
  priceDisplay: string;
  imageUrl: string;
}
```

**That's your contract** - if model structure changes, both teams update.

---

## What's the development workflow?

1. **Get model spec** from backend team
2. **Create Svelte component** in `/SvelteApp/src`
3. **Test in isolation** with mock data
4. **Build:** `npm run build`
5. **Backend integrates** component into view
6. **Test full page** in browser

**Hot reload:**
```bash
npm run dev  # Watch mode
```

Changes auto-rebuild when you save.

---

## Next Steps

- [Create Your First Component](../getting-started/first-component.md)
- [Learn About SSR Constraints](../concepts/hrce-architecture.md)
- [See Complete Examples](../examples/product-card-example.md)
- [Read FAQ for MVC Developers](faq-mvc-devs.md)
