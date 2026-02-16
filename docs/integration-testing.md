# Integration Testing

This document describes APM's integration testing strategy for the .NET port to ensure runtime setup scripts work correctly and the golden scenario from the README functions as expected.

## Testing Strategy

APM uses a tiered approach to integration testing:

### 1. **Unit Tests** (Every CI run)
- **Location**: `tests/Apm.Cli.Tests/`
- **Framework**: xUnit with FakeItEasy (mocking) and AwesomeAssertions (fluent assertions)
- **Purpose**: Fast verification of individual components
- **Duration**: ~1-2 minutes per platform
- **Trigger**: Every push/PR

### 2. **Integration Tests** (Every CI run)
- **Location**: `tests/Apm.Cli.Tests/` (integration test classes)
- **Purpose**: Verification that runtime setup, compilation, and install workflows function correctly
- **Scope**:
  - Binary functionality (`--version`, `--help`)
  - APM runtime detection
  - Workflow compilation without execution
- **Duration**: ~2-3 minutes per platform
- **Trigger**: Every push/PR

### 3. **NativeAOT Binary Validation** (Releases only)
- **Purpose**: Complete verification using NativeAOT-compiled binaries
- **Scope**:
  - Project initialization (`apm init`)
  - Compile dry-run (`apm compile --dry-run`)
  - Binary isolation (no source checkout required)
- **Duration**: ~5-10 minutes per platform
- **Trigger**: Only on version tags (releases)

## Running Tests Locally

### Unit & Integration Tests
```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~WorkflowTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### NativeAOT Binary Testing

```bash
# Build NativeAOT binary for your platform
dotnet publish src/Apm.Cli -c Release -r win-x64 -p:NativeAot=true

# Test the binary
./publish/apm --version
./publish/apm init test-project
cd test-project
../publish/apm compile --dry-run
```

## CI/CD Integration

### GitHub Actions Workflow

**On every push/PR:**
1. Unit tests + Integration tests (cross-platform matrix)

**On version tag releases:**
1. Unit tests + Integration tests
2. Build NativeAOT binaries (cross-platform: win-x64, linux-x64, linux-arm64, osx-x64, osx-arm64)
3. Pack NuGet package
4. Integration tests with built binaries
5. Release validation (isolated binary testing)
6. Create GitHub Release
7. Push to NuGet.org

**Manual workflow dispatch:**
- Test builds (uploads as workflow artifacts)
- Allows testing the full build pipeline without creating a release
- Useful for validating changes before tagging

### GitHub Actions Authentication

Integration tests may require GitHub API access:

**Required Permissions:**
- `contents: read` - for repository access

**Environment Variables:**
- `GITHUB_TOKEN: ${{ secrets.GH_MODELS_PAT }}` - for runtime tests
- `GITHUB_APM_PAT: ${{ secrets.GH_CLI_PAT }}` - for package installations
- `ADO_APM_PAT: ${{ secrets.ADO_APM_PAT }}` - for Azure DevOps packages

### Release Pipeline Sequencing

The workflow ensures quality gates at each step:

1. **test** job - Unit tests + integration tests (all platforms)
2. **build** job - NativeAOT binary compilation (depends on test success)
3. **pack-nuget** job - NuGet package creation (depends on test success)
4. **integration-tests** job - Binary integration tests (depends on build success)
5. **release-validation** job - Isolated binary validation (depends on integration-tests)
6. **publish-release** job - GitHub Release + NuGet.org push (depends on all previous)

Each stage must succeed before proceeding to the next, ensuring only fully validated releases reach users.

### Test Matrix

All tests run on:
- **Linux x64**: ubuntu-24.04
- **Linux ARM64**: ubuntu-24.04-arm
- **macOS Intel**: macos-15-intel
- **macOS Apple Silicon**: macos-latest (arm64)
- **Windows x64**: windows-latest

**.NET Version**: 10.0.x

## What the Tests Verify

### Unit & Integration Tests Verify:
- ✅ YAML manifest parsing and validation
- ✅ Dependency resolution and tree building
- ✅ Compilation engine correctness
- ✅ Primitive discovery and conflict detection
- ✅ Runtime adapter functionality
- ✅ CLI command parsing and execution
- ✅ Git-based package download

### Binary Validation Tests Verify:
- ✅ NativeAOT binary starts and responds to commands
- ✅ `apm init` creates correct project structure
- ✅ `apm compile --dry-run` works without dependencies
- ✅ Binaries work in isolation (no source checkout)
- ✅ Cross-platform binary compatibility

## Benefits

### **Speed vs Confidence Balance**
- **Unit tests**: Fast feedback (~1 min) on every change
- **Integration tests**: Medium confidence (~3 min) on every change
- **Binary validation**: High confidence (~10 min) only when shipping

### **Cost Efficiency**
- Unit and integration tests use no API credits
- Binary validation only runs on releases
- Manual workflow dispatch for test builds without publishing

### **Platform Coverage**
- Tests run on all 5 supported platforms (including Windows native)
- Catches platform-specific NativeAOT issues
- Windows native support — no WSL required

### **Release Confidence**
- Binary integration tests must pass before any publishing steps
- Multi-stage release pipeline ensures quality gates
- NuGet package validated before push
- Cross-platform binary verification

## Debugging Test Failures

### Unit/Integration Test Failures
- Check test output with `dotnet test --verbosity detailed`
- Review specific test: `dotnet test --filter "FullyQualifiedName~TestName"`
- Ensure .NET 10 SDK is installed: `dotnet --version`

### Binary Validation Failures
- Verify NativeAOT publish succeeded without warnings
- Check platform-specific runtime identifier matches
- Review binary size (unexpectedly small may indicate build issues)
- Test locally with same RID: `dotnet publish src/Apm.Cli -c Release -r <rid> -p:NativeAot=true`

## Adding New Tests

### For New Features:
1. Add unit tests in `tests/Apm.Cli.Tests/` matching the source structure
2. Use `FakeItEasy` for dependency mocking
3. Use `AwesomeAssertions` for fluent assertion syntax
4. If feature requires API calls, consider integration test category

### For New Runtime Support:
1. Add adapter tests
2. Add integration test for runtime setup
3. Update CI matrix if new platform support needed

---

This testing strategy ensures we ship with confidence while maintaining fast development cycles across all supported platforms.
