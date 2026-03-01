# SpecTrace Deployment Guide

This guide covers installation options and deployment approaches for distributing SpecTrace on individual machines or across an organisation.

## Distribution formats

Each GitHub Release includes two options:

| File | Best for |
|------|----------|
| `SpecTrace-x.y.z-Setup.exe` | Managed machines, shared workstations, Start Menu integration |
| `SpecTrace-x.y.z-Portable.zip` | USB kits, temporary use, environments that forbid installers |
| `SpecTrace-x.y.z-SHA256SUMS.txt` | Integrity verification, winget manifest authoring |

Both require the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime?runtime=desktop&os=windows&arch=x64). The installer detects and links to it automatically if absent.

---

## Installer (`-Setup.exe`)

The installer is built with Inno Setup and supports both elevated and standard-user installs:

| Run as | Install location |
|--------|-----------------|
| Administrator | `%ProgramFiles%\SpecTrace` |
| Standard user | `%LocalAppData%\Programs\SpecTrace` |

**Silent install (admin):**

```powershell
Start-Process -FilePath ".\SpecTrace-1.0.0-Setup.exe" -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART" -Wait
```

**Silent install (standard user, no UAC prompt):**

```powershell
Start-Process -FilePath ".\SpecTrace-1.0.0-Setup.exe" -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CURRENTUSER" -Wait
```

**Silent uninstall:**

```powershell
# Admin install
Start-Process -FilePath "${env:ProgramFiles}\SpecTrace\unins000.exe" -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES" -Wait

# Standard-user install
Start-Process -FilePath "${env:LOCALAPPDATA}\Programs\SpecTrace\unins000.exe" -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES" -Wait
```

---

## Portable (`-Portable.zip`)

Extract the ZIP to any folder and run `SpecTrace.exe` directly. No registry changes, no installation artefacts.

```powershell
$target = "C:\Tools\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Expand-Archive -Path ".\SpecTrace-1.0.0-Portable.zip" -DestinationPath $target -Force
```

---

## Enterprise deployment

### winget

```powershell
winget install SpecTrace.SpecTrace
```

Available once the winget-pkgs manifest is published for the relevant version.

### Microsoft Intune (Win32 app)

Wrap `SpecTrace-x.y.z-Setup.exe` as a Win32 app:

- **Install command:** `SpecTrace-1.0.0-Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART`
- **Uninstall command:** `"%ProgramFiles%\SpecTrace\unins000.exe" /VERYSILENT /SUPPRESSMSGBOXES`
- **Detection rule:** File exists — `%ProgramFiles%\SpecTrace\SpecTrace.exe`
- **Return codes:** `0` = success, `3010` = reboot required (unlikely but map it as success/soft reboot)

### Microsoft SCCM / MECM

Create a standard Application with:

- **Install program:** `SpecTrace-1.0.0-Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART`
- **Uninstall program:** `"%ProgramFiles%\SpecTrace\unins000.exe" /VERYSILENT /SUPPRESSMSGBOXES`
- **Detection method:** File — `%ProgramFiles%\SpecTrace\SpecTrace.exe`, exists

### RMM tools (NinjaOne, Datto, ConnectWise, etc.)

Deploy via a PowerShell script component:

```powershell
# Download and run the installer silently
$release  = "1.0.0"
$url      = "https://github.com/jhew/SpecTrace/releases/download/v$release/SpecTrace-$release-Setup.exe"
$installer = "$env:TEMP\SpecTrace-Setup.exe"

Invoke-WebRequest -Uri $url -OutFile $installer -UseBasicParsing
Start-Process -FilePath $installer -ArgumentList "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART" -Wait
Remove-Item $installer -Force
```

---

## Exporting system information

SpecTrace is a GUI application — users run it, view their hardware summary, and export via the **Export...** button in the toolbar. Supported formats: **JSON, HTML, Markdown, plain text**.

For automated or unattended hardware collection in enterprise pipelines, evaluate tools with a dedicated CLI (e.g., `winmgmt` queries, PowerShell `Get-WmiObject`, or SCCM hardware inventory) rather than SpecTrace.

---

## Troubleshooting

| Symptom | Likely cause |
|---------|-------------|
| App does not start | .NET 8 Desktop Runtime missing — download from the link above |
| Blank WMI fields | WMI service not running; run `winmgmt /resetrepository` as admin |
| SmartScreen warning | Binary not yet code-signed; click *More info → Run anyway*, or check the SHA-256 in `SHA256SUMS.txt` |
| Deep Scan shows less data | Deep Scan needs admin rights; re-run as administrator for full SMART/power data |
