---
applyTo: ".github/workflows/build-release.yml"
description: "CI/CD Pipeline configuration for NativeAOT binary packaging and release workflow"
---

# CI/CD Pipeline Instructions

## NativeAOT Binary Packaging
- **CRITICAL**: Uses .NET NativeAOT ahead-of-time compilation for self-contained single-file binaries
- **Binary Structure**: Creates `publish/{rid}/apm` (single executable per platform)
- **Platform Naming**: `apm-{os}-{arch}` (e.g., `apm-osx-arm64`, `apm-linux-x64`)
- **Project File**: `src/Apm.Cli/Apm.Cli.csproj` handles NativeAOT publish settings

## Artifact Flow
- **Upload**: Artifacts include the NativeAOT-compiled binary for each platform
- **Download**: GitHub Actions downloads artifacts by name
- **Release Prep**: Archive binaries using `tar -czf "apm-{rid}.tar.gz" apm` (Linux/macOS) or `zip` (Windows)

## Critical Testing Phases
1. **Unit Tests**: `dotnet test` with xUnit, FakeItEasy, AwesomeAssertions
2. **Integration Tests**: Full source code access for comprehensive testing
3. **Release Validation**: ISOLATION testing - validates exact shipped binary experience

## Release Flow Dependencies
- **Sequential Jobs**: test → build → integration-tests → release-validation → create-release → publish-nuget
- **Tag Triggers**: Only `v*.*.*` tags trigger full release pipeline
- **Artifact Retention**: 30 days for debugging failed releases

## Key Configuration
- `.NET 10` - Target framework across all jobs
- `PublishAot=true` - NativeAOT compilation enabled
- `SelfContained=true` - No runtime dependency required
- 5-platform matrix: ubuntu-latest (x64/arm64), macos-latest (x64/arm64), windows-latest (x64)

## Performance Considerations
- NativeAOT produces fast-starting, small binaries (~10-20MB)
- Trimming enabled to remove unused framework code
- Single-file deployment simplifies distribution
- Matrix builds across platforms for parallel compilation