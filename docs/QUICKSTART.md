# Quick Start Guide - HRCE

## اختيار بيئة التطوير

### 1️⃣ التطوير على Windows (موصى به للمبتدئين)

إذا كنت تريد فقط تطوير الواجهة الأمامية أو منطق التطبيق:

```powershell
# استنساخ المشروع
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git
cd HRCE

# بناء وتشغيل
.\build.ps1
dotnet run
```

✅ **يعمل بدون eBPF** - سيستخدم NullMonitor

❌ **لا يدعم مراقبة النواة**

---

### 2️⃣ التطوير على WSL2 (الخيار الأمثل)

للحصول على تجربة كاملة مع eBPF على Windows:

```powershell
# تثبيت WSL2
wsl --install -d Ubuntu-24.04

# الانتقال إلى WSL
wsl

# داخل WSL
cd /mnt/x/source/repos/HRCE
./build.sh
sudo dotnet run
```

✅ **دعم كامل لـ eBPF**

✅ **نفس كود المشروع من Windows**

⚠️ **يتطلب صلاحيات sudo**

---

### 3️⃣ التطوير باستخدام Docker (للنشر والاختبار)

```bash
# تطوير مع Hot Reload
docker compose --profile dev up

# إنتاج
docker compose --profile prod up -d
```

✅ **بيئة معزولة**

✅ **دعم كامل لـ eBPF**

❌ **بطء في Hot Reload**

---

## اختبار الميزات

### فحص المنصة

```bash
curl http://localhost:5000/api/monitoring/platform
```

الناتج المتوقع:
```json
{
  "osDescription": "Ubuntu 24.04 LTS",
  "platform": "Linux",
  "monitoringSystem": "EbpfLinux"
}
```

### فحص الإحصائيات

```bash
curl http://localhost:5000/api/monitoring/stats
```

### بث الأحداث

```bash
curl -N http://localhost:5000/api/monitoring/stream
```

---

## استكشاف المشاكل الشائعة

### ❌ `clang: command not found`

```bash
# على WSL/Linux
sudo apt install -y clang llvm
```

### ❌ `Permission denied` عند التشغيل

```bash
# استخدم sudo على Linux/WSL
sudo dotnet run

# أو في Docker
docker run --privileged ...
```

### ❌ `eBPF not working on Windows`

eBPF **لا يعمل على Windows مباشرة**.

**الحلول:**
1. استخدم WSL2 ✅
2. استخدم Docker Desktop ✅
3. استخدم Linux VM
4. تطوير بدون eBPF (سيعمل كل شيء ما عدا مراقبة النواة)

---

## الخطوة التالية

بعد نجاح التشغيل:

1. راجع [CROSS-PLATFORM-DEV.md](CROSS-PLATFORM-DEV.md) للتفاصيل
2. راجع [WSL2-DEVELOPMENT-GUIDE.md](WSL2-DEVELOPMENT-GUIDE.md) لإعداد WSL2
3. راجع [README.md](../README.md) للوثائق الكاملة

---

## الدعم

- 🐛 مشاكل تقنية: [GitHub Issues](https://github.com/sultanaalyami/SvelteHybridMVC/issues)
- 📖 الوثائق: [docs/](.)
- 💬 استفسارات: افتح Issue جديد
