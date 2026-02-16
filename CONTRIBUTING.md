# Contributing to APM CLI (.NET)

Thank you for considering contributing to the .NET port of APM CLI! This is a community-driven fork that maintains feature parity with the original Python implementation, targeting environments where Python is unavailable or Windows users without WSL.

This document outlines the process for contributing to the project.

## Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

## How to Contribute

### Reporting Bugs

Before submitting a bug report:

1. Check the [GitHub Issues](https://github.com/seiggy/apm-dotnet/issues) to see if the bug has already been reported.
2. Update your copy of the code to the latest version to ensure the issue hasn't been fixed.

When submitting a bug report:

1. Use our bug report template.
2. Include detailed steps to reproduce the bug.
3. Describe the expected behavior and what actually happened.
4. Include any relevant logs or error messages.
5. Specify your OS, .NET SDK version, and runtime (e.g. NativeAOT binary vs `dotnet run`).

### Suggesting Enhancements

Enhancement suggestions are welcome! Please:

1. Use our feature request template.
2. Clearly describe the enhancement and its benefits.
3. Provide examples of how the enhancement would work.

### Development Process

1. Fork the repository.
2. Create a new branch for your feature/fix: `git checkout -b feature/your-feature-name` or `git checkout -b fix/issue-description`.
3. Make your changes.
4. Run tests: `dotnet test`
5. Ensure your code compiles cleanly (`TreatWarningsAsErrors` is enabled).
6. Commit your changes with a descriptive message.
7. Push to your fork.
8. Submit a pull request.

### Pull Request Process

1. **Choose the appropriate PR template** for your change:
   - **🚀 New Feature**: Use the `feature.md` template
   - **🐛 Bug Fix**: Use the `bugfix.md` template
   - **📖 Documentation**: Use the `documentation.md` template
   - **🔧 Maintenance**: Use the `maintenance.md` template
   - **Other**: Use the standard PR template

2. **Apply the correct label** after creating your PR:
   - `enhancement` or `feature` - New functionality
   - `bug` or `fix` - Bug fixes
   - `documentation` or `docs` - Documentation updates
   - `ignore-for-release` - Exclude from release notes

3. Follow the template provided.
4. Ensure your PR addresses only one concern (one feature, one bug fix).
5. Include tests for new functionality.
6. Update documentation if needed.
7. PRs must pass all checks before they can be merged.

**Note**: Labels are used to automatically categorize changes in release notes. The correct label helps maintainers and users understand what changed in each release.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Git

## Development Environment

```bash
# Clone the repository
git clone https://github.com/seiggy/apm-dotnet.git
cd apm-dotnet

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run from source
dotnet run --project src/Apm.Cli
```

## Testing

We use **xUnit** for unit testing with **FakeItEasy** for mocking and **AwesomeAssertions** for fluent assertions.

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
apm/
├── apm.slnx                    # Solution file
├── Directory.Build.props        # Shared build properties
├── Directory.Packages.props     # Central package version management
├── src/
│   └── Apm.Cli/                # Main CLI application
│       ├── Adapters/
│       ├── Commands/
│       ├── Compilation/
│       ├── Core/
│       ├── Dependencies/
│       ├── Integration/
│       ├── Models/
│       ├── Output/
│       ├── Primitives/
│       ├── Registry/
│       ├── Runtime/
│       ├── Utils/
│       ├── Workflow/
│       └── Program.cs
├── tests/
│   └── Apm.Cli.Tests/          # Unit & integration tests
├── templates/                   # APM project templates
└── docs/                        # Documentation
```

## Coding Style

This project follows:
- C# latest language features (configured via `Directory.Build.props`)
- **Nullable reference types** are enabled project-wide
- **Implicit usings** are enabled
- **TreatWarningsAsErrors** is enabled — all warnings must be resolved
- Follow standard [.NET naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## NativeAOT Considerations

This project supports NativeAOT compilation for self-contained, single-file binaries. When contributing:

- Avoid reflection-heavy patterns that break AOT trimming
- Test your changes with a NativeAOT publish when modifying serialization or dynamic code:
  ```bash
  dotnet publish src/Apm.Cli -c Release -r win-x64 -p:NativeAot=true
  ```
- Supported RIDs: `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`

## Documentation

If your changes affect how users interact with the project, update the documentation accordingly.

## License

By contributing to this project, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).

## Questions?

If you have any questions, feel free to open an issue or reach out to the maintainers.

Thank you for your contributions!