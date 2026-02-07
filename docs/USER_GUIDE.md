# SpecTrace User Guide

## Getting Started

### System Requirements
- **Operating System**: Windows 10 or later (Windows 11 recommended)
- **Memory**: 4 GB RAM minimum, 8 GB recommended
- **Storage**: 10 MB for application, additional space for exported reports
- **Display**: 1024x768 minimum resolution
- **Permissions**: Standard user permissions (Administrator not required)

### Installation
1. Download the latest `SpecTrace.exe` from the [Releases page](../../releases)
2. Save it to any folder on your computer (Desktop, Downloads, etc.)
3. Double-click to run - no installation required!

> **Note**: SpecTrace is a portable application that doesn't require installation. You can run it from any location.

## Using SpecTrace

### Main Interface

When you start SpecTrace, you'll see:
- **System Overview**: Key hardware and OS information at a glance
- **Navigation Tabs**: Click to explore different system categories
- **Export Button**: Save your system information to a file
- **Theme Toggle**: Switch between Light and Dark themes

### Gathering System Information

1. **Automatic Detection**: SpecTrace automatically scans your system when it starts
2. **Manual Refresh**: Click the refresh button to update information
3. **Category Navigation**: Use tabs to view specific hardware categories:
   - **Overview**: Summary of key system specs
   - **Processor**: CPU details, cores, speed, features
   - **Memory**: RAM information, usage, available slots
   - **Storage**: Hard drives, SSDs, available space
   - **Graphics**: GPU information, display settings
   - **Network**: Network adapters, connections
   - **System**: Operating system, BIOS, motherboard details

### Exporting System Information

1. Click the **Export** button in the main interface
2. Choose your preferred format:
   - **Text File (.txt)**: Simple, readable format
   - **JSON File (.json)**: Structured data format
   - **XML File (.xml)**: Standardized markup format
3. Select a save location
4. Click **Save**

Your exported file will contain all detected system information in an organized format.

### Privacy and Security

SpecTrace includes built-in privacy protection:
- **PII Redaction**: Personal information is automatically hidden or masked
- **Local Processing**: All data stays on your computer - nothing is sent online
- **No Installation Required**: Run from anywhere without system changes
- **No Registry Changes**: Leaves no traces on your system

## Common Use Cases

### Technical Support
When contacting technical support:
1. Run SpecTrace and export your system information
2. Attach the exported file to your support request
3. Support staff can quickly understand your system configuration

### Hardware Upgrades
Before upgrading your computer:
1. Export current system specs for reference
2. Check compatibility requirements against your current setup
3. Keep the export as a backup of your old configuration

### System Documentation
For IT professionals or personal records:
1. Run SpecTrace on each computer
2. Export and organize the files by computer name/user
3. Maintain an inventory of your hardware assets

### Troubleshooting
When experiencing system issues:
1. Export system information before making changes
2. Use the data to research compatibility issues
3. Compare before/after exports to identify changes

## Keyboard Shortcuts

- **Ctrl + R**: Refresh system information
- **Ctrl + E**: Export system information
- **Ctrl + T**: Toggle theme (Light/Dark)
- **F1**: Show this help
- **Esc**: Close current dialog

## Themes

SpecTrace includes multiple visual themes:
- **Light Theme**: Clean, bright interface for daytime use
- **Dark Theme**: Easy on the eyes for low-light environments
- **Auto Theme**: Follows your Windows theme setting

Switch themes using the theme button in the top-right corner.

## Performance Tips

- **First Run**: Initial system scan may take 10-30 seconds
- **Large Systems**: Systems with many drives or network adapters may take longer to scan
- **Background Apps**: Close unnecessary programs for faster scanning
- **Regular Use**: Subsequent runs are typically faster as information is cached

## Data Accuracy

SpecTrace provides the most accurate information available from Windows APIs:
- **Hardware Detection**: Based on Windows Management Instrumentation (WMI)
- **Real-time Data**: CPU usage, memory usage, and temperatures are live
- **Historical Data**: Some information reflects system history since last boot
- **Driver Dependent**: Some details depend on having proper drivers installed

## File Formats Explained

### Text Format (.txt)
```
=== SYSTEM OVERVIEW ===
Computer Name: DESKTOP-ABC123
Operating System: Windows 11 Pro
Processor: Intel Core i7-12700K
Memory: 16 GB DDR4
...
```
Best for: Human reading, email sharing, simple documentation

### JSON Format (.json)
```json
{
  "system": {
    "computerName": "DESKTOP-ABC123",
    "operatingSystem": "Windows 11 Pro",
    "processor": "Intel Core i7-12700K"
  }
}
```
Best for: Integration with other tools, automated processing, developers

### XML Format (.xml)
```xml
<system>
  <computerName>DESKTOP-ABC123</computerName>
  <operatingSystem>Windows 11 Pro</operatingSystem>
  <processor>Intel Core i7-12700K</processor>
</system>
```
Best for: Enterprise systems, standardized data exchange, compliance

## Need More Help?

- **Deployment Guide**: See the [Deployment Guide](DEPLOYMENT.md) for silent install, packaging, and RMM examples.
- **FAQ**: Check the [Frequently Asked Questions](FAQ.md)
- **Troubleshooting**: See the [Troubleshooting Guide](TROUBLESHOOTING.md)
- **Issues**: Report problems on our [GitHub Issues page](../../issues)
- **Discussions**: Join conversations on [GitHub Discussions](../../discussions)

---

*Last updated: December 2024*
