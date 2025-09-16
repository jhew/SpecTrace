# SpecTrace Troubleshooting Guide

This guide helps resolve common issues when using SpecTrace. If you don't find a solution here, please check our [FAQ](FAQ.md) or report the issue on [GitHub](../../issues).

## Application Won't Start

### ❌ Error: "This app can't run on your PC"
**Cause**: Incompatible Windows version or missing .NET runtime

**Solutions**:
1. **Check Windows Version**:
   - Press `Win + R`, type `winver`, press Enter
   - Ensure you have Windows 10 (build 1903+) or Windows 11
   
2. **Install .NET 8.0 Runtime**:
   - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Install the "Desktop Runtime" (not SDK)
   - Restart your computer

3. **Run Compatibility Check**:
   - Right-click `SpecTrace.exe` → Properties → Compatibility
   - Try "Run compatibility troubleshooter"

### ❌ Error: "Windows protected your PC"
**Cause**: Windows SmartScreen blocking unknown executable

**Solution**:
1. Click "More info" in the SmartScreen dialog
2. Click "Run anyway"
3. **Alternative**: Add SpecTrace to your antivirus exclusions

### ❌ Application starts but crashes immediately
**Cause**: Corrupted download or system compatibility issue

**Solutions**:
1. **Re-download SpecTrace**:
   - Delete current file
   - Download fresh copy from GitHub releases
   - Verify file size matches release notes

2. **Run as Administrator**:
   - Right-click `SpecTrace.exe`
   - Select "Run as administrator"

3. **Check Event Viewer**:
   - Press `Win + R`, type `eventvwr`, press Enter
   - Navigate to Windows Logs → Application
   - Look for SpecTrace-related errors

## Display and Interface Issues

### ❌ Blurry or incorrectly sized interface
**Cause**: High DPI display scaling issues

**Solution**:
1. Right-click `SpecTrace.exe` → Properties
2. Go to "Compatibility" tab
3. Click "Change high DPI settings"
4. Check "Override high DPI scaling behavior"
5. Select "System (Enhanced)" from dropdown
6. Click OK and restart SpecTrace

### ❌ Interface appears cut off or too small
**Cause**: Display resolution or scaling configuration

**Solutions**:
1. **Adjust Windows Scaling**:
   - Right-click desktop → Display settings
   - Try different scaling percentages (100%, 125%, 150%)
   
2. **Change Screen Resolution**:
   - Ensure resolution is at least 1024x768
   - Try native resolution of your monitor

3. **Reset Window Position**:
   - Hold `Shift` while starting SpecTrace
   - This resets window to default position and size

### ❌ Dark theme looks wrong or unreadable
**Cause**: Windows theme compatibility or display driver issues

**Solutions**:
1. Switch to Light theme in SpecTrace
2. Update your display drivers
3. Check Windows theme settings:
   - Settings → Personalization → Colors
   - Ensure "Choose your default app mode" is set correctly

## Data Detection Issues

### ❌ System information shows as "Unknown" or missing
**Cause**: Driver issues, permissions, or hardware compatibility

**Solutions**:
1. **Update Drivers**:
   - Open Device Manager (`Win + X` → Device Manager)
   - Look for devices with yellow warning icons
   - Update drivers for problematic devices

2. **Run as Administrator**:
   - Some system information requires elevated permissions
   - Right-click SpecTrace → "Run as administrator"

3. **Check WMI Service**:
   - Press `Win + R`, type `services.msc`, press Enter
   - Find "Windows Management Instrumentation"
   - Ensure it's "Running" and set to "Automatic"

### ❌ CPU temperature not showing
**Cause**: Hardware doesn't support temperature reporting or driver limitations

**Solutions**:
1. Install manufacturer's system monitoring software
2. Update motherboard/chipset drivers
3. Note: Many systems don't expose temperature through standard Windows APIs

### ❌ Wrong amount of RAM or storage detected
**Cause**: Hardware reporting differences or reserved system memory

**Explanations**:
- **RAM**: Windows reserves some memory for hardware and system use
- **Storage**: Manufacturer's stated capacity vs. actual formatted capacity
- **Graphics**: Shared system memory may reduce available RAM

These differences are normal and reflect how Windows sees your system.

## Export and File Issues

### ❌ Export function not working
**Cause**: Permissions, disk space, or file access issues

**Solutions**:
1. **Check Permissions**:
   - Try exporting to Desktop or Documents folder
   - Ensure you have write access to selected location

2. **Free Disk Space**:
   - Ensure destination drive has at least 100 MB free
   - Choose a different location if needed

3. **File In Use**:
   - Close any programs that might be using the export file
   - Try a different filename

4. **Path Length**:
   - Use shorter folder paths and filenames
   - Avoid special characters in filename

### ❌ Exported file is corrupted or incomplete
**Cause**: Interrupted export process or disk issues

**Solutions**:
1. **Retry Export**:
   - Close other applications
   - Try exporting again

2. **Check Disk Health**:
   - Run `chkdsk` on the destination drive
   - Try exporting to a different drive

3. **Different Format**:
   - Try exporting in a different format (TXT instead of JSON)

## Performance Issues

### ❌ SpecTrace is very slow to start or respond
**Cause**: System load, antivirus scanning, or hardware issues

**Solutions**:
1. **Close Other Programs**:
   - Close unnecessary applications
   - Wait for system startup to complete

2. **Antivirus Exclusion**:
   - Add SpecTrace.exe to antivirus exclusions
   - Temporarily disable real-time scanning for testing

3. **Check System Resources**:
   - Open Task Manager (`Ctrl + Shift + Esc`)
   - Ensure CPU and memory usage aren't at 100%

4. **Restart Computer**:
   - A fresh restart often resolves performance issues

### ❌ System detection takes too long
**Cause**: Complex system configuration or hardware timeouts

**Solutions**:
1. **Be Patient**: First scan can take 30-60 seconds on complex systems
2. **Disconnect External Devices**: Unplug unnecessary USB devices
3. **Close Network Applications**: VPN or network monitoring tools may slow detection

## Network and Security Issues

### ❌ Antivirus software blocks SpecTrace
**Cause**: False positive detection by security software

**Solutions**:
1. **Add Exception**:
   - Add SpecTrace.exe to antivirus whitelist/exclusions
   - Add the folder containing SpecTrace to exclusions

2. **Download from Official Source**:
   - Only download from GitHub releases page
   - Verify file hash if provided

3. **Temporary Disable**:
   - Temporarily disable real-time protection
   - Run SpecTrace, then re-enable protection

### ❌ Corporate firewall/security blocks execution
**Cause**: Enterprise security policies

**Solutions**:
1. **Contact IT Department**: Request approval for SpecTrace
2. **Use Portable Mode**: Run from removable media if allowed
3. **Request Whitelist**: Provide GitHub repository URL for verification

## Advanced Troubleshooting

### Collecting Debug Information

If basic troubleshooting doesn't help, collect this information before reporting an issue:

1. **System Information**:
   ```
   Win + R → msinfo32 → File → Save
   ```

2. **Event Logs**:
   ```
   Win + R → eventvwr → Windows Logs → Application
   Look for SpecTrace entries around the time of the problem
   ```

3. **SpecTrace Version**:
   - Right-click SpecTrace.exe → Properties → Details
   - Note File version and Product version

### Reset SpecTrace Settings

If SpecTrace behavior is inconsistent:

1. **Delete Configuration**:
   - Close SpecTrace
   - Delete any config files in the SpecTrace folder
   - Restart SpecTrace with default settings

2. **Clean Installation**:
   - Delete current SpecTrace.exe
   - Download fresh copy from GitHub
   - Run from clean folder

### System File Checker

If Windows itself seems problematic:

1. **Run SFC Scan**:
   ```
   Win + X → Windows PowerShell (Admin)
   sfc /scannow
   ```

2. **DISM Repair**:
   ```
   DISM /Online /Cleanup-Image /RestoreHealth
   ```

## Getting Additional Help

### Before Reporting Issues

Please gather:
- ✅ Windows version (`winver` command)
- ✅ SpecTrace version (from file properties)
- ✅ Exact error message or description
- ✅ Steps to reproduce the problem
- ✅ Screenshot of the issue (if visual)

### Where to Get Help

1. **GitHub Issues**: [Report bugs](../../issues)
2. **GitHub Discussions**: [Ask questions](../../discussions)
3. **Documentation**: [User Guide](USER_GUIDE.md) and [FAQ](FAQ.md)

### What Information to Include

When reporting issues, include:
```
- Windows Version: [e.g., Windows 11 22H2]
- SpecTrace Version: [e.g., 1.0.0]
- Problem Description: [Detailed description]
- Steps to Reproduce: [1. Do this, 2. Then this, 3. Error occurs]
- Expected Behavior: [What should happen]
- Actual Behavior: [What actually happens]
- Error Messages: [Exact text of any errors]
- Screenshots: [If applicable]
```

---

*Still having issues? Don't hesitate to reach out through our [GitHub repository](../../issues)!*