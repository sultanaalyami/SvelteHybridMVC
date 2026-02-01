# ========================================
# Multi-Platform Dockerfile for HRCE
# Supports: eBPF (Linux), ETW (Windows), Development & Production
# ========================================

# ============ Stage 1: eBPF Build (Linux Kernel Space) ============
FROM ubuntu:24.04 AS ebpf-builder

# Install eBPF development tools
RUN apt-get update && apt-get install -y \
    clang \
    llvm \
    libbpf-dev \
    linux-headers-generic \
    make \
    gcc \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /ebpf

# Copy eBPF source code
COPY eBPF/src/ ./src/
COPY eBPF/build.ps1 ./

# Compile eBPF programs for Linux
RUN clang -O2 -target bpf -c src/monitor.c -o monitor.o

# ============ Stage 2: .NET Build ============
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-builder

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["HRCE.csproj", "."]
RUN dotnet restore "./HRCE.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "./HRCE.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
RUN dotnet publish "./HRCE.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ============ Stage 3: Node.js Build (for Svelte SSR) ============
FROM node:22-alpine AS node-builder

WORKDIR /node

# Copy package files
COPY Node/package*.json ./

# Install dependencies
RUN npm ci --production

# Copy Node.js server code
COPY Node/ ./

# ============ Stage 4: Runtime (Production) ============
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Install runtime dependencies for eBPF
RUN apt-get update && apt-get install -y \
    libbpf0 \
    && rm -rf /var/lib/apt/lists/*

# Install Node.js runtime for Svelte SSR
COPY --from=node:22-alpine /usr/lib /usr/lib
COPY --from=node:22-alpine /usr/local/lib /usr/local/lib
COPY --from=node:22-alpine /usr/local/include /usr/local/include
COPY --from=node:22-alpine /usr/local/bin /usr/local/bin

WORKDIR /app

# Copy compiled eBPF programs
COPY --from=ebpf-builder /ebpf/monitor.o ./eBPF/

# Copy .NET application
COPY --from=dotnet-builder /app/publish .

# Copy Node.js application
COPY --from=node-builder /node ./Node

# Create directory for runtime platform detection
RUN mkdir -p /app/runtime

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV EBPF_ENABLED=true

# Expose ports
EXPOSE 8080 8081 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HRCE.dll"]

# ============ Stage 5: Development Environment ============
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development

# Install development tools
RUN apt-get update && apt-get install -y \
    clang \
    llvm \
    libbpf-dev \
    linux-headers-generic \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install Node.js for development
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs

WORKDIR /src

# Copy everything for development
COPY . .

# Restore dependencies
RUN dotnet restore

# Install Node.js dependencies
WORKDIR /src/Node
RUN npm install

WORKDIR /src

# Development entry point with hot reload
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["dotnet", "watch", "run", "--no-restore"]