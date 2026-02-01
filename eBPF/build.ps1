# سكريبت بناء نظام eBPF لبيئة Windows
# المتطلبات: Clang, eBPF for Windows SDK

param (
    [string]$SrcDir = "src",
    [string]$OutDir = "bin"
)

$ErrorActionPreference = "Stop"

# إنشاء مجلد المخرجات
if (!(Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

Write-Host "[INFO] Compiling Kernel-Space program (monitor.c)..." -ForegroundColor Cyan

# 1. تجميع كود C إلى BPF Bytecode (ELF)
# استخدام Clang مع target bpf
clang -target bpf -O2 -c "$SrcDir/monitor.c" -o "$OutDir/monitor.o"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Compilation failed."
}

Write-Host "[INFO] Converting to Native Driver (monitor.sys)..." -ForegroundColor Cyan

# 2. تحويل ELF إلى Native PE Driver
# يتطلب أداة Convert-BpfToNative من eBPF for Windows
# هذا الأمر افتراضي ويجب تعديل المسار حسب التثبيت الفعلي
$ConvertTool = "Convert-BpfToNative.ps1"

if (Get-Command $ConvertTool -ErrorAction SilentlyContinue) {
    & $ConvertTool -InputPath "$OutDir/monitor.o" -OutputPath "$OutDir/monitor.sys" -IncludeDir "$SrcDir"
} else {
    Write-Warning "Convert-BpfToNative.ps1 not found. Skipping native conversion."
    Write-Warning "Please ensure eBPF for Windows SDK is installed and in PATH."
}

Write-Host "[INFO] Compiling User-Space application (monitor_user.cpp)..." -ForegroundColor Cyan

# 3. تجميع تطبيق المستخدم
# يتطلب MSVC أو Clang
# مثال باستخدام Clang:
clang++ "$SrcDir/monitor_user.cpp" -o "$OutDir/monitor_user.exe" -lWs2_32

if ($LASTEXITCODE -eq 0) {
    Write-Host "[SUCCESS] Build completed successfully." -ForegroundColor Green
    Write-Host "Artifacts are in: $OutDir"
} else {
    Write-Error "User-space compilation failed."
}
