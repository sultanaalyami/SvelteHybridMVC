# HRCE - Hybrid Razor Component Engine

<div dir="rtl">

## نظام مراقبة هجين يجمع بين eBPF و .NET 10

نظام متقدم لتتبع تذاكر التنفيذ (Execution Tickets) من مستوى النواة (Kernel Space) إلى واجهة المستخدم، مع دعم متعدد المنصات.

نظام هجين يجمع بين قوة **ASP.NET MVC** وتفاعلية **Svelte** مع مراقبة على مستوى النواة.

</div>

## ✨ المميزات الرئيسية

- 🔬 **مراقبة مستوى النواة**: استخدام eBPF على Linux، ETW على Windows
- 🌐 **متعدد المنصات**: يعمل على Windows، Linux، macOS
- ⚡ **ASP.NET Core 10**: أحدث تقنيات .NET مع C# 14
- 🎨 **Svelte SSR**: واجهة مستخدم تفاعلية مع Server-Side Rendering
- 🐳 **Docker Ready**: صور Docker محسّنة متعددة المراحل
- 📊 **Real-time Monitoring**: بث الأحداث في الوقت الفعلي عبر SSE
- 🧠 **Neural Spine Visualization**: تصور بيانات eBPF كنخاع شوكي رقمي حي
- 🔒 **Security First**: تكامل مع Identity Framework
- **"نمط MVC مهيمن"**: Razor Views تتحكم بالصفحات مع مكونات Svelte للتفاعلية
- **دفق SSR سريع**: عرض على الخادم مع Hydration انتقائي
- **محاذاة RTL جاهز**: دعم كامل للغة العربية

## 🏗️ الهيكل المعماري

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (SvelteKit + Razor Pages)                      │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                  Application Layer                           │
│           (ASP.NET Core 10 + C# 14)                         │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│              Platform Abstraction Layer                      │
│    ┌──────────┬──────────┬──────────┬──────────┐           │
│    │  eBPF    │   ETW    │  DTrace  │   Null   │           │
│    │ (Linux)  │(Windows) │ (macOS)  │(Fallback)│           │
│    └──────────┴──────────┴──────────┴──────────┘           │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    Kernel Space                              │
│              (Network, Syscalls, Events)                    │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 البدء السريع

### التطوير على Windows (بدون eBPF)

```powershell
# استنساخ المشروع
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git
cd HRCE

# استعادة الحزم
dotnet restore
cd Node && npm install && cd ..

# بناء المشروع
.\build.ps1

# تشغيل التطبيق
dotnet run
```

ثم افتح http://localhost:5000

### التطوير على Linux (مع eBPF)

```bash
# استنساخ المشروع
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git
cd HRCE

# تثبيت المتطلبات
sudo apt install -y clang llvm libbpf-dev linux-headers-$(uname -r)

# بناء المشروع
./build.sh

# تشغيل التطبيق
sudo dotnet run
```

### باستخدام Docker

```bash
# بيئة التطوير مع Hot Reload
docker compose --profile dev up

# بيئة الإنتاج
docker compose --profile prod up -d
```

## 📋 المتطلبات

### جميع المنصات
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)

### Linux (لـ eBPF)
- Kernel 5.4 أو أحدث
- clang & llvm
- libbpf-dev
- linux-headers

### Windows (لـ WSL2)
- Windows 10/11 مع WSL2
- Ubuntu 24.04 في WSL

## 📁 هيكل المشروع

```
HRCE/
├── Controllers/               # MVC Controllers
├── Views/                     # Razor Views
│   ├── Shared/_Layout.cshtml
│   ├── Home/
│   └── Products/
├── Core/                      # النواة (Interfaces & Models)
│   ├── Platform/              # خدمات اكتشاف المنصة
│   │   ├── IPlatformDetectionService.cs
│   │   └── PlatformDetectionService.cs
│   └── Monitoring/            # أنظمة المراقبة
│       ├── IKernelMonitor.cs
│       ├── EbpfLinuxMonitor.cs
│       ├── EtwWindowsMonitor.cs
│       └── NullMonitor.cs
├── Infrastructure/            # البنية التحتية
│   └── Extensions/
│       └── MonitoringServiceExtensions.cs
├── eBPF/                      # برامج eBPF (C)
│   ├── src/
│   │   ├── monitor.c
│   │   └── bpf_helpers.h
│   └── bin/
├── Node/                      # خادم Svelte SSR
│   ├── server.js
│   └── package.json
├── SvelteApp/src/             # مكونات Svelte
├── wwwroot/css/               # الأنماط
├── docs/                      # الوثائق
│   ├── CROSS-PLATFORM-DEV.md
│   └── WSL2-DEVELOPMENT-GUIDE.md
├── build.sh                   # سكريبت بناء Linux
├── build.ps1                  # سكريبت بناء Windows
├── deploy.sh                  # سكريبت النشر
├── Dockerfile                 # Dockerfile متعدد المراحل
├── docker-compose.yml         # Docker Compose
└── Program.cs                 # نقطة الدخول
```

## 🛠️ البناء

### سكريبتات البناء

```bash
# Linux/WSL
./build.sh

# Windows
.\build.ps1

# Docker
docker build -t hrce:latest .
```

## 🌐 نقاط النهاية (API Endpoints)

### معلومات المنصة
```
GET /api/platform
```

### إحصائيات المراقبة
```
GET /api/monitoring/stats
```

### بث الأحداث (Server-Sent Events)
```
GET /api/monitoring/events
```

### فحص الصحة
```
GET /health
```

### تصور النخاع الشوكي الرقمي
```
GET /Visualization/NeuralSpine
```

## 📚 الوثائق

- [دليل التطوير متعدد المنصات](docs/CROSS-PLATFORM-DEV.md)
- [دليل تطوير WSL2](docs/WSL2-DEVELOPMENT-GUIDE.md)
- [معمارية تذكرة التنفيذ](docs/architecture/معمارية%20تذكرة%20التنفيذ(Execution%20Ticket%20Tracking).md)
- [🧠 نظام تصور النخاع الشوكي الرقمي](docs/NEURAL-SPINE-VISUALIZATION.md) **← جديد!**
- [🔧 التكامل التقني للتصور](docs/NEURAL-SPINE-TECHNICAL.md)
- [⚡ دليل البدء السريع للتصور](VISUALIZATION-QUICKSTART.md)

## 📊 مصفوفة التوافقية

| المكون | Windows | Linux | macOS | ملاحظات |
|--------|---------|-------|-------|---------|
| ASP.NET Core 10 | ✅ | ✅ | ✅ | دعم كامل |
| eBPF | ❌ | ✅ | ❌ | Linux فقط (Kernel 5.4+) |
| ETW | ✅ | ❌ | ❌ | Windows فقط |
| Docker | ✅ | ✅ | ✅ | يحتاج Linux لـ eBPF |
| SvelteKit | ✅ | ✅ | ✅ | يعمل في المتصفح |
| SQLite | ✅ | ✅ | ✅ | قاعدة بيانات محمولة |

## 🐳 Docker

### تشغيل مع Docker Compose

```bash
# تطوير
docker compose --profile dev up

# إنتاج
docker compose --profile prod up -d

# مع SQL Server
docker compose --profile prod --profile sqlserver up -d

# مع Monitoring Dashboard
docker compose --profile prod --profile monitoring up -d
```

## 🚢 النشر

### Docker

```bash
DEPLOY_TARGET=docker ./deploy.sh
```

### Kubernetes

```bash
DEPLOY_TARGET=kubernetes \
  REGISTRY=your-registry.io \
  IMAGE_TAG=v1.0.0 \
  ./deploy.sh
```

## 🔧 التقنيات

- ASP.NET Core 10
- C# 14 (.NET 10)
- Svelte 5
- eBPF (Linux Kernel)
- ETW (Windows)
- Docker & Kubernetes
- Razor Runtime Compilation
- CSS مخصص (Bootstrap-like)

## 🤝 المساهمة

نرحب بالمساهمات! يرجى:

1. Fork المشروع
2. إنشاء فرع للميزة (`git checkout -b feature/AmazingFeature`)
3. Commit التغييرات (`git commit -m 'Add some AmazingFeature'`)
4. Push للفرع (`git push origin feature/AmazingFeature`)
5. فتح Pull Request

## 📝 الترخيص

هذا المشروع مرخص تحت MIT License

## 👥 المؤلفون

- Sultan Alyami - [@sultanaalyami](https://github.com/sultanaalyami)

## 🙏 شكر وتقدير

- [eBPF](https://ebpf.io/) - للمراقبة على مستوى النواة
- [.NET Team](https://github.com/dotnet) - لـ ASP.NET Core 10
- [Svelte](https://svelte.dev/) - لإطار العمل التفاعلي

---

<div align="center">

**صُنع بـ ❤️ باستخدام .NET 10 و eBPF**

[الوثائق](docs/) • [GitHub](https://github.com/sultanaalyami/SvelteHybridMVC)

</div>
