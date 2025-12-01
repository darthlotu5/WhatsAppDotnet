#!/usr/bin/env pwsh
# WhatsApp.Client NuGet Publishing Script

param(
    [string]$ApiKey = "",
    [string]$Version = "",
    [string]$Configuration = "Release",
    [switch]$DryRun = $false,
    [switch]$Help = $false
)

if ($Help) {
    Write-Host "WhatsApp.Client NuGet Publishing Script" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: ./publish-nuget.ps1 [options]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -ApiKey <key>        NuGet API key (required for publishing)"
    Write-Host "  -Version <version>   Package version (if not specified, reads from .csproj)"
    Write-Host "  -Configuration <cfg> Build configuration (default: Release)"
    Write-Host "  -DryRun             Build and pack without publishing"
    Write-Host "  -Help               Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  ./publish-nuget.ps1 -DryRun                    # Test build and pack"
    Write-Host "  ./publish-nuget.ps1 -ApiKey your-key           # Build and publish"
    Write-Host "  ./publish-nuget.ps1 -ApiKey your-key -Version 1.0.1  # Specify version"
    exit 0
}

Write-Host "🚀 WhatsApp.Client NuGet Publishing Script" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Check if we're in the right directory
if (!(Test-Path "src/WhatsApp.Client/WhatsApp.Client.csproj")) {
    Write-Error "❌ WhatsApp.Client.csproj not found. Please run this script from the project root directory."
    exit 1
}

# Validate API key for publishing
if (!$DryRun -and [string]::IsNullOrWhiteSpace($ApiKey)) {
    Write-Error "❌ NuGet API key is required for publishing. Use -ApiKey parameter or -DryRun for testing."
    exit 1
}

try {
    # Clean previous builds
    Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }

    # Restore dependencies
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Build the project
    Write-Host "🔨 Building project ($Configuration)..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # Run tests (if any exist)
    if (Get-ChildItem -Path . -Filter "*.Tests.csproj" -Recurse) {
        Write-Host "🧪 Running tests..." -ForegroundColor Yellow
        dotnet test --configuration $Configuration --no-build --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    }

    # Pack the NuGet package
    Write-Host "📦 Creating NuGet package..." -ForegroundColor Yellow
    $packArgs = @(
        "pack"
        "--configuration", $Configuration
        "--no-build"
        "--output", "./nupkg"
        "--verbosity", "minimal"
    )
    
    if (![string]::IsNullOrWhiteSpace($Version)) {
        $packArgs += "/p:Version=$Version"
    }
    
    dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }

    # Get the created package file
    $nupkgFiles = Get-ChildItem -Path "./nupkg" -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending
    if ($nupkgFiles.Count -eq 0) {
        throw "No .nupkg file found in ./nupkg directory"
    }
    
    $packageFile = $nupkgFiles[0]
    Write-Host "✅ Package created: $($packageFile.Name)" -ForegroundColor Green

    # Show package contents
    Write-Host "📋 Package contents:" -ForegroundColor Cyan
    dotnet nuget list package $packageFile.FullName

    if ($DryRun) {
        Write-Host "🔍 Dry run completed successfully!" -ForegroundColor Green
        Write-Host "   Package file: $($packageFile.FullName)"
        Write-Host "   To publish, run without -DryRun and provide -ApiKey"
    } else {
        # Publish to NuGet
        Write-Host "🚀 Publishing to NuGet.org..." -ForegroundColor Yellow
        dotnet nuget push $packageFile.FullName --api-key $ApiKey --source https://api.nuget.org/v3/index.json --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
        
        Write-Host "🎉 Successfully published to NuGet.org!" -ForegroundColor Green
        Write-Host "   Package: $($packageFile.Name)"
        Write-Host "   It may take a few minutes to appear in search results."
    }

} catch {
    Write-Error "❌ Error: $_"
    exit 1
}

Write-Host ""
Write-Host "✨ Operation completed successfully!" -ForegroundColor Green
