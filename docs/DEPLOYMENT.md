# SpecTrace Deployment Guide

This guide covers distribution options for SpecTrace.

## System requirements

- Windows 11 (build 22000) or later
- x64 processor
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime?runtime=desktop&os=windows&arch=x64)

---

## Option 1 — Portable ZIP

The portable build is a single `SpecTrace.exe` that runs directly without installation.

### Manual distribution

1. Download `SpecTrace-<version>-Portable.zip` from the [Releases page](https://github.com/jhew/SpecTrace/releases/latest).
2. Extract to any folder (USB drive, network share, `C:\ProgramData\SpecTrace`, etc.).
3. Run `SpecTrace.exe`.

### Intune / SCCM (Win32 app)

**Install command** — copies the EXE to a well-known location:

```powershell
# Install-SpecTrace.ps1
$target = "C:\ProgramData\SpecTrace"
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path ".\SpecTrace.exe" -Destination "$target\SpecTrace.exe" -Force
```

**Uninstall command:**

```powershell
Remove-Item -Path "C:\ProgramData\SpecTrace" -Recurse -Force -ErrorAction SilentlyContinue
```

**Detection rule:** File exists at `C:\ProgramData\SpecTrace\SpecTrace.exe`

---

## Option 2 — Inno Setup Installer

The installer build (`SpecTrace-<version>-Setup.exe`) is produced by the release pipeline alongside the portable ZIP.

### Interactive install

Double-click the installer. It will:

- Check for the .NET 8 Desktop Runtime and prompt the user to download it if missing.
- Install to `%LocalAppData%\Programs\SpecTrace` for standard users, or `%ProgramFiles%\SpecTrace` when elevated.
- Add a Start Menu entry and an optional desktop shortcut.

### Silent install

```powershell
# Install silently for the current user (no elevation needed)
.\SpecTrace-1.0.0-Setup.exe /SILENT /NORESTART

# Install machine-wide to %ProgramFiles% (requires admin)
Start-Process -FilePath ".\SpecTrace-1.0.0-Setup.exe" -ArgumentList "/SILENT /NORESTART" -Verb RunAs -Wait
```

Inno Setup silent-install flags:

| Flag | Effect |
|---|---|
| `/SILENT` | No wizard, shows progress dialog |
| `/VERYSILENT` | No UI at all |
| `/NORESTART` | Suppresses any reboot prompt |
| `/TASKS=""` | Skips the desktop-icon task |

### winget

Once the winget-pkgs manifest is submitted for a release, users can install and update via:

```
winget install SpecTrace.SpecTrace
winget upgrade SpecTrace.SpecTrace
```

---

## Verifying downloads

Every release includes a `SHA256SUMS.txt` file. Verify before deploying:

```powershell
$expected = (Get-Content .\SpecTrace-1.0.0-SHA256SUMS.txt | Select-String "Setup.exe").ToString().Split()[0]
$actual   = (Get-FileHash .\SpecTrace-1.0.0-Setup.exe -Algorithm SHA256).Hash.ToLower()
if ($expected -eq $actual) { Write-Host "OK" } else { Write-Warning "Hash mismatch!" }
```
