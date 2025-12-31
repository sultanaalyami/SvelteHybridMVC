# SvelteHybridMVC

نظام هجين يجمع بين قوة **ASP.NET MVC** وتفاعلية **Svelte** - حيث MVC هو المتحكم الرئيسي مع مكونات Svelte تفاعلية.

## 🎯 المميزات
 
 
- **"نمط MVC مهيمن"**: Razor Views تتحكم بالصفحات مع مكونات Svelte للتفاعلية
- **دفق SSR سريع**: عرض على الخادم مع Hydration انتقائي
- **تصميم احترافي**: CSS شبيه بـ Bootstrap بدون اعتماديات خارجية
- **محاذاة RTL جاهز**: دعم كامل للغة العربية
 
## 🚀 التشغيل

```bash
# استعادة الحزم
dotnet restore
npm install

# التشغيل
dotnet run
```

ثم افتح http://localhost:5000

## 📁 هيكل المشروع

```
SvelteHybridMVC/
├── Controllers/          # MVC Controllers
├── Views/               # Razor Views
│   ├── Shared/_Layout.cshtml
│   ├── Home/
│   └── Products/
├── SvelteApp/src/       # مكونات Svelte
├── wwwroot/css/         # الأنماط
├── Core/                # إعدادات النظام الهجين
└── Infrastructure/      # خدمات DI
```

## 🔧 التقنيات

- ASP.NET Core 10
- Svelte 5
- Razor Runtime Compilation
- CSS مخصص (Bootstrap-like)

## 📝 الترخيص

MIT
