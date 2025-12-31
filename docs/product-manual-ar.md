# 📘 **دفتر تعليمات المنتج – HRCE**  
## **Hybrid Razor Component Engine**  
### **الإصدار 1.0**

---

# 1) مقدمة

HRCE هو محرك عرض هجين يعمل فوق ASP.NET MVC،  
يسمح بدمج Razor التقليدي مع مكوّنات عرض محسّنة (مثل Svelte SSR)  
داخل نفس ملف `.cshtml`،  
مع عزل كامل للمكوّنات (CSS/JS/Runtime)  
وبدون تغيير أي جزء من MVC.

هذا الدفتر يشرح **كيف يستخدم المطوّر المنتج** خطوة بخطوة.

---

# 2) المتطلبات الأساسية

قبل استخدام HRCE يجب أن تتوفر:

- مشروع ASP.NET MVC أو ASP.NET Core MVC  
- Razor Views تعمل بشكل طبيعي  
- تعريف Presentation Models  
- تعريف Presenters  
- تمكين HRCE في Startup/Program (يتم مرة واحدة فقط)

---

# 3) إنشاء مكوّن جديد

كل مكوّن في HRCE يتكون من 4 أجزاء:

1. **Presentation Model**  
2. **Presenter**  
3. **Razor Template**  
4. **Svelte Template (اختياري)**

ويجب أن تكون جميعها في مجلد واحد.

### 3.1 إنشاء Presentation Model

```csharp
public record CardModel(string Title, string Description, string ImageUrl);
```

### 3.2 إنشاء Presenter

```csharp
public class CardPresenter
{
    public CardModel Map(Product p)
        => new CardModel(p.Name, p.Description, p.ImageUrl);
}
```

### 3.3 إنشاء Razor Template

ملف: `_Card.cshtml`

```cshtml
@svelte "_Card"
@model CardModel

<div class="card-razor">
    <h2>@Model.Title</h2>
</div>
```

> السطر `@svelte "_Card"` هو **مصدر التعليمات** الذي يفعّل HRCE.

### 3.4 إنشاء Svelte Template (اختياري)

ملف: `_Card.svelte`

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

# 4) استدعاء المكوّن في Razor

في أي View:

```cshtml
@await Html.PartialAsync("_Card", Model.MainCard)
```

لا شيء يتغير بالنسبة للمطور.  
HRCE يتولى الباقي.

---

# 5) كيف يعمل HRCE داخلياً (للمطورين المتقدمين)

### 5.1 المرحلة الأولى – Razor Rendering

Razor يعالج `.cshtml` كالمعتاد  
وينتج HTML أولي يحتوي على directive:

```html
@svelte "_Card"
<div class="card-razor">...</div>
```

### 5.2 المرحلة الثانية – Hybrid Processing

HRCE يقرأ الـ HTML ويبحث عن:

```
@svelte "ComponentName"
```

ثم:

1. يستخرج اسم المكوّن  
2. يمرّر الـ Model إلى SvelteRenderer  
3. يحصل على HTML سفلتي  
4. يستبدل directive بالـ HTML الناتج

### 5.3 المرحلة الثالثة – Isolation Layer

HRCE يطبّق:

- **عزل CSS** (scoped hashing)  
- **عزل JS** (runtime sandbox)  
- **عزل DOM** (root element isolation)  
- **عزل الهوية** (Component-level authorization)

---

# 6) قواعد العزل الأمني للمكوّنات

HRCE يفرض تلقائياً:

### 6.1 عزل CSS

- كل CSS داخل المكوّن يُعاد كتابته بـ Hash  
- لا يتأثر بأي CSS خارجي  
- ولا يؤثر على أي عنصر خارج المكوّن

### 6.2 عزل JavaScript

- لا يمكن للمكوّن الوصول إلى `window` أو `document` مباشرة  
- لا يمكنه الاشتراك في Events عامة  
- لا يمكنه تعديل DOM خارج جذر المكوّن

### 6.3 عزل Runtime

- كل مكوّن يعمل داخل Sandbox  
- لا يمكنه الوصول إلى Services أو Context  
- لا يمكنه تنفيذ أي Global Script

### 6.4 عزل الهوية (Component Authorization)

يمكن تحديد سياسة وصول:

```csharp
[ComponentAuthorize(Roles = "Admin")]
```

أو في تعريف المكوّن:

```
SecurityPolicy: "CanViewCard"
```

HRCE يقرر:

- إظهار المكوّن  
- أو إخفاؤه  
- أو استبداله بـ Placeholder

---

# 7) قواعد كتابة المكوّنات

### 7.1 يجب أن يكون لكل مكوّن:

- Model  
- Presenter  
- Razor Template  
- (اختياري) Svelte Template

### 7.2 يجب أن يكون اسم Razor وSvelte متطابقاً

```
_Card.cshtml
_Card.svelte
```

### 7.3 يجب أن يبدأ Razor Template بـ directive

```cshtml
@svelte "_Card"
```

### 7.4 يجب أن يكون Model immutable

### 7.5 يجب أن لا يحتوي Razor Template على منطق أعمال

---

# 8) أفضل الممارسات

### 8.1 اجعل Presenter هو المصدر الوحيد لتحويل البيانات  
### 8.2 اجعل Model بسيطاً وواضحاً  
### 8.3 اجعل Svelte Template مسؤولاً عن التفاعل فقط  
### 8.4 لا تضع CSS أو JS خارج مجلد المكوّن  
### 8.5 لا تستخدم Global Scripts داخل المكوّن

---

# 9) مثال كامل لمكوّن جاهز

```
/Presentation
    /Components
        /Card
            CardModel.cs
            CardPresenter.cs
            _Card.cshtml
            _Card.svelte
            Card.css
```

---

# 10) ماذا يفعل المطوّر بالضبط؟

### 10.1 مطوّر Backend

- يبني Presenter  
- يبني Model  
- يستدعي Partial  
- لا يلمس Svelte

### 10.2 مطوّر Frontend

- يبني `.svelte`  
- يبني CSS  
- لا يلمس Razor

### 10.3 HRCE

- يدمج  
- يعزل  
- يؤمّن  
- يحقن  
- يقدّم HTML النهائي

---

# 11) خاتمة

هذا الدفتر يقدّم **تعليمات تشغيل المنتج HRCE**  
بشكل واضح، بسيط، ومنهجي.

المنتج يوفّر:

- نمطاً موحّداً لبناء المكوّنات  
- عزل كامل CSS/JS/Runtime  
- تكامل Razor + Svelte بدون كسر MVC  
- قابلية تطبيق سياسات الهوية على مستوى المكوّن  
- تجربة تطوير حديثة دون تغيير بنية النظام
