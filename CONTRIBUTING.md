# Contributing to SpecTrace

Thank you for your interest in contributing to SpecTrace! This document provides guidelines for contributing to this project.

## Project Philosophy

SpecTrace is designed primarily as an **end-user application** for system information and diagnostics. While we welcome contributions, please note:

- **User Experience First**: All changes should prioritize ease of use for end users
- **Stability Over Features**: Reliability and compatibility are more important than cutting-edge features
- **Privacy Focused**: No telemetry, no data collection, no unnecessary network connections
- **Professional Quality**: Code should be production-ready and well-tested

## Types of Contributions Welcome

### 🐛 Bug Reports
- Clear reproduction steps
- System information (use SpecTrace itself!)
- Expected vs actual behavior
- Screenshots if applicable

### 💡 Feature Requests
- Clear use case description
- Benefit to typical users
- Consider impact on simplicity and performance

### 🔧 Code Contributions
- Bug fixes
- Performance improvements
- Hardware compatibility enhancements
- UI/UX improvements
- Documentation updates

### 📚 Documentation
- User guides and tutorials
- FAQ improvements
- Translation support
- Video demonstrations

## Development Guidelines

### Getting Started
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test thoroughly on multiple Windows versions
5. Submit a pull request

### Code Standards
- Follow existing code style and patterns
- Add comments for complex logic
- Include error handling and validation
- Test on both Windows 10 and Windows 11
- Verify compatibility with both .NET Framework and .NET 8.0

### Testing Requirements
- Test basic functionality without admin privileges
- Test advanced features with admin privileges
- Verify all export formats work correctly
- Test light and dark themes
- Check performance impact (startup time under 3 seconds)

### Pull Request Process
1. **Small, focused changes** - One feature/fix per PR
2. **Clear description** - What changes and why
3. **Test results** - Include testing details
4. **Documentation updates** - Update README if needed
5. **No breaking changes** - Maintain backward compatibility

## Development Environment

### Prerequisites
- Visual Studio 2022 or Visual Studio Code
- .NET 8.0 SDK
- Windows 10 version 1909+ or Windows 11
- Git for version control

### Building
```bash
# Clone the repository
git clone https://github.com/jhew/SpecTrace.git
cd SpecTrace

# Restore dependencies
dotnet restore src/SpecTrace/SpecTrace.csproj

# Build the project
dotnet build src/SpecTrace/SpecTrace.csproj --configuration Release

# Run the application
dotnet run --project src/SpecTrace/SpecTrace.csproj
```

### Project Structure
```
SpecTrace/
├── src/SpecTrace/              # Main application
│   ├── Views/                  # WPF UI components
│   ├── Services/               # Hardware detection services
│   ├── Models/                 # Data models
│   └── Utils/                  # Utility classes
├── docs/                       # Documentation
├── .github/workflows/          # CI/CD configuration
└── README.md                   # Project documentation
```

## Hardware Detection Guidelines

When adding new hardware detection:

1. **Graceful Degradation**: Always handle cases where hardware isn't present
2. **Timeout Protection**: All WMI/hardware queries must have timeouts
3. **Error Handling**: Never crash on hardware detection failures
4. **Performance**: Avoid slow operations in quick scan mode
5. **Compatibility**: Test on different hardware configurations

### Supported Detection Methods
- Windows Management Instrumentation (WMI)
- Setup API
- Registry queries (read-only)
- Win32 APIs
- Hardware-specific APIs (when available)

### Avoid
- Direct hardware register access
- Kernel-mode drivers
- Unstable or experimental APIs
- Vendor-specific SDKs (unless widely available)

## Submitting Issues

### Bug Reports
Use the bug report template and include:
- SpecTrace version
- Windows version
- Hardware configuration
- Steps to reproduce
- Expected vs actual behavior
- Error messages or logs

### Feature Requests
- Clear description of the proposed feature
- Use case and benefits
- Any relevant hardware/software details
- Mockups or examples (if applicable)

## Code of Conduct

### Our Standards
- **Professional Communication**: Respectful and constructive feedback
- **Inclusive Environment**: Welcome contributors of all backgrounds
- **Focus on Technical Merit**: Evaluate contributions objectively
- **User-Centric Thinking**: Always consider the end-user impact

### Unacceptable Behavior
- Harassment or discrimination
- Trolling or inflammatory comments
- Personal attacks or insults
- Publishing private information without consent

## Recognition

Contributors will be recognized in:
- GitHub contributors list
- Release notes for significant contributions
- Special thanks in documentation

## Questions?

- **General Questions**: Use [GitHub Discussions](https://github.com/jhew/SpecTrace/discussions)
- **Bug Reports**: Use [GitHub Issues](https://github.com/jhew/SpecTrace/issues)
- **Security Issues**: Email privately to maintainers

Thank you for helping make SpecTrace better for everyone! 🚀