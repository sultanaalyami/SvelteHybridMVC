#!/bin/bash

# ========================================
# HRCE Cross-Platform Build Script
# Builds eBPF programs and .NET application
# ========================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
BUILD_CONFIG="${BUILD_CONFIG:-Release}"
SKIP_EBPF="${SKIP_EBPF:-false}"
SKIP_DOTNET="${SKIP_DOTNET:-false}"
SKIP_NODE="${SKIP_NODE:-false}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}HRCE Cross-Platform Build Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Detect platform
PLATFORM="unknown"
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="linux"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macos"
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    PLATFORM="windows"
fi

echo -e "${YELLOW}Platform:${NC} $PLATFORM"
echo -e "${YELLOW}Build Configuration:${NC} $BUILD_CONFIG"
echo ""

# Function to check command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command_exists dotnet; then
    echo -e "${RED}Error: dotnet is not installed${NC}"
    exit 1
fi

if [[ "$PLATFORM" == "linux" ]] && [[ "$SKIP_EBPF" == "false" ]]; then
    if ! command_exists clang; then
        echo -e "${RED}Error: clang is not installed${NC}"
        exit 1
    fi
fi

if [[ "$SKIP_NODE" == "false" ]]; then
    if ! command_exists node || ! command_exists npm; then
        echo -e "${RED}Error: Node.js is not installed${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}✓ All prerequisites met${NC}"
echo ""

# Build eBPF programs (Linux only)
if [[ "$PLATFORM" == "linux" ]] && [[ "$SKIP_EBPF" == "false" ]]; then
    echo -e "${YELLOW}Building eBPF programs...${NC}"
    
    cd eBPF
    
    # Create output directory
    mkdir -p bin
    
    # Compile monitor.c
    clang -O2 \
        -target bpf \
        -c src/monitor.c \
        -o bin/monitor.o
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ eBPF programs built successfully${NC}"
    else
        echo -e "${RED}✗ eBPF build failed${NC}"
        exit 1
    fi
    
    cd ..
    echo ""
else
    echo -e "${YELLOW}Skipping eBPF build (not on Linux or SKIP_EBPF=true)${NC}"
    echo ""
fi

# Build Node.js application
if [[ "$SKIP_NODE" == "false" ]]; then
    echo -e "${YELLOW}Building Node.js application...${NC}"
    
    cd Node
    
    # Install dependencies
    npm ci
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Node.js dependencies installed${NC}"
    else
        echo -e "${RED}✗ Node.js build failed${NC}"
        exit 1
    fi
    
    cd ..
    echo ""
else
    echo -e "${YELLOW}Skipping Node.js build${NC}"
    echo ""
fi

# Build .NET application
if [[ "$SKIP_DOTNET" == "false" ]]; then
    echo -e "${YELLOW}Building .NET application...${NC}"
    
    # Restore packages
    dotnet restore
    
    # Build
    dotnet build \
        --configuration $BUILD_CONFIG \
        --no-restore
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ .NET application built successfully${NC}"
    else
        echo -e "${RED}✗ .NET build failed${NC}"
        exit 1
    fi
    echo ""
else
    echo -e "${YELLOW}Skipping .NET build${NC}"
    echo ""
fi

# Display build information
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Build Summary${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${YELLOW}Platform:${NC} $PLATFORM"
echo -e "${YELLOW}Configuration:${NC} $BUILD_CONFIG"

if [[ "$PLATFORM" == "linux" ]] && [[ "$SKIP_EBPF" == "false" ]]; then
    echo -e "${YELLOW}eBPF Output:${NC} eBPF/bin/monitor.o"
fi

if [[ "$SKIP_NODE" == "false" ]]; then
    echo -e "${YELLOW}Node.js:${NC} Node/node_modules/"
fi

if [[ "$SKIP_DOTNET" == "false" ]]; then
    echo -e "${YELLOW}.NET Output:${NC} bin/$BUILD_CONFIG/"
fi

echo ""
echo -e "${GREEN}✓ Build completed successfully!${NC}"
