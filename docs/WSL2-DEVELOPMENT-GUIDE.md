# دليل تطوير HRCE على WSL2

## المتطلبات الأساسية

### 1. تثبيت WSL2 على Windows

```powershell
# تفعيل WSL
wsl --install

# تثبيت Ubuntu 24.04
wsl --install -d Ubuntu-24.04

# التحقق من الإصدار
wsl --version
```

### 2. تثبيت أدوات التطوير على Ubuntu

```bash
# تحديث النظام
sudo apt update && sudo apt upgrade -y

# تثبيت أدوات eBPF
sudo apt install -y \
    clang \
    llvm \
    libbpf-dev \
    linux-headers-$(uname -r) \
    build-essential \
    cmake \
    git

# تثبيت .NET 10 SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 10.0

# إضافة .NET إلى PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# التحقق من .NET
dotnet --version

# تثبيت Node.js 22
curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -
sudo apt install -y nodejs

# التحقق من Node.js
node --version
npm --version
```

### 3. تثبيت Docker على WSL2

```bash
# إضافة مستودع Docker
sudo apt-get install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update

# تثبيت Docker
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# إضافة المستخدم إلى مجموعة docker
sudo usermod -aG docker $USER
newgrp docker

# التحقق من Docker
docker --version
docker compose version
```

## إعداد المشروع

### 1. استنساخ المشروع

```bash
# الانتقال إلى مجلد العمل
cd ~

# استنساخ المشروع (أو استخدام المجلد المشترك مع Windows)
# الطريقة 1: من Git
git clone https://github.com/sultanaalyami/SvelteHybridMVC.git HRCE
cd HRCE

# الطريقة 2: الوصول للمشروع من Windows
cd /mnt/x/source/repos/HRCE
```

### 2. بناء برامج eBPF

```bash
# الانتقال إلى مجلد eBPF
cd eBPF

# ترجمة برامج eBPF
clang -O2 -target bpf -c src/monitor.c -o monitor.o

# التحقق من الملف الناتج
file monitor.o
# يجب أن يكون: monitor.o: ELF 64-bit LSB relocatable, eBPF, version 1 (SYSV)

# (اختياري) فحص البرنامج
llvm-objdump -S monitor.o
```

### 3. بناء التطبيق بـ .NET

```bash
# العودة إلى جذر المشروع
cd ..

# استعادة الحزم
dotnet restore

# بناء المشروع
dotnet build

# تشغيل المشروع
dotnet run
```

## استخدام Docker

### 1. بناء صورة التطوير

```bash
# بناء صورة التطوير
docker build --target development -t hrce:dev .

# تشغيل حاوية التطوير
docker run -it --rm \
  --privileged \
  --cap-add=SYS_ADMIN \
  --cap-add=NET_ADMIN \
  -v $(pwd):/src \
  -p 5000:8080 \
  -p 5001:8081 \
  -p 3000:3000 \
  hrce:dev
```

### 2. استخدام Docker Compose

```bash
# تشغيل بيئة التطوير
docker compose --profile dev up

# تشغيل بيئة الإنتاج
docker compose --profile prod up -d

# عرض السجلات
docker compose logs -f hrce-dev

# إيقاف البيئة
docker compose down
```

### 3. بناء صورة الإنتاج

```bash
# بناء صورة الإنتاج
docker build -t hrce:latest .

# تشغيل الحاوية
docker run -d \
  --name hrce-prod \
  --privileged \
  --cap-add=SYS_ADMIN \
  --cap-add=NET_ADMIN \
  -p 8080:8080 \
  -p 8081:8081 \
  -v hrce-data:/app/data \
  hrce:latest
```

## اختبار eBPF

### 1. التحقق من دعم eBPF

```bash
# التحقق من إصدار Kernel
uname -r

# يجب أن يكون >= 5.4

# التحقق من دعم eBPF
sudo sysctl kernel.unprivileged_bpf_disabled
# يجب أن يكون 0 أو 1

# التحقق من BPF filesystem
mount | grep bpf
```

### 2. تشغيل برنامج eBPF اختباري

```bash
# تثبيت bpftool
sudo apt install -y linux-tools-common linux-tools-generic

# عرض برامج eBPF المحملة
sudo bpftool prog list

# عرض BPF maps
sudo bpftool map list
```

### 3. اختبار التطبيق مع eBPF

```bash
# تشغيل التطبيق
dotnet run

# في نافذة طرفية أخرى، التحقق من تحميل برامج eBPF
sudo bpftool prog list | grep monitor

# عرض الأحداث
curl http://localhost:5000/api/monitoring/events
```

## استكشاف الأخطاء

### مشكلة: eBPF لا يعمل

```bash
# التحقق من الصلاحيات
id
# يجب أن يحتوي على docker

# التحقق من privileged mode
docker inspect <container_id> | grep Privileged
# يجب أن يكون true

# التحقق من Kernel logs
sudo dmesg | grep -i bpf
```

### مشكلة: فشل بناء eBPF

```bash
# التحقق من وجود headers
ls -la /usr/src/linux-headers-$(uname -r)/

# إعادة تثبيت headers
sudo apt install --reinstall linux-headers-$(uname -r)

# التحقق من clang
clang --version
# يجب أن يكون >= 10.0
```

### مشكلة: Docker لا يعمل

```bash
# إعادة تشغيل Docker service
sudo service docker restart

# التحقق من الحالة
sudo service docker status

# إعادة تحميل المستخدم للمجموعة
newgrp docker
```

## النشر للإنتاج

### 1. بناء صورة متعددة المنصات

```bash
# إنشاء builder
docker buildx create --name multiplatform --use

# بناء ونشر لمنصات متعددة
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t your-registry/hrce:latest \
  --push \
  .
```

### 2. نشر على Kubernetes

```bash
# تطبيق manifests
kubectl apply -f k8s/

# التحقق من الحالة
kubectl get pods -l app=hrce

# عرض السجلات
kubectl logs -f deployment/hrce
```

## الموارد الإضافية

- [eBPF Documentation](https://ebpf.io/)
- [libbpf Documentation](https://github.com/libbpf/libbpf)
- [WSL2 Documentation](https://docs.microsoft.com/en-us/windows/wsl/)
- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
