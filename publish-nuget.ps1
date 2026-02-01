# SvelteHybrid NuGet Publisher for Windows
# ========================================

param(
    [Parameter()]
    [ValidateSet("nuget", "github", "local")]
    [string]$Target = "local",
    
    [Parameter()]
    [string]$ApiKey
)

$PackagePath = "packages\SvelteHybrid.AspNetCore.1.0.0.nupkg"
$SymbolsPath = "packages\SvelteHybrid.AspNetCore.1.0.0.snupkg"

Write-Host "========================================" -ForegroundColor Green
Write-Host "SvelteHybrid NuGet Publisher" -ForegroundColor Green  
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Check package exists
if (-not (Test-Path $PackagePath)) {
    Write-Host "Error: Package not found at $PackagePath" -ForegroundColor Red
    Write-Host "Run 'dotnet pack -c Release -o packages' first" -ForegroundColor Yellow
    exit 1
}

Write-Host "Package found: $PackagePath" -ForegroundColor Cyan
Write-Host ""

switch ($Target) {
    "nuget" {
        Write-Host "Publishing to NuGet.org..." -ForegroundColor Yellow
        
        if (-not $ApiKey) {
            $ApiKey = $env:NUGET_API_KEY
        }
        
        if (-not $ApiKey) {
            Write-Host "Error: API Key required" -ForegroundColor Red
            Write-Host "Get your key from: https://www.nuget.org/account/apikeys" -ForegroundColor Yellow
            $ApiKey = Read-Host "Enter NuGet API Key"
        }
        
        dotnet nuget push $PackagePath `
            --api-key $ApiKey `
            --source "https://api.nuget.org/v3/index.json" `
            --skip-duplicate
        
        Write-Host "Published to NuGet.org" -ForegroundColor Green
        Write-Host "View at: https://www.nuget.org/packages/SvelteHybrid.AspNetCore/" -ForegroundColor Cyan
    }
    
    "github" {
        Write-Host "Publishing to GitHub Packages..." -ForegroundColor Yellow
        
        if (-not $ApiKey) {
            $ApiKey = $env:GITHUB_TOKEN
        }
        
        if (-not $ApiKey) {
            Write-Host "Error: GitHub Token required" -ForegroundColor Red
            Write-Host "Create at: https://github.com/settings/tokens" -ForegroundColor Yellow
            Write-Host "Required scopes: write:packages, read:packages" -ForegroundColor Yellow
            $ApiKey = Read-Host "Enter GitHub Token"
        }
        
        # Add source if needed
        try {
            dotnet nuget add source `
                --username sultanaalyami `
                --password $ApiKey `
                --store-password-in-clear-text `
                --name github `
                "https://nuget.pkg.github.com/sultanaalyami/index.json" 2>$null
        } catch {}
        
        dotnet nuget push $PackagePath `
            --api-key $ApiKey `
            --source "github" `
            --skip-duplicate
        
        Write-Host "Published to GitHub Packages" -ForegroundColor Green
        Write-Host "View at: https://github.com/sultanaalyami/SvelteHybridMVC/packages" -ForegroundColor Cyan
    }
    
    "local" {
        Write-Host "Creating local feed..." -ForegroundColor Yellow
        
        $LocalFeed = ".\local-nuget-feed"
        New-Item -ItemType Directory -Force -Path $LocalFeed | Out-Null
        
        Copy-Item $PackagePath $LocalFeed
        if (Test-Path $SymbolsPath) {
            Copy-Item $SymbolsPath $LocalFeed
        }
        
        # Add local source
        try {
            dotnet nuget add source $LocalFeed --name local-feed 2>$null
        } catch {}
        
        Write-Host "Package copied to local feed" -ForegroundColor Green
        Write-Host "Install with: dotnet add package SvelteHybrid.AspNetCore --source local-feed" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Done!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
