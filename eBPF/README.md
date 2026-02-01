# نظام مراقبة الشبكة باستخدام eBPF على Windows

هذا المشروع يقدم تطبيقاً عملياً لاستراتيجيات هندسة eBPF في بيئة Visual Studio Enterprise 2026، كما هو موضح في وثائق التصميم.

## المكونات

1. **monitor.c**: برنامج النواة (Kernel-Space) الذي يعمل داخل بيئة eBPF الآمنة. يقوم باعتراض حزم الشبكة باستخدام `XDP` وتخزين الإحصائيات في `BPF_MAP_TYPE_HASH`.
2. **monitor_user.cpp**: تطبيق المستخدم (User-Space) الذي يقوم بتحميل برنامج النواة وقراءة الإحصائيات دورياً.
3. **build.ps1**: سكريبت PowerShell لأتمتة عملية البناء والتحويل إلى Native Driver.

## المتطلبات المسبقة

- **Visual Studio 2026 Enterprise** (أو إصدار حديث مع أدوات C++).
- **Clang/LLVM**: لتجميع كود BPF.
- **eBPF for Windows SDK**: الإصدار 10.0.28000.1 أو أحدث.
- **Nuget Packages**: تأكد من تثبيت حزم `eBPF-for-Windows`.

## خطوات البناء

1. افتح PowerShell كمسؤول.
2. انتقل إلى مجلد `eBPF`.
3. قم بتشغيل سكريبت البناء:
   ```powershell
   .\build.ps1
   ```

سيقوم السكريبت بالخطوات التالية:
- تجميع `monitor.c` إلى `monitor.o` (ELF).
- تحويل `monitor.o` إلى `monitor.sys` (Native Driver) باستخدام `Convert-BpfToNative`.
- تجميع `monitor_user.cpp` إلى `monitor_user.exe`.

## التشغيل

1. حدد رقم واجهة الشبكة (Interface Index) التي تريد مراقبتها:
   ```powershell
   Get-NetAdapter
   ```
2. قم بتشغيل التطبيق:
   ```powershell
   .\bin\monitor_user.exe <if_index>
   ```

## الهيكلية

- **Hook**: `BPF_PROG_TYPE_XDP` (Ingress Path).
- **Map**: `flow_stats_map` (Hash Map).
- **Key**: Source/Dest IP & Port.
- **Value**: Packets & Bytes counters.

## ملاحظات الأمان

- تم تصميم الكود ليتوافق مع قيود Verifier (حلقات محدودة، وصول آمن للذاكرة).
- يستخدم النموذج التنفيذي Native لضمان التوافق مع HVCI.
