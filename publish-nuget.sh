#!/bin/bash
# ========================================
# SvelteHybrid NuGet Package Publisher
# ========================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

PACKAGE_PATH="packages/SvelteHybrid.AspNetCore.1.0.0.nupkg"
SYMBOLS_PATH="packages/SvelteHybrid.AspNetCore.1.0.0.snupkg"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}SvelteHybrid NuGet Publisher${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if package exists
if [ ! -f "$PACKAGE_PATH" ]; then
    echo -e "${RED}Error: Package not found at $PACKAGE_PATH${NC}"
    echo -e "${YELLOW}Run 'dotnet pack -c Release -o packages' first${NC}"
    exit 1
fi

echo -e "${BLUE}Package found: $PACKAGE_PATH${NC}"
echo ""

# Choose target
echo -e "${YELLOW}Select publish target:${NC}"
echo "1) NuGet.org (Public)"
echo "2) GitHub Packages"
echo "3) Local feed"
read -p "Enter choice [1-3]: " choice

case $choice in
    1)
        echo ""
        echo -e "${YELLOW}Publishing to NuGet.org...${NC}"
        
        if [ -z "$NUGET_API_KEY" ]; then
            echo -e "${RED}Error: NUGET_API_KEY environment variable not set${NC}"
            echo -e "${YELLOW}Get your API key from: https://www.nuget.org/account/apikeys${NC}"
            echo ""
            read -p "Enter NuGet API Key: " NUGET_API_KEY
        fi
        
        dotnet nuget push "$PACKAGE_PATH" \
            --api-key "$NUGET_API_KEY" \
            --source "https://api.nuget.org/v3/index.json" \
            --skip-duplicate
        
        echo -e "${GREEN}? Published to NuGet.org${NC}"
        echo -e "${BLUE}View at: https://www.nuget.org/packages/SvelteHybrid.AspNetCore/${NC}"
        ;;
        
    2)
        echo ""
        echo -e "${YELLOW}Publishing to GitHub Packages...${NC}"
        
        if [ -z "$GITHUB_TOKEN" ]; then
            echo -e "${RED}Error: GITHUB_TOKEN environment variable not set${NC}"
            echo -e "${YELLOW}Create a token at: https://github.com/settings/tokens${NC}"
            echo -e "${YELLOW}Required scopes: write:packages, read:packages${NC}"
            echo ""
            read -p "Enter GitHub Token: " GITHUB_TOKEN
        fi
        
        # Configure source if needed
        dotnet nuget add source \
            --username sultanaalyami \
            --password "$GITHUB_TOKEN" \
            --store-password-in-clear-text \
            --name github \
            "https://nuget.pkg.github.com/sultanaalyami/index.json" 2>/dev/null || true
        
        dotnet nuget push "$PACKAGE_PATH" \
            --api-key "$GITHUB_TOKEN" \
            --source "github" \
            --skip-duplicate
        
        echo -e "${GREEN}? Published to GitHub Packages${NC}"
        echo -e "${BLUE}View at: https://github.com/sultanaalyami/SvelteHybridMVC/packages${NC}"
        ;;
        
    3)
        echo ""
        echo -e "${YELLOW}Creating local feed...${NC}"
        
        LOCAL_FEED="./local-nuget-feed"
        mkdir -p "$LOCAL_FEED"
        
        cp "$PACKAGE_PATH" "$LOCAL_FEED/"
        cp "$SYMBOLS_PATH" "$LOCAL_FEED/" 2>/dev/null || true
        
        # Add local source
        dotnet nuget add source "$LOCAL_FEED" --name local-feed 2>/dev/null || true
        
        echo -e "${GREEN}? Package copied to local feed${NC}"
        echo -e "${BLUE}Install with: dotnet add package SvelteHybrid.AspNetCore --source local-feed${NC}"
        ;;
        
    *)
        echo -e "${RED}Invalid choice${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}? Done!${NC}"
echo -e "${GREEN}========================================${NC}"
