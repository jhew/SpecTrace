# Contributing

## Bug reports

Open an issue with:
- SpecTrace version
- Windows version
- Steps to reproduce
- Expected vs actual behaviour
- Any error messages shown in the UI

## Feature requests

Open an issue describing the use case and what problem it solves.

## Pull requests

- Keep changes focused  one fix or feature per PR
- Follow the existing code style
- Add error handling for any new hardware queries (never let a detection failure crash the app)
- Test on Windows 11
- Test without administrator privileges, and with if the feature requires elevation
- Update documentation if the change affects behaviour visible to users

## Building

Prerequisites: .NET 8.0 SDK, Windows 11, Visual Studio 2022 or VS Code.

```
git clone https://github.com/jhew/SpecTrace.git
cd SpecTrace
dotnet restore src/SpecTrace/SpecTrace.csproj
dotnet build src/SpecTrace/SpecTrace.csproj --configuration Release
dotnet run --project src/SpecTrace/SpecTrace.csproj
```

## Hardware detection guidelines

- All WMI and hardware queries must have timeouts
- Handle missing or unsupported hardware gracefully  log to debug output, do not throw
- Avoid slow operations in quick scan mode; defer them to deep scan
- Use WMI, Setup API, Win32 APIs, or read-only registry queries
- Do not use kernel-mode drivers, direct register access, or vendor-specific SDKs

## Questions

Use [GitHub Discussions](https://github.com/jhew/SpecTrace/discussions) for general questions and [GitHub Issues](https://github.com/jhew/SpecTrace/issues) for bugs and feature requests.
