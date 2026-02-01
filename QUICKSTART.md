# HRCE - Hybrid Razor Component Engine
## البدء السريع

تم تنفيذ HRCE بنجاح في هذا المشروع! 🎉

## 📁 هيكل المشروع

```
HRCE/
├── Services/
│   └── HRCE/
│       ├── HRCEOptions.cs              # خيارات التكوين
│       ├── IHybridViewEngine.cs        # واجهة المحرك الهجين
│       ├── HybridViewEngine.cs         # تنفيذ المحرك
│       ├── ISvelteRenderer.cs          # واجهة عارض Svelte
│       ├── SvelteRenderer.cs           # تنفيذ العارض
│       ├── IIsolationService.cs        # واجهة خدمة العزل
│       ├── IsolationService.cs         # تنفيذ العزل
│       └── ServiceCollectionExtensions.cs
│
├── Middleware/
│   └── HRCEMiddleware.cs               # معالجة التوجيهات
│
├── Components/
│   └── ProductCard/                    # مثال: مكون بطاقة المنتج
│       ├── ProductCardModel.cs         # Presentation Model
│       ├── ProductCardPresenter.cs     # Presenter
│       └── _ProductCard.cshtml         # Razor Template
│
└── Program.cs                          # تسجيل الخدمات
```

## 🚀 كيفية الاستخدام

### 1. إنشاء مكون جديد

أنشئ مجلد جديد في `Components/` واتبع البنية التالية:

```
Components/
└── YourComponent/
    ├── YourComponentModel.cs
    ├── YourComponentPresenter.cs
    └── _YourComponent.cshtml
```

### 2. Presentation Model

```csharp
namespace HRCE.Components.YourComponent;

public record YourComponentModel(
    string Property1,
    string Property2
);
```

### 3. Presenter

```csharp
namespace HRCE.Components.YourComponent;

public class YourComponentPresenter
{
    public YourComponentModel Map(object source)
    {
        return new YourComponentModel(
            Property1: "Value 1",
            Property2: "Value 2"
        );
    }
}
```

### 4. Razor Template

```razor
@using HRCE.Components.YourComponent
@model YourComponentModel

<div class="your-component">
    <h3>@Model.Property1</h3>
    <p>@Model.Property2</p>
</div>

<style>
    .your-component {
        /* أنماط معزولة */
    }
</style>
```

### 5. الاستخدام في الصفحة

```razor
@using HRCE.Components.YourComponent

@{
    var presenter = new YourComponentPresenter();
    var model = presenter.Map(yourData);
}

@await Html.PartialAsync("~/Components/YourComponent/_YourComponent.cshtml", model)
```

## ✨ الميزات المتاحة

### ✅ ما يعمل الآن:
- ✅ **مكونات معزولة** - CSS/JS لكل مكون منفصل
- ✅ **Presentation Models** - بيانات نظيفة للعرض
- ✅ **Presenters** - تحويل البيانات بشكل منظم
- ✅ **Razor Templates** - دعم كامل لـ Razor
- ✅ **Middleware** - معالجة تلقائية للمكونات

### 🔜 قريباً:
- 🔜 **Svelte SSR** - عرض Svelte من جانب الخادم
- 🔜 **Authorization** - سياسات الأمان على مستوى المكونات
- 🔜 **Hot Reload** - إعادة تحميل سريع أثناء التطوير

## 🧪 اختبار المشروع

1. شغّل المشروع:
   ```bash
   dotnet run
   ```

2. افتح المتصفح على: `https://localhost:5001`

3. ستشاهد صفحة العرض التجريبية مع مكونات ProductCard

## 📖 التكوين

يمكنك تخصيص HRCE في `Program.cs`:

```csharp
builder.Services.AddHRCE(options =>
{
    options.EnableSvelteSSR = true;              // تفعيل Svelte SSR
    options.ComponentBasePath = "~/Components";   // مسار المكونات
    options.IsolationMode = IsolationMode.Full;  // وضع العزل
    options.DefaultRenderer = RendererType.Svelte;
    options.EnableHotReload = true;              // إعادة التحميل السريع
});
```

## 🎯 أفضل الممارسات

1. **افصل المنطق عن العرض**
   - استخدم Presenters لتحويل البيانات
   - لا تضع منطق أعمال في Razor

2. **استخدم Immutable Models**
   - استخدم `record` بدلاً من `class`
   - تجنب تعديل البيانات بعد الإنشاء

3. **عزل الأنماط**
   - ضع CSS داخل ملف `.cshtml`
   - استخدم أسماء فريدة للـ classes

4. **اختبر المكونات**
   - اختبر Presenters بشكل منفصل
   - استخدم `data-testid` للاختبار

## 🐛 حل المشكلات

### المكون لا يظهر؟
- تأكد من المسار الصحيح في `Html.PartialAsync`
- تحقق من namespace في `@using`

### الأنماط لا تعمل؟
- تأكد من وجود `<style>` داخل `.cshtml`
- تحقق من وضع العزل في الإعدادات

### خطأ في البناء؟
- شغّل `dotnet build`
- تحقق من `using statements`

## 📚 المزيد من الموارد

راجع `ReadMe.md` الأصلي للتوثيق الكامل باللغة العربية.

---

**تم التنفيذ بنجاح! 🎉**
الآن يمكنك البدء ببناء مكونات HRCE الخاصة بك!
