using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using SpecTrace.Models;
using Microsoft.Win32;

namespace SpecTrace.Detectors
{
    public class MachineDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectMachineInfo(systemInfo.Machine, redact);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Machine detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectMachineInfo(MachineInfo machine, bool redact)
        {
            // Get hostname
            machine.Hostname = redact ? "REDACTED" : Environment.MachineName;

            // Get OS information
            DetectOsInfo(machine);
            
            // Get machine manufacturer/model
            DetectHardwareInfo(machine, redact);
            
            // Get motherboard information
            DetectMotherboardInfo(machine, redact);
            
            // Get security status
            DetectSecurityStatus(machine);
        }

        private void DetectOsInfo(MachineInfo machine)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    machine.Os = GetStringValue(obj, "Caption");
                    machine.Build = GetIntValue(obj, "BuildNumber");
                    break;
                }
            }
            catch
            {
                machine.Os = Environment.OSVersion.ToString();
            }
        }

        private void DetectHardwareInfo(MachineInfo machine, bool redact)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    machine.Manufacturer = GetStringValue(obj, "Manufacturer");
                    machine.Model = GetStringValue(obj, "Model");
                    break;
                }

                // Get serial number from BIOS
                using var biosSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                using var biosResults = biosSearcher.Get();

                foreach (ManagementObject obj in biosResults)
                {
                    machine.SerialNumber = redact ? "REDACTED" : GetStringValue(obj, "SerialNumber");
                    break;
                }
            }
            catch
            {
                // Hardware info detection failed
            }
        }

        private void DetectMotherboardInfo(MachineInfo machine, bool redact)
        {
            try
            {
                // Get motherboard information from Win32_BaseBoard
                using var boardSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                using var boardResults = boardSearcher.Get();

                foreach (ManagementObject obj in boardResults)
                {
                    machine.Motherboard.Manufacturer = GetStringValue(obj, "Manufacturer");
                    machine.Motherboard.Model = GetStringValue(obj, "Product");
                    machine.Motherboard.Version = GetStringValue(obj, "Version");
                    machine.Motherboard.SerialNumber = redact ? "REDACTED" : GetStringValue(obj, "SerialNumber");
                    break;
                }

                // Get BIOS information from Win32_BIOS
                using var biosSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                using var biosResults = biosSearcher.Get();

                foreach (ManagementObject obj in biosResults)
                {
                    machine.Motherboard.BiosVendor = GetStringValue(obj, "Manufacturer");
                    machine.Motherboard.BiosVersion = GetStringValue(obj, "SMBIOSBIOSVersion");
                    
                    // Parse BIOS release date
                    var biosDate = GetStringValue(obj, "ReleaseDate");
                    if (!string.IsNullOrEmpty(biosDate) && biosDate.Length >= 8)
                    {
                        // WMI date format is typically: YYYYMMDDHHMMSS.MMMMMM+UUU
                        try
                        {
                            var year = biosDate.Substring(0, 4);
                            var month = biosDate.Substring(4, 2);
                            var day = biosDate.Substring(6, 2);
                            machine.Motherboard.BiosDate = $"{year}-{month}-{day}";
                        }
                        catch
                        {
                            machine.Motherboard.BiosDate = biosDate;
                        }
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Motherboard detection failed: {ex.Message}");
                // Motherboard info detection failed, fields will remain empty
            }
        }

        private void DetectSecurityStatus(MachineInfo machine)
        {
            try
            {
                // Check Secure Boot status
                machine.SecureBoot = IsSecureBootEnabled();
                
                // Check TPM status
                machine.Tpm = GetTpmVersion();
            }
            catch
            {
                // Security status detection failed
            }
        }

        private bool IsSecureBootEnabled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                if (key != null)
                {
                    var value = key.GetValue("UEFISecureBootEnabled");
                    return Convert.ToInt32(value ?? 0) == 1;
                }
            }
            catch
            {
                // Fallback method using WMI
                try
                {
                    using var searcher = new ManagementObjectSearcher(@"root\cimv2\security\microsofttpm", 
                        "SELECT * FROM Win32_Tpm");
                    return searcher.Get().Count > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private string GetTpmVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\cimv2\security\microsofttpm", 
                    "SELECT * FROM Win32_Tpm");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var specVersion = GetStringValue(obj, "SpecVersion");
                    if (specVersion.StartsWith("2."))
                        return "2.0";
                    else if (specVersion.StartsWith("1."))
                        return "1.2";
                    else
                        return specVersion;
                }
            }
            catch
            {
                // Try registry approach
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TPM\WMI");
                    if (key != null)
                        return "2.0"; // Assume 2.0 if TPM service exists
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }

        private string GetStringValue(ManagementObject obj, string property)
        {
            return obj[property]?.ToString() ?? "";
        }

        private int GetIntValue(ManagementObject obj, string property)
        {
            if (obj[property] == null) return 0;
            if (int.TryParse(obj[property].ToString(), out int result))
                return result;
            return 0;
        }
    }
}
