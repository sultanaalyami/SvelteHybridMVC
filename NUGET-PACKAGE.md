# ?? SvelteHybrid.AspNetCore v2.0

**Zero-config Svelte integration for ASP.NET Core - Works instantly!**

## ? ÇáÅÕÏÇÑ 2.0 - ÅÚÇÏÉ ÈäÇÁ ßÇãáÉ!

ÇáÂä ÇáÍÒãÉ ÊÚãá **ãä ÇáÕäÏæŞ** ÈÓØÑ æÇÍÏ İŞØ!

## ?? ãæŞÚ ÇáÍÒãÉ

```
packages/
??? SvelteHybrid.AspNetCore.2.0.0.nupkg      # ÇáÍÒãÉ ÇáÌÏíÏÉ
??? SvelteHybrid.AspNetCore.2.0.0.snupkg     # ÑãæÒ ÇáÊÕÍíÍ
```

## ?? ÇáÇÓÊÎÏÇã - ÓØÑ æÇÍÏ İŞØ!

```csharp
// Program.cs
app.UseSvelteHybrid();  // ? åĞÇ ßá ÔíÁ!
```

## ? ãÇĞÇ ÊÍÕá İæÑÇğ¿

### ŞÈá (MVC ÊŞáíÏí ããá)
```html
<button onclick="location.reload()">ÇÖÛØäí</button>
<div>ãÍÊæì ËÇÈÊ íÍÊÇÌ ÅÚÇÏÉ ÊÍãíá</div>
```

### ÈÚÏ (ÓÍÑ ÊİÇÚáí!)
```html
<div s-reactive s-data="{ count: 0 }">
    <button s-click="count++">ÖÛØÊ {count} ãÑÉ</button>
    <input s-model="name" />
    <p>ãÑÍÈÇğ¡ {name}!</p>
</div>
```

## ?? ÃãËáÉ ÓÑíÚÉ

### ÚÏÇÏ (ÈÏæä JavaScript!)
```html
<div s-reactive s-data="{ count: 0 }">
    <button s-click="count++">ÇáÚÏÏ: {count}</button>
</div>
```

### ÑÈØ ËäÇÆí ÇáÇÊÌÇå
```html
<div s-reactive s-data="{ text: '' }">
    <input s-model="text" placeholder="ÇßÊÈ åäÇ..." />
    <p>ßÊÈÊ: {text}</p>
</div>
```

### ÚÑÖ ãÔÑæØ
```html
<div s-reactive s-data="{ show: false }">
    <button s-click="show = !show">ÊÈÏíá</button>
    <div s-if="show">ÇáÂä ÊÑÇäí!</div>
</div>
```

### ŞÇÆãÉ ãåÇã
```html
<div s-reactive s-data="{ todos: [], newTodo: '' }">
    <input s-model="newTodo" />
    <button s-click="todos.push({text: newTodo}); newTodo = ''">ÅÖÇİÉ</button>
    <template s-for="todo in todos">
        <li>{todo.text}</li>
    </template>
</div>
```

### ÅÔÚÇÑÇÊ Toast
```html
<button s-click="$toast.success('Êã ÈäÌÇÍ!')">ÍİÙ</button>
```

## ?? ÇáÊæÌíåÇÊ ÇáãÊÇÍÉ

| ÇáÊæÌíå | ÇáæÕİ | ãËÇá |
|---------|-------|------|
| `s-reactive` | ÊİÚíá ÇáÊİÇÚáíÉ | `<div s-reactive>` |
| `s-data` | ÊÚÑíİ ÇáÍÇáÉ | `s-data="{ count: 0 }"` |
| `s-click` | ÍÏË ÇáäŞÑ | `s-click="count++"` |
| `s-model` | ÑÈØ ËäÇÆí | `s-model="name"` |
| `s-if` | ÚÑÖ ãÔÑæØ | `s-if="show"` |
| `s-else` | ÇáİÑÚ ÇáÈÏíá | `s-else` |
| `s-for` | ÍáŞÉ | `s-for="item in items"` |
| `s-show` | ÅÙåÇÑ/ÅÎİÇÁ | `s-show="visible"` |
| `s-class` | İÆÉ ÏíäÇãíßíÉ | `s-class="{ active: isActive }"` |
| `s-disabled` | ÊÚØíá | `s-disabled="loading"` |
| `s-submit` | ÅÑÓÇá äãæĞÌ | `s-submit="await save()"` |

## ?? ãßæäÇÊ ãÏãÌÉ

```html
<!-- Toast -->
<button s-click="$toast.success('Êã!')">ÅÔÚÇÑ</button>

<!-- Loading -->
<s-loading s-show="isLoading"></s-loading>

<!-- Modal -->
<s-modal id="confirm">ãÍÊæì</s-modal>
<button s-click="$modal.open('confirm')">İÊÍ</button>

<!-- Tabs -->
<s-tabs>
    <s-tab title="ÊÈæíÈ 1">ãÍÊæì 1</s-tab>
    <s-tab title="ÊÈæíÈ 2">ãÍÊæì 2</s-tab>
</s-tabs>
```

## ?? ÇáãŞÇÑäÉ

| ÇáãíÒÉ | MVC ÇáÊŞáíÏí | ãÚ SvelteHybrid |
|--------|-------------|-----------------|
| ÅÚÇÏÉ ÊÍãíá ÇáÕİÍÉ | ßá ÅÌÑÇÁ | ÃÈÏÇğ |
| æŞÊ ÇáÅÚÏÇÏ | ÓÇÚÇÊ | 30 ËÇäíÉ |
| JavaScript ãØáæÈ | äÚã¡ ßËíÑ | áÇ |
| ÍÌã ÇáÍÒãÉ | 0 KB | 15 KB |

## ?? ÇáÃäÙãÉ ÇáãÏÚæãÉ

- ? .NET 8.0
- ? .NET 9.0
- ? .NET 10.0

## ?? ÇáÊÑÎíÕ

MIT License
