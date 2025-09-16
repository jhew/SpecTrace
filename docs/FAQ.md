# Frequently Asked Questions

## General Questions

### What is SpecTrace?
SpecTrace is a free Windows system information tool that provides detailed hardware and software information about your computer. It's designed for users who need to understand their system specifications for technical support, hardware upgrades, or documentation purposes.

### Is SpecTrace free?
Yes, SpecTrace is completely free and open-source under the MIT License. You can use it for personal or commercial purposes without any restrictions.

### Do I need to install SpecTrace?
No! SpecTrace is a portable application. Just download the `.exe` file and run it directly. No installation, no registry changes, no system modifications required.

### Does SpecTrace work on my version of Windows?
SpecTrace supports:
- ✅ Windows 11 (all versions)
- ✅ Windows 10 (version 1903 and later)
- ❌ Windows 8.1 and earlier (not supported)

### Is my data safe?
Absolutely. SpecTrace:
- Runs entirely on your computer - no data is sent anywhere
- Includes automatic privacy protection (PII redaction)
- Doesn't require internet access
- Leaves no traces on your system when closed

## Usage Questions

### Why is the first scan taking so long?
The initial system scan can take 10-30 seconds because SpecTrace is:
- Detecting all hardware components
- Querying detailed specifications
- Building the initial data cache
Subsequent runs are much faster!

### Some information appears as "Unknown" - why?
This can happen when:
- **Missing Drivers**: Install the latest drivers for your hardware
- **Restricted Access**: Some information requires specific permissions
- **Older Hardware**: Very old components may not report all details
- **Virtual Machines**: VMs may not expose all hardware information

### Can I run SpecTrace without Administrator privileges?
Yes! SpecTrace is designed to work with standard user permissions. However, running as Administrator may provide access to additional system information.

### How do I update SpecTrace?
Since SpecTrace is portable:
1. Download the latest version from the Releases page
2. Replace your old `SpecTrace.exe` with the new one
3. Your settings and preferences are preserved

## Export and Data Questions

### What's the difference between export formats?

| Format | Best For | File Size | Human Readable |
|--------|----------|-----------|----------------|
| **Text (.txt)** | Technical support, documentation | Small | Very Easy |
| **JSON (.json)** | Integration, automation | Medium | Moderate |
| **XML (.xml)** | Enterprise, compliance | Large | Difficult |

### Where are exported files saved?
By default, exports are saved to your Documents folder, but you can choose any location during the export process.

### Can I automate SpecTrace exports?
Currently, SpecTrace is designed as an interactive application. Command-line automation features may be added in future versions.

### How large are the exported files?
Typical file sizes:
- **Text format**: 5-15 KB
- **JSON format**: 10-25 KB  
- **XML format**: 15-35 KB

File size depends on your system complexity (number of drives, network adapters, etc.).

## Privacy and Security

### What personal information does SpecTrace collect?
SpecTrace automatically redacts or masks:
- Computer names (partially masked)
- User names
- Serial numbers
- Network MAC addresses
- Windows product keys

### Can I disable PII redaction?
The redaction feature is built-in for your protection and cannot be disabled in the current version.

### Does SpecTrace access the internet?
No. SpecTrace works completely offline and never connects to the internet.

### Is SpecTrace safe to run on corporate networks?
Yes. SpecTrace:
- Doesn't require network access
- Doesn't modify system settings
- Doesn't install anything
- Only reads publicly available system information

## Technical Questions

### What technology is SpecTrace built with?
SpecTrace is built using:
- **.NET 8.0**: Modern, high-performance framework
- **WPF**: Windows Presentation Foundation for the user interface
- **WMI**: Windows Management Instrumentation for hardware detection

### Can I run multiple copies of SpecTrace?
Yes, you can run multiple instances simultaneously. Each window operates independently.

### Why does SpecTrace show different information than other tools?
Different tools use different methods to gather information:
- SpecTrace uses Windows APIs and WMI for maximum compatibility
- Some tools use direct hardware access (requires Administrator)
- Older tools may not support newer hardware

The information SpecTrace provides is based on what Windows reports about your system.

### Does SpecTrace support other operating systems?
Currently, SpecTrace is Windows-only. Support for other operating systems is not planned.

## Troubleshooting

### SpecTrace won't start - what should I do?
Try these steps:
1. **Check Windows version**: Ensure you have Windows 10 (1903+) or Windows 11
2. **Install .NET**: Download .NET 8.0 Runtime from Microsoft
3. **Run as Administrator**: Right-click → "Run as administrator"
4. **Check antivirus**: Some antivirus programs may block unknown executables

### The interface looks blurry or incorrectly sized
This is a display scaling issue:
1. Right-click on `SpecTrace.exe`
2. Select "Properties" → "Compatibility"
3. Click "Change high DPI settings"
4. Check "Override high DPI scaling behavior"
5. Select "System" from the dropdown

### Export function isn't working
Check that:
- You have write permissions to the selected folder
- The destination drive has enough free space
- The filename doesn't contain invalid characters
- Another program isn't using the file

### I found a bug - how do I report it?
Please report bugs on our [GitHub Issues page](../../issues) and include:
- Your Windows version
- SpecTrace version
- Steps to reproduce the problem
- Any error messages
- Exported system information (if relevant)

## Getting Help

### Where can I get more help?
- **User Guide**: See the complete [User Guide](USER_GUIDE.md)
- **Troubleshooting**: Check the [Troubleshooting Guide](TROUBLESHOOTING.md)
- **GitHub Issues**: Report bugs or request features
- **GitHub Discussions**: Ask questions and share tips

### How can I contribute or provide feedback?
- **Feedback**: Share your experience through GitHub Discussions
- **Bug Reports**: Use GitHub Issues for problems
- **Feature Requests**: Suggest improvements through GitHub Issues
- **Code Contributions**: See our [Contributing Guide](../CONTRIBUTING.md)

---

*Don't see your question here? Check our [GitHub Discussions](../../discussions) or create a new discussion.*