#!/bin/bash

# WhatsApp.Client NuGet Publishing Script for macOS/Linux

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
CONFIGURATION="Release"
DRY_RUN=false
API_KEY=""
VERSION=""

# Function to print usage
show_help() {
    echo -e "${GREEN}WhatsApp.Client NuGet Publishing Script${NC}"
    echo ""
    echo "Usage: ./publish-nuget.sh [options]"
    echo ""
    echo "Options:"
    echo "  -k, --api-key <key>        NuGet API key (required for publishing)"
    echo "  -v, --version <version>    Package version (if not specified, reads from .csproj)"
    echo "  -c, --configuration <cfg>  Build configuration (default: Release)"
    echo "  -d, --dry-run             Build and pack without publishing"
    echo "  -h, --help                Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./publish-nuget.sh --dry-run                    # Test build and pack"
    echo "  ./publish-nuget.sh --api-key your-key           # Build and publish"
    echo "  ./publish-nuget.sh -k your-key -v 1.0.1         # Specify version"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -k|--api-key)
            API_KEY="$2"
            shift 2
            ;;
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -d|--dry-run)
            DRY_RUN=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo -e "${RED}❌ Unknown option: $1${NC}"
            show_help
            exit 1
            ;;
    esac
done

echo -e "${GREEN}🚀 WhatsApp.Client NuGet Publishing Script${NC}"
echo -e "${GREEN}=========================================${NC}"

# Check if we're in the right directory
if [[ ! -f "src/WhatsApp.Client/WhatsApp.Client.csproj" ]]; then
    echo -e "${RED}❌ WhatsApp.Client.csproj not found. Please run this script from the project root directory.${NC}"
    exit 1
fi

# Validate API key for publishing
if [[ "$DRY_RUN" == "false" && -z "$API_KEY" ]]; then
    echo -e "${RED}❌ NuGet API key is required for publishing. Use --api-key parameter or --dry-run for testing.${NC}"
    exit 1
fi

# Function to check command success
check_result() {
    if [[ $? -ne 0 ]]; then
        echo -e "${RED}❌ Error: $1 failed${NC}"
        exit 1
    fi
}

# Create nupkg directory if it doesn't exist
mkdir -p ./nupkg

# Clean previous builds
echo -e "${YELLOW}🧹 Cleaning previous builds...${NC}"
dotnet clean --configuration "$CONFIGURATION" --verbosity minimal
check_result "Clean"

# Restore dependencies
echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
dotnet restore --verbosity minimal
check_result "Restore"

# Build the project
echo -e "${YELLOW}🔨 Building project ($CONFIGURATION)...${NC}"
dotnet build --configuration "$CONFIGURATION" --no-restore --verbosity minimal
check_result "Build"

# Run tests (if any exist)
if find . -name "*.Tests.csproj" -type f | grep -q .; then
    echo -e "${YELLOW}🧪 Running tests...${NC}"
    dotnet test --configuration "$CONFIGURATION" --no-build --verbosity minimal
    check_result "Tests"
fi

# Pack the NuGet package
echo -e "${YELLOW}📦 Creating NuGet package...${NC}"
pack_args=(
    "pack"
    "--configuration" "$CONFIGURATION"
    "--no-build"
    "--output" "./nupkg"
    "--verbosity" "minimal"
)

if [[ -n "$VERSION" ]]; then
    pack_args+=("/p:Version=$VERSION")
fi

dotnet "${pack_args[@]}"
check_result "Pack"

# Get the created package file
PACKAGE_FILE=$(find ./nupkg -name "*.nupkg" -type f | head -n 1)
if [[ -z "$PACKAGE_FILE" ]]; then
    echo -e "${RED}❌ No .nupkg file found in ./nupkg directory${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Package created: $(basename "$PACKAGE_FILE")${NC}"

# Show package info
echo -e "${CYAN}📋 Package information:${NC}"
echo "   File: $PACKAGE_FILE"
echo "   Size: $(du -h "$PACKAGE_FILE" | cut -f1)"

if [[ "$DRY_RUN" == "true" ]]; then
    echo -e "${GREEN}🔍 Dry run completed successfully!${NC}"
    echo "   Package file: $PACKAGE_FILE"
    echo "   To publish, run without --dry-run and provide --api-key"
else
    # Publish to NuGet
    echo -e "${YELLOW}🚀 Publishing to NuGet.org...${NC}"
    dotnet nuget push "$PACKAGE_FILE" --api-key "$API_KEY" --source https://api.nuget.org/v3/index.json --verbosity minimal
    check_result "Publish"
    
    echo -e "${GREEN}🎉 Successfully published to NuGet.org!${NC}"
    echo "   Package: $(basename "$PACKAGE_FILE")"
    echo "   It may take a few minutes to appear in search results."
fi

echo ""
echo -e "${GREEN}✨ Operation completed successfully!${NC}"
