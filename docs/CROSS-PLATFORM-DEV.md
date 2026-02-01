# دليل التطوير متعدد المنصات - HRCE

## نظرة عامة

يدعم HRCE (Hybrid Razor Component Engine) التطوير على منصات متعددة مع تكامل eBPF للمراقبة على مستوى النواة.

### مصفوفة التوافقية

| المكون | Windows | Linux | macOS | ملاحظات |
|--------|---------|-------|-------|---------|
| ASP.NET Core 10 | ✅ | ✅ | ✅ | دعم كامل |
| eBPF (النواة) | ❌ | ✅ | ❌ | Linux فقط (Kernel 5.4+) |
| ETW (Windows) | ✅ | ❌ | ❌ | Windows فقط |
| DTrace | ❌ | ❌ | ⚠️ | macOS (غير مُطبّق بعد) |
| SSE / WebSocket | ✅ | ✅ | ✅ | يعمل في كل مكان |
| SQLite | ✅ | ✅ | ✅ | قاعدة بيانات محمولة |
| SvelteKit | ✅ | ✅ | ✅ | يعمل في المتصفح |

## البدء السريع

### 1. التطوير على Windows (بدون eBPF)

```powershell
# استنساخ المشروع
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git
cd HRCE

# بناء المشروع
.\build.ps1

# تشغيل التطبيق
dotnet run
```

التطبيق سيعمل بدون مراقبة النواة (NullMonitor).

### 2. التطوير على Windows مع WSL2 (مع eBPF)

```powershell
# تثبيت WSL2
wsl --install -d Ubuntu-24.04

# الانتقال إلى WSL
wsl

# في WSL: بناء المشروع مع eBPF
./build.sh

# تشغيل التطبيق
dotnet run
```

### 3. التطوير على Linux (Native eBPF)

```bash
# استنساخ المشروع
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git
cd HRCE

# تثبيت المتطلبات
sudo apt install -y clang llvm libbpf-dev linux-headers-$(uname -r)

# بناء المشروع
./build.sh

# تشغيل التطبيق (يتطلب صلاحيات root لـ eBPF)
sudo dotnet run
```

### 4. التطوير باستخدام Docker

```bash
# بناء وتشغيل بيئة التطوير
docker compose --profile dev up

# أو بناء صورة الإنتاج
docker build -t hrce:latest .
docker run -d --privileged -p 8080:8080 hrce:latest
```

## هيكل المشروع

```
HRCE/
├── Core/                          # النواة (Interfaces & Models)
│   ├── Platform/                  # خدمات اكتشاف المنصة
│   │   ├── IPlatformDetectionService.cs
│   │   └── PlatformDetectionService.cs
│   └── Monitoring/                # أنظمة المراقبة
│       ├── IKernelMonitor.cs      # واجهة موحدة
│       ├── EbpfLinuxMonitor.cs    # تطبيق eBPF
│       ├── EtwWindowsMonitor.cs   # تطبيق ETW
│       └── NullMonitor.cs         # Fallback
├── Infrastructure/                # البنية التحتية
│   └── Extensions/
│       └── MonitoringServiceExtensions.cs
├── eBPF/                          # برامج eBPF (C)
│   ├── src/
│   │   ├── monitor.c              # برنامج eBPF الرئيسي
│   │   ├── bpf_helpers.h
│   │   └── monitor_user.cpp       # تطبيق Userspace
│   ├── bin/                       # ملفات مُترجمة (.o)
│   └── build.ps1                  # سكريبت البناء
├── Node/                          # خادم Svelte SSR
│   ├── server.js
│   ├── package.json
│   └── Dockerfile
├── docs/                          # الوثائق
│   ├── WSL2-DEVELOPMENT-GUIDE.md
│   └── CROSS-PLATFORM-DEV.md (هذا الملف)
├── build.sh                       # سكريبت بناء Linux/WSL
├── build.ps1                      # سكريبت بناء Windows
├── deploy.sh                      # سكريبت النشر
├── Dockerfile                     # Dockerfile متعدد المراحل
├── docker-compose.yml             # Docker Compose
└── Program.cs                     # نقطة الدخول
```

## سيناريوهات التطوير

### السيناريو 1: تطوير الواجهة الأمامية (Frontend)

**المنصة المناسبة:** أي نظام (Windows/Linux/macOS)

```bash
# تشغيل التطبيق بدون مراقبة النواة
dotnet run

# في نافذة أخرى: تطوير Svelte
cd Node
npm run dev
```

**لا حاجة لـ eBPF** - يمكن العمل على Windows مباشرة.

### السيناريو 2: تطوير المنطق (Backend Logic)

**المنصة المناسبة:** أي نظام

```bash
# تشغيل مع Hot Reload
dotnet watch run
```

**لا حاجة لـ eBPF** - المراقبة ستستخدم NullMonitor.

### السيناريو 3: تطوير مراقبة النواة (Kernel Monitoring)

**المنصة المناسبة:** Linux أو WSL2

```bash
# في WSL2 أو Linux
cd eBPF

# تعديل monitor.c
vim src/monitor.c

# إعادة الترجمة
clang -O2 -target bpf -c src/monitor.c -o bin/monitor.o

# إعادة تشغيل التطبيق
sudo dotnet run
```

**ضروري استخدام Linux** - eBPF لا يعمل على Windows.

### السيناريو 4: الاختبار الكامل

**المنصة المناسبة:** Docker

```bash
# بناء صورة الإنتاج
docker build -t hrce:test .

# تشغيل مع eBPF
docker run -it --privileged \
  --cap-add=SYS_ADMIN \
  --cap-add=NET_ADMIN \
  -p 8080:8080 \
  hrce:test
```

## واجهات برمجة التطبيقات (APIs)

### معلومات المنصة

```bash
curl http://localhost:5000/api/platform
```

```json
{
  "osDescription": "Ubuntu 24.04 LTS",
  "runtimeIdentifier": "linux-x64",
  "platform": "Linux",
  "architecture": "X64",
  "isContainer": true,
  "kernelVersion": "5.15.0-92-generic",
  "monitoringSystem": "EbpfLinux"
}
```

### إحصائيات المراقبة

```bash
curl http://localhost:5000/api/monitoring/stats
```

```json
{
  "eventsProcessed": 12543,
  "eventsDropped": 2,
  "uptime": "02:15:32",
  "eventsPerSecond": 1.54,
  "eventTypeCounts": {
    "network_packet": 8432,
    "syscall_trace": 4111
  }
}
```

### بث الأحداث (SSE)

```bash
# الاستماع للأحداث في الوقت الفعلي
curl -N http://localhost:5000/api/monitoring/events
```

## استكشاف الأخطاء

### المشكلة: eBPF لا يعمل على Windows

**الحل:**
- استخدم WSL2 للتطوير مع eBPF
- أو استخدم Docker مع `--privileged`
- أو قم بالتطوير بدون eBPF (سيستخدم NullMonitor)

```powershell
# تشغيل في WSL2
wsl ./build.sh
wsl sudo dotnet run
```

### المشكلة: Clang غير موجود

**الحل على Linux:**
```bash
sudo apt install -y clang llvm
```

**على WSL2:**
```bash
wsl sudo apt install -y clang llvm
```

### المشكلة: Permission denied عند تشغيل eBPF

**الحل:**
```bash
# تشغيل بصلاحيات root
sudo dotnet run

# أو في Docker
docker run --privileged --cap-add=SYS_ADMIN ...
```

### المشكلة: Kernel headers مفقودة

**الحل:**
```bash
# تثبيت headers للنواة الحالية
sudo apt install -y linux-headers-$(uname -r)

# التحقق
ls /usr/src/linux-headers-$(uname -r)/
```

## أفضل الممارسات

### 1. التطوير على Windows

- استخدم Visual Studio 2026 للتطوير الأساسي
- استخدم WSL2 لاختبار eBPF
- استخدم Docker Desktop مع WSL2 backend

### 2. التطوير على Linux

- استخدم VS Code مع C# DevKit
- قم بتثبيت kernel headers قبل البدء
- استخدم `sudo` عند تشغيل التطبيق مع eBPF

### 3. التطوير باستخدام Docker

- استخدم multi-stage builds لتقليل حجم الصورة
- استخدم `--privileged` فقط في التطوير
- في الإنتاج، استخدم `--cap-add` بدقة

### 4. إدارة الكود

- احتفظ بكود eBPF منفصل في مجلد `eBPF/`
- استخدم Platform Detection لاختيار Monitor المناسب
- لا تضع منطق خاص بالمنصة في Core Logic

## الموارد

- [WSL2 Development Guide](WSL2-DEVELOPMENT-GUIDE.md)
- [eBPF Documentation](https://ebpf.io/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Docker Documentation](https://docs.docker.com/)

## الدعم

لأي مشاكل أو استفسارات:
1. راجع [WSL2-DEVELOPMENT-GUIDE.md](WSL2-DEVELOPMENT-GUIDE.md)
2. تحقق من السجلات: `docker logs hrce-dev`
3. افتح Issue على GitHub
