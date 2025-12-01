# Publishing WhatsAppDotnet to NuGet

This document provides step-by-step instructions for publishing the WhatsAppDotnet library to NuGet.org.

## 📋 Prerequisites

Before publishing, ensure you have:

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate an API key from your NuGet.org profile
3. **.NET 8 SDK**: Installed on your development machine
4. **Git**: For version control and tagging releases

## 🚀 Publishing Steps

### 1. Prepare for Release

#### Update Version Number
Edit `WhatsAppDotnet.csproj` and update the version:
```xml
<Version>1.0.1</Version>
<AssemblyVersion>1.0.1.0</AssemblyVersion>
<FileVersion>1.0.1.0</FileVersion>
```

#### Update CHANGELOG.md
Add your changes to the changelog:
```markdown
## [1.0.1] - 2025-12-XX
### Added
- New feature description
### Fixed  
- Bug fix description
```

#### Commit Changes
```bash
git add .
git commit -m "Release v1.0.1"
git tag v1.0.1
git push origin main --tags
```

### 2. Test Build (Recommended)

Run a dry-run to test the build and packaging:

**On macOS/Linux:**
```bash
./publish-nuget.sh --dry-run
```

**On Windows:**
```powershell
./publish-nuget.ps1 -DryRun
```

This will:
- Clean and build the project
- Run tests (if any)
- Create the `.nupkg` file
- Show package information
- **NOT** publish to NuGet

### 3. Publish to NuGet

#### Option A: Using the Script (Recommended)

**On macOS/Linux:**
```bash
./publish-nuget.sh --api-key YOUR_NUGET_API_KEY
```

**On Windows:**
```powershell
./publish-nuget.ps1 -ApiKey YOUR_NUGET_API_KEY
```

#### Option B: Manual Steps

1. **Build and Pack:**
   ```bash
   dotnet clean --configuration Release
   dotnet restore
   dotnet build --configuration Release --no-restore
   dotnet pack --configuration Release --no-build --output ./nupkg
   ```

2. **Publish:**
   ```bash
   dotnet nuget push ./nupkg/WhatsAppDotnet.*.nupkg \
     --api-key YOUR_NUGET_API_KEY \
     --source https://api.nuget.org/v3/index.json
   ```

### 4. Verify Publication

1. **Check NuGet.org**: Visit [nuget.org/packages/WhatsAppDotnet](https://www.nuget.org/packages/WhatsAppDotnet)
2. **Test Installation**: Try installing in a test project:
   ```bash
   dotnet add package WhatsAppDotnet --version 1.0.1
   ```

## 🔧 Script Options

### macOS/Linux Script (`publish-nuget.sh`)

```bash
./publish-nuget.sh [options]

Options:
  -k, --api-key <key>        NuGet API key (required for publishing)
  -v, --version <version>    Override package version
  -c, --configuration <cfg>  Build configuration (default: Release)
  -d, --dry-run             Build and pack without publishing
  -h, --help                Show help message
```

### Windows Script (`publish-nuget.ps1`)

```powershell
./publish-nuget.ps1 [parameters]

Parameters:
  -ApiKey <key>        NuGet API key (required for publishing)
  -Version <version>   Override package version
  -Configuration <cfg> Build configuration (default: Release)
  -DryRun             Build and pack without publishing
  -Help               Show help message
```

## 🔐 Security Best Practices

### Protecting Your API Key

1. **Never commit API keys** to version control
2. **Use environment variables:**
   ```bash
   export NUGET_API_KEY="your-key-here"
   ./publish-nuget.sh --api-key "$NUGET_API_KEY"
   ```

3. **Use scoped API keys** with minimal permissions
4. **Rotate API keys** regularly

### GitHub Actions (Optional)

For automated publishing, you can set up GitHub Actions:

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    - name: Publish to NuGet
      run: ./publish-nuget.sh --api-key ${{ secrets.NUGET_API_KEY }}
```

## 📦 Package Information

The generated package includes:

- **Library DLL**: `WhatsAppDotnet.dll`
- **XML Documentation**: `WhatsAppDotnet.xml`
- **Dependencies**: Automatically resolved
- **Metadata**: Version, description, license, etc.

## 🚨 Troubleshooting

### Common Issues

1. **"Package already exists"**
   - You cannot overwrite an existing version
   - Increment the version number and try again

2. **"API key is invalid"**
   - Verify your API key is correct
   - Check API key permissions and scope

3. **"Build failed"**
   - Run `dotnet build` manually to see detailed errors
   - Ensure all dependencies are available

4. **"Tests failed"**
   - Fix failing tests before publishing
   - Use `--dry-run` to skip tests temporarily

### Debug Commands

```bash
# Check package contents
dotnet nuget list package ./nupkg/WhatsAppDotnet.*.nupkg

# Validate package
dotnet nuget verify ./nupkg/WhatsAppDotnet.*.nupkg

# Check build output
dotnet build --verbosity detailed
```

## 📈 Post-Publication

### Monitor Package

1. **Download Statistics**: Check download counts on NuGet.org
2. **User Feedback**: Monitor GitHub issues and discussions
3. **Dependencies**: Keep dependencies updated
4. **Security**: Monitor for security advisories

### Promote Your Package

1. **Update README badges** with NuGet version and download counts
2. **Share on social media** and developer communities
3. **Write blog posts** about features and usage
4. **Create video tutorials** for complex scenarios

## 📚 Additional Resources

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Creating NuGet Packages](https://docs.microsoft.com/en-us/nuget/create-packages/)
- [NuGet Best Practices](https://docs.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices)
- [Semantic Versioning](https://semver.org/)

---

**Happy publishing! 🎉** Remember to test thoroughly before each release and follow semantic versioning principles.
