# ========================================
# HRCE Cross-Platform Build Script (PowerShell)
# Builds .NET application on Windows
# ========================================

param(
    [string]$BuildConfig = "Release",
    [switch]$SkipDotnet = $false,
    [switch]$SkipNode = $false
)

# Colors
$ErrorColor = "Red"
$SuccessColor = "Green"
$WarningColor = "Yellow"

Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host "HRCE Cross-Platform Build Script" -ForegroundColor $SuccessColor
Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Platform: Windows" -ForegroundColor $WarningColor
Write-Host "Build Configuration: $BuildConfig" -ForegroundColor $WarningColor
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor $WarningColor

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Error: dotnet is not installed" -ForegroundColor $ErrorColor
    exit 1
}

if (-not $SkipNode) {
    if (-not (Get-Command node -ErrorAction SilentlyContinue) -or 
        -not (Get-Command npm -ErrorAction SilentlyContinue)) {
        Write-Host "Error: Node.js is not installed" -ForegroundColor $ErrorColor
        exit 1
    }
}

Write-Host "✓ All prerequisites met" -ForegroundColor $SuccessColor
Write-Host ""

# Build Node.js application
if (-not $SkipNode) {
    Write-Host "Building Node.js application..." -ForegroundColor $WarningColor
    
    Push-Location Node
    
    # Install dependencies
    npm ci
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Node.js build failed" -ForegroundColor $ErrorColor
        Pop-Location
        exit 1
    }
    
    Write-Host "✓ Node.js dependencies installed" -ForegroundColor $SuccessColor
    Pop-Location
    Write-Host ""
} else {
    Write-Host "Skipping Node.js build" -ForegroundColor $WarningColor
    Write-Host ""
}

# Build .NET application
if (-not $SkipDotnet) {
    Write-Host "Building .NET application..." -ForegroundColor $WarningColor
    
    # Restore packages
    dotnet restore
    
    # Build
    dotnet build --configuration $BuildConfig --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ .NET build failed" -ForegroundColor $ErrorColor
        exit 1
    }
    
    Write-Host "✓ .NET application built successfully" -ForegroundColor $SuccessColor
    Write-Host ""
} else {
    Write-Host "Skipping .NET build" -ForegroundColor $WarningColor
    Write-Host ""
}

# Display build information
Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host "Build Summary" -ForegroundColor $SuccessColor
Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host "Platform: Windows" -ForegroundColor $WarningColor
Write-Host "Configuration: $BuildConfig" -ForegroundColor $WarningColor

if (-not $SkipNode) {
    Write-Host "Node.js: Node\node_modules\" -ForegroundColor $WarningColor
}

if (-not $SkipDotnet) {
    Write-Host ".NET Output: bin\$BuildConfig\" -ForegroundColor $WarningColor
}

Write-Host ""
Write-Host "✓ Build completed successfully!" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "NOTE: eBPF is not supported on Windows." -ForegroundColor $WarningColor
Write-Host "For eBPF development, use WSL2 or Linux." -ForegroundColor $WarningColor
Write-Host "Run 'wsl ./build.sh' to build with eBPF support." -ForegroundColor $WarningColor
