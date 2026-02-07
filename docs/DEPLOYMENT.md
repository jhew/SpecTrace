# SpecTrace Deployment Guide

This guide covers silent usage, packaging options, and deployment examples for enterprise tooling.

## Silent install/run

SpecTrace is a portable, single EXE. There is no installer required, so “silent install” typically means:

1. Copy `SpecTrace.exe` to a target folder.
2. Run it with CLI flags to export reports.
3. Collect the exported file(s) with your deployment tool.

### Example: run silently and export JSON

```powershell
# Create an output folder
New-Item -ItemType Directory -Path "C:\ProgramData\SpecTrace" -Force | Out-Null

# Run SpecTrace with redaction and a limited section set
Start-Process -FilePath "C:\ProgramData\SpecTrace\SpecTrace.exe" \
  -ArgumentList "--quick --redact --select cpu,memory,storage --json C:\ProgramData\SpecTrace\specs.json" \
  -Wait -NoNewWindow
```

### Example: full export in unattended mode

```powershell
Start-Process -FilePath "C:\ProgramData\SpecTrace\SpecTrace.exe" \
  -ArgumentList "--deep --redact --json C:\ProgramData\SpecTrace\specs.json" \
  -Wait -NoNewWindow
```

> Tip: If you omit output flags, SpecTrace prints a summary to stdout. For automation, prefer `--json` (or another export flag) to generate files.

## Packaging options

### Portable deployment (recommended)

- **Pros**: No installer, minimal footprint, easy rollback.
- **Flow**:
  1. Ship `SpecTrace.exe` to a known folder (e.g., `C:\ProgramData\SpecTrace`).
  2. Run it with CLI flags.
  3. Collect output files.
  4. Remove the EXE if you want a clean exit.

### MSIX packaging

If your environment standardizes on MSIX, you can wrap the EXE:

1. Use the MSIX Packaging Tool to capture a clean install.
2. Add a **desktop shortcut** or **start menu entry** for `SpecTrace.exe`.
3. Configure **App Installer** or Intune to deploy the MSIX package.
4. Use a **run script** or **scheduled task** for automated collection.

> Because SpecTrace is portable, MSIX is mainly for centralized distribution/updates rather than installation requirements.

### MSI packaging

For MSI-based environments, you can wrap `SpecTrace.exe` with a simple MSI:

1. Use a packaging tool (e.g., WiX Toolset) to place `SpecTrace.exe` into `C:\ProgramData\SpecTrace`.
2. Add uninstall metadata and optional shortcuts.
3. Use your software distribution platform to deploy the MSI.

> The MSI’s primary role is distribution and inventory tracking. SpecTrace itself still runs as a portable EXE.

## Deployment examples

### Microsoft Intune (Win32 app)

**Install command** (copy EXE):

```powershell
powershell -ExecutionPolicy Bypass -File .\Install-SpecTrace.ps1
```

**Sample `Install-SpecTrace.ps1`:**

```powershell
$target = "C:\ProgramData\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path ".\SpecTrace.exe" -Destination "$target\SpecTrace.exe" -Force
```

**Detection rule idea:**

- File exists: `C:\ProgramData\SpecTrace\SpecTrace.exe`

**Run script for collection:**

```powershell
Start-Process -FilePath "C:\ProgramData\SpecTrace\SpecTrace.exe" \
  -ArgumentList "--quick --redact --select cpu,graphics,storage --json C:\ProgramData\SpecTrace\specs.json" \
  -Wait -NoNewWindow
```

### Microsoft SCCM / MECM

**Application install program:**

```powershell
powershell -ExecutionPolicy Bypass -File Install-SpecTrace.ps1
```

**Run command as a program or scheduled task:**

```powershell
"C:\ProgramData\SpecTrace\SpecTrace.exe" --quick --redact --select cpu,network,storage --json "C:\ProgramData\SpecTrace\specs.json"
```

**Collect output** using SCCM hardware inventory, a file collection task, or a custom script that uploads `specs.json`.

### Popular RMM tools

#### NinjaOne

- **Script**: PowerShell
- **Run as**: System

```powershell
$target = "C:\ProgramData\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path "$PSScriptRoot\SpecTrace.exe" -Destination "$target\SpecTrace.exe" -Force
Start-Process -FilePath "$target\SpecTrace.exe" \
  -ArgumentList "--quick --redact --select cpu,memory,storage --json $target\specs.json" \
  -Wait -NoNewWindow
```

#### Datto RMM

- **Component**: PowerShell
- **Run as**: System

```powershell
$target = "C:\ProgramData\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path "$PSScriptRoot\SpecTrace.exe" -Destination "$target\SpecTrace.exe" -Force
Start-Process -FilePath "$target\SpecTrace.exe" \
  -ArgumentList "--quick --redact --select cpu,graphics,storage --json $target\specs.json" \
  -Wait -NoNewWindow
```

#### ConnectWise Automate

- **Script**: PowerShell or Batch
- **Run as**: System

```powershell
$target = "C:\ProgramData\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path "$PSScriptRoot\SpecTrace.exe" -Destination "$target\SpecTrace.exe" -Force
Start-Process -FilePath "$target\SpecTrace.exe" \
  -ArgumentList "--quick --redact --select cpu,network,storage --json $target\specs.json" \
  -Wait -NoNewWindow
```

> Adjust `--select` based on the sections you need: `cpu,memory,graphics,storage,network,usb,audio,sensors,security,processes`.

## Automation tips

- **Redaction**: Use `--redact` for safe sharing.
- **Targeted exports**: Use `--select` for smaller, faster collections.
- **Centralized collection**: Place output in a predictable path like `C:\ProgramData\SpecTrace`.
- **Scheduling**: Use Task Scheduler or your RMM to run nightly/weekly exports.

## Troubleshooting

- **Empty output**: Ensure `SpecTrace.exe` is reachable and write permissions exist for the output folder.
- **Deep scan**: `--deep` requires administrative privileges.
- **Exit codes**: A non-zero exit code can indicate partial detection (e.g., CPU not detected).
