# Privacy

*Last updated: September 2025*

## Data collection

SpecTrace does not collect, transmit, or store any data remotely. It makes no network connections of any kind.

## What SpecTrace reads locally

To generate a system report, SpecTrace reads hardware and configuration information from the local machine via Windows APIs (WMI, Setup API, registry, Win32). This includes:

- Hardware identifiers (CPU, memory, storage, GPU, monitors, network adapters)
- System configuration (OS version, BIOS, security feature status)
- Performance metrics (clocks, temperatures, SMART data)

This information is displayed in the application and optionally written to a file you specify. Nothing is sent elsewhere.

## PII redaction

The Redact PII option suppresses the following fields from the UI and any exports:

- Hostnames
- Serial numbers (system, drives, memory modules)
- MAC addresses
- Volume labels

Enable this before exporting if the report will be shared externally.

## Exported files

Exports are written only to paths you choose. SpecTrace does not create configuration files, write to the registry, or store data in any location other than where you explicitly save it.

## Third-party components

SpecTrace uses standard Windows system APIs and .NET 8.0. These operate locally and do not transmit data.

## Source code

SpecTrace is open source. The full source is available for review at [github.com/jhew/SpecTrace](https://github.com/jhew/SpecTrace).
