# SvelteHybridMVC

نظام هجين يجمع بين قوة **ASP.NET MVC** وتفاعلية **Svelte** - حيث MVC هو المتحكم الرئيسي مع مكونات Svelte تفاعلية.

## 🎯 المميزات

- **MVC مهيمن**: Razor Views تتحكم بالصفحات مع مكونات Svelte للتفاعلية
- **SSR سريع**: عرض على الخادم مع Hydration انتقائي
- **تصميم احترافي**: CSS شبيه بـ Bootstrap بدون اعتماديات خارجية
- **RTL جاهز**: دعم كامل للغة العربية
- **🔒 أمان محسّن**: حماية شاملة ضد XSS، CSRF، وأكثر

## 🛡️ الأمان

تم فحص النظام بشكل شامل وإصلاح جميع الثغرات الأمنية:
- ✅ حماية من XSS (Cross-Site Scripting)
- ✅ حماية من CSRF (Cross-Site Request Forgery)
- ✅ التحقق من صحة المدخلات
- ✅ رؤوس أمان HTTP شاملة
- ✅ إجبار HTTPS و HSTS
- ✅ معالجة آمنة للأخطاء

لمزيد من التفاصيل، راجع [تقرير الأمان](SECURITY_AUDIT.md).

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
