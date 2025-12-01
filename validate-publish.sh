#!/bin/bash

# Pre-publish validation script for WhatsApp.Client
# This script checks that everything is ready for publishing

set -e

echo "WhatsApp.Client Pre-Publish Validation"
echo "======================================"

# Check if we're in the right directory
if [ ! -f "src/WhatsApp.Client/WhatsApp.Client.csproj" ]; then
    echo "❌ Error: Run this script from the project root directory"
    exit 1
fi

echo "✓ Project structure is correct"

# Check .NET version
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK not found"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ .NET SDK version: $DOTNET_VERSION"

# Check project builds successfully
echo "🔨 Building project..."
cd src/WhatsApp.Client
if dotnet build --configuration Release --verbosity quiet; then
    echo "✓ Project builds successfully"
else
    echo "❌ Error: Project build failed"
    exit 1
fi

# Check project version and validate semantic versioning
PROJECT_VERSION=$(grep -o '<Version>.*</Version>' WhatsApp.Client.csproj | sed 's/<Version>\(.*\)<\/Version>/\1/')
if [ -n "$PROJECT_VERSION" ]; then
    echo "✓ Project version: $PROJECT_VERSION"
    
    # Validate semantic versioning format
    if [[ $PROJECT_VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9\.\-]+)?(\+[a-zA-Z0-9\.\-]+)?$ ]]; then
        echo "✓ Valid semantic version format"
        
        # Check version components
        IFS='.' read -r MAJOR MINOR PATCH_FULL <<< "$PROJECT_VERSION"
        PATCH=$(echo "$PATCH_FULL" | cut -d'-' -f1 | cut -d'+' -f1)
        
        echo "  └─ Major: $MAJOR, Minor: $MINOR, Patch: $PATCH"
        
        # Check for pre-release
        if [[ $PROJECT_VERSION == *"-"* ]]; then
            PRERELEASE=$(echo "$PROJECT_VERSION" | cut -d'-' -f2 | cut -d'+' -f1)
            echo "  └─ Pre-release: $PRERELEASE"
        fi
        
        # Check for build metadata
        if [[ $PROJECT_VERSION == *"+"* ]]; then
            BUILD=$(echo "$PROJECT_VERSION" | cut -d'+' -f2)
            echo "  └─ Build metadata: $BUILD"
        fi
    else
        echo "⚠️  Warning: Version does not follow semantic versioning (semver) format"
        echo "   Expected: MAJOR.MINOR.PATCH[-prerelease][+build]"
        echo "   Examples: 1.0.0, 1.2.3-alpha, 2.0.0-beta.1, 1.0.0+20231201"
    fi
else
    echo "⚠️  Warning: No version specified in project file"
fi

# Check if NuGet package can be created
echo "📦 Creating test NuGet package..."
if dotnet pack --configuration Release --output ../../nupkg --verbosity quiet; then
    echo "✓ NuGet package created successfully"
    PACKAGE_FILE=$(ls ../../nupkg/WhatsApp.Client.*.nupkg 2>/dev/null | head -1)
    if [ -n "$PACKAGE_FILE" ]; then
        echo "✓ Package file: $(basename "$PACKAGE_FILE")"
        PACKAGE_SIZE=$(stat -f%z "$PACKAGE_FILE" 2>/dev/null || stat -c%s "$PACKAGE_FILE" 2>/dev/null)
        echo "✓ Package size: $((PACKAGE_SIZE / 1024)) KB"
    fi
else
    echo "❌ Error: Failed to create NuGet package"
    exit 1
fi

cd ../..

# Check Git status
if command -v git &> /dev/null; then
    if git rev-parse --git-dir > /dev/null 2>&1; then
        if [ -n "$(git status --porcelain)" ]; then
            echo "⚠️  Warning: You have uncommitted changes"
            git status --short
        else
            echo "✓ Git working directory is clean"
        fi
        
        CURRENT_BRANCH=$(git branch --show-current)
        echo "✓ Current branch: $CURRENT_BRANCH"
        
        # Check if there are any tags
        if git tag -l | grep -q "v"; then
            LATEST_TAG=$(git tag -l "v*" | sort -V | tail -1)
            echo "✓ Latest tag: $LATEST_TAG"
        else
            echo "ℹ️  No version tags found (this might be the first release)"
        fi
    else
        echo "⚠️  Warning: Not in a Git repository"
    fi
fi

echo ""
echo "Pre-publish validation completed!"
echo ""
echo "Next steps for publishing:"
echo "1. Commit any pending changes"
echo "2. Update version in WhatsApp.Client.csproj if needed (use semantic versioning)"
if [ -n "$PROJECT_VERSION" ]; then
    echo "3. Create and push a semantic version tag: git tag v$PROJECT_VERSION && git push origin v$PROJECT_VERSION"
else
    echo "3. Create and push a semantic version tag: git tag v1.0.0 && git push origin v1.0.0"
fi
echo "4. GitHub Actions will automatically build and publish to NuGet"
echo ""
echo "Semantic versioning examples:"
echo "  • v1.0.0        - Initial release"
echo "  • v1.0.1        - Patch release (bug fixes)"
echo "  • v1.1.0        - Minor release (new features, backward compatible)"
echo "  • v2.0.0        - Major release (breaking changes)"
echo "  • v1.0.0-alpha  - Pre-release"
echo "  • v1.0.0-beta.1 - Pre-release with additional identifier"
echo ""
echo "For manual publishing, run: ./publish-nuget.sh --api-key YOUR_NUGET_KEY"
