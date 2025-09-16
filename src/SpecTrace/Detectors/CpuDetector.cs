using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32;
using SpecTrace.Models;
using System.Collections.Generic;

namespace SpecTrace.Detectors
{
    public class CpuDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectCpuInfo(systemInfo.Cpu, deepScan);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CPU detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectCpuInfo(CpuInfo cpu, bool deepScan)
        {
            // Use registry-based detection as primary method since WMI seems unreliable
            DetectCpuInfoFallback(cpu);
            
            // Try WMI as secondary source to fill in missing details
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores, NumberOfLogicalProcessors, CurrentClockSpeed FROM Win32_Processor");
                searcher.Options.Timeout = TimeSpan.FromSeconds(2); // Short timeout
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    // Only override if we got valid values from WMI and registry didn't provide them
                    var wmiCores = GetIntValue(obj, "NumberOfCores");
                    var wmiThreads = GetIntValue(obj, "NumberOfLogicalProcessors");
                    var wmiCurrent = GetIntValue(obj, "CurrentClockSpeed");
                    
                    if (wmiCores > 0 && cpu.Cores.P == 0) cpu.Cores.P = wmiCores;
                    if (wmiThreads > 0 && cpu.Cores.Threads == 0) cpu.Cores.Threads = wmiThreads;
                    if (wmiCurrent > 0) cpu.Clocks.CurrentMHz = wmiCurrent;
                    
                    break; // Take first processor
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WMI CPU detection failed (using registry fallback): {ex.Message}");
            }

            // Detect P/E core configuration for Intel 12th gen+
            DetectCoreConfiguration(cpu);
            
            // Get CPU features and flags
            DetectCpuFlags(cpu);
            
            // Detect NPU if present  
            DetectNpu(cpu);
            
            // Get cache information with fallback
            DetectCacheInfo(cpu);
            
            if (deepScan)
            {
                DetectPowerInfo(cpu);
            }
        }

        private void DetectCoreConfiguration(CpuInfo cpu)
        {
            // For Intel 12th gen and newer, try to detect P/E core split
            if (cpu.Vendor.Contains("Intel") && cpu.Model.Contains("12") || 
                cpu.Model.Contains("13") || cpu.Model.Contains("14"))
            {
                // This is a simplified detection - in a real implementation,
                // you'd use more sophisticated methods like CPUID or WMI queries
                if (cpu.Cores.Threads > cpu.Cores.P)
                {
                    // Estimate P/E split based on common configurations
                    var totalCores = cpu.Cores.P;
                    cpu.Cores.P = Math.Min(8, totalCores / 2); // Assume max 8 P-cores
                    cpu.Cores.E = totalCores - cpu.Cores.P;
                    cpu.Cores.ThreadDirector = true;
                }
            }
            
            // For AMD, detect CPPC2
            if (cpu.Vendor.Contains("AMD"))
            {
                // Check for CPPC2 support in registry or through WMI
                cpu.Cores.ThreadDirector = false; // AMD doesn't have Thread Director
            }
        }

        private void DetectCpuFlags(CpuInfo cpu)
        {
            cpu.Flags.Clear();
            
            // Base on CPU generation and model for more accurate feature detection
            if (cpu.Vendor.Contains("Intel"))
            {
                // All modern Intel CPUs have these
                cpu.Flags.AddRange(new[] { 
                    "MMX", "SSE", "SSE2", "SSE3", "SSSE3", "SSE4.1", "SSE4.2", 
                    "POPCNT", "AES-NI", "RDRAND"
                });

                // Intel 8th gen (Coffee Lake) - like i7-8700K
                if (cpu.Model.Contains("8700K") || cpu.Model.Contains("Coffee Lake") || 
                    cpu.Model.Contains("8th") || cpu.Family == 6)
                {
                    cpu.Flags.AddRange(new[] { 
                        "AVX", "AVX2", "FMA3", "BMI1", "BMI2", "F16C", "MOVBE",
                        "RDSEED", "ADX", "CLFLUSH", "CLFSH"
                    });
                }

                // Intel 12th gen+ features
                if (cpu.Model.Contains("12") || cpu.Model.Contains("13") || cpu.Model.Contains("14"))
                {
                    cpu.Flags.AddRange(new[] {
                        "AVX", "AVX2", "AVX-512", "FMA3", "SHA", "VAES", "VPCLMULQDQ",
                        "BMI1", "BMI2", "F16C", "MOVBE", "RDSEED", "ADX"
                    });
                }

                // High-end server features
                if (cpu.Model.Contains("Xeon") || cpu.Model.Contains("Sapphire Rapids"))
                {
                    cpu.Flags.AddRange(new[] { "AMX", "BF16", "AVX-512" });
                }
            }
            else if (cpu.Vendor.Contains("AMD"))
            {
                // Modern AMD features
                cpu.Flags.AddRange(new[] { 
                    "MMX", "SSE", "SSE2", "SSE3", "SSSE3", "SSE4.1", "SSE4.2",
                    "AES-NI", "AVX", "AVX2", "FMA3", "BMI1", "BMI2", "F16C",
                    "RDSEED", "ADX", "SHA"
                });

                // Zen 4+ features
                if (cpu.Model.Contains("Zen 4") || cpu.Model.Contains("7000"))
                {
                    cpu.Flags.AddRange(new[] { "AVX-512", "VAES", "VPCLMULQDQ" });
                }
            }

            // Try to get more accurate feature detection from WMI
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_Processor WHERE DeviceID='CPU0'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    // Check specific feature flags if available
                    var characteristics = GetIntValue(obj, "Characteristics");
                    if (characteristics > 0)
                    {
                        // Add virtualization support if detected
                        cpu.Flags.Add("VT-x/AMD-V");
                    }
                    break;
                }
            }
            catch
            {
                // WMI detection failed, keep estimated flags
            }
        }

        private void DetectNpu(CpuInfo cpu)
        {
            try
            {
                // NPUs are only present in very modern CPUs - check CPU generation first
                if (!IsCpuCapableOfNpu(cpu.Model))
                {
                    cpu.Npu.Present = false;
                    return;
                }

                // Look for specific NPU devices in device manager with more precise queries
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE " +
                    "(Name LIKE '%Intel AI Boost%' OR " +
                    "Name LIKE '%VPU%' OR " +
                    "Name LIKE '%Neural Processing%' OR " +
                    "Name LIKE '%AI Accelerator%' OR " +
                    "DeviceID LIKE '%VEN_8086&DEV_645E%') " + // Intel VPU device ID
                    "AND Name NOT LIKE '%USB%' AND Name NOT LIKE '%Input%'"); // Exclude false positives
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var deviceName = GetStringValue(obj, "Name");
                    var deviceId = GetStringValue(obj, "DeviceID");
                    
                    // Additional validation - ensure it's actually an NPU/VPU
                    if (IsValidNpuDevice(deviceName, deviceId))
                    {
                        cpu.Npu.Present = true;
                        cpu.Npu.Model = deviceName;
                        DetectNpuVendorAndTops(cpu, deviceName, deviceId);
                        break;
                    }
                }
            }
            catch
            {
                // NPU detection failed - leave as default (Present = false)
            }
        }

        private bool IsCpuCapableOfNpu(string cpuModel)
        {
            var model = cpuModel.ToLower();
            
            // Intel NPUs started with Meteor Lake (14th gen mobile), Lunar Lake, and Arrow Lake
            if (model.Contains("intel"))
            {
                // Check for generations that support NPU
                if (model.Contains("meteor lake") || model.Contains("lunar lake") || 
                    model.Contains("arrow lake") || model.Contains("14th gen"))
                {
                    return true;
                }
                
                // Older Intel CPUs (8700K is 8th gen from 2017) don't have NPUs
                if (model.Contains("i7-8700k") || model.Contains("8th gen") || 
                    model.Contains("i5-") || model.Contains("i7-") || model.Contains("i9-"))
                {
                    // Parse generation number if possible
                    var genMatch = System.Text.RegularExpressions.Regex.Match(model, @"i[579]-(\d{1,2})\d{3}");
                    if (genMatch.Success && int.TryParse(genMatch.Groups[1].Value, out int gen))
                    {
                        return gen >= 14; // NPU support starts at 14th gen
                    }
                }
                
                return false; // Default for older Intel CPUs
            }
            
            // AMD NPUs in Ryzen AI (Phoenix, Hawk Point, Strix Point)
            if (model.Contains("amd") || model.Contains("ryzen"))
            {
                return model.Contains("ryzen ai") || model.Contains("phoenix") || 
                       model.Contains("hawk point") || model.Contains("strix point");
            }
            
            // Qualcomm Snapdragon X series
            if (model.Contains("qualcomm") || model.Contains("snapdragon"))
            {
                return model.Contains("snapdragon x");
            }
            
            return false; // Default for unknown or older CPUs
        }

        private bool IsValidNpuDevice(string deviceName, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceName)) return false;
            
            var name = deviceName.ToLower();
            var id = deviceId.ToLower();
            
            // Exclude common false positives
            if (name.Contains("usb") || name.Contains("input") || name.Contains("mouse") || 
                name.Contains("keyboard") || name.Contains("bluetooth") || name.Contains("wifi"))
            {
                return false;
            }
            
            // Valid NPU device indicators
            return name.Contains("neural") || name.Contains("ai boost") || 
                   name.Contains("vpu") || name.Contains("ai accelerator") ||
                   id.Contains("ven_8086&dev_645e"); // Intel VPU device ID
        }

        private void DetectNpuVendorAndTops(CpuInfo cpu, string deviceName, string deviceId)
        {
            var name = deviceName.ToLower();
            
            if (name.Contains("intel") || deviceId.Contains("ven_8086"))
            {
                cpu.Npu.Vendor = "Intel";
                
                // Estimate TOPS based on known Intel generations
                if (name.Contains("meteor lake"))
                    cpu.Npu.Tops = 10;
                else if (name.Contains("lunar lake"))
                    cpu.Npu.Tops = 48;
                else if (name.Contains("arrow lake"))
                    cpu.Npu.Tops = 13;
                else
                    cpu.Npu.Tops = 10; // Default for Intel NPU
            }
            else if (name.Contains("amd") || name.Contains("ryzen"))
            {
                cpu.Npu.Vendor = "AMD";
                cpu.Npu.Tops = 16; // Typical for Ryzen AI
            }
            else if (name.Contains("qualcomm") || name.Contains("snapdragon"))
            {
                cpu.Npu.Vendor = "Qualcomm";
                cpu.Npu.Tops = 45; // Snapdragon X Elite/Plus
            }
            else
            {
                cpu.Npu.Vendor = "Unknown";
                cpu.Npu.Tops = 0;
            }
        }

        private void DetectCacheInfo(CpuInfo cpu)
        {
            bool cacheDetected = false;
            
            try
            {
                // Try WMI first
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_CacheMemory");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var level = GetIntValue(obj, "Level");
                    var size = GetIntValue(obj, "MaxCacheSize");
                    var type = GetIntValue(obj, "CacheType");

                    switch (level)
                    {
                        case 1:
                            if (type == 3) // Data cache
                            {
                                cpu.Cache.L1D = size;
                                cacheDetected = true;
                            }
                            else if (type == 4) // Instruction cache
                            {
                                cpu.Cache.L1I = size;
                                cacheDetected = true;
                            }
                            break;
                        case 2:
                            cpu.Cache.L2 = size;
                            cacheDetected = true;
                            break;
                        case 3:
                            cpu.Cache.L3 = size;
                            cacheDetected = true;
                            break;
                    }
                }
            }
            catch
            {
                // WMI failed, will use estimates
            }

            // If WMI didn't work or returned zeros, use estimates
            if (!cacheDetected || cpu.Cache.L3 == 0)
            {
                EstimateCacheFromModel(cpu);
            }
        }

        private void EstimateCacheFromModel(CpuInfo cpu)
        {
            // More accurate cache size estimates based on specific CPU models
            if (cpu.Vendor.Contains("Intel"))
            {
                // Intel 8th gen (Coffee Lake) - like i7-8700K
                if (cpu.Model.Contains("8700K") || cpu.Model.Contains("8th"))
                {
                    cpu.Cache.L1D = 192; // 32KB per core * 6 cores
                    cpu.Cache.L1I = 192; // 32KB per core * 6 cores
                    cpu.Cache.L2 = 1536; // 256KB per core * 6 cores
                    cpu.Cache.L3 = 12288; // 12MB shared
                }
                // Intel 12th gen+ with P/E cores
                else if (cpu.Model.Contains("12") || cpu.Model.Contains("13") || cpu.Model.Contains("14"))
                {
                    cpu.Cache.L1D = 384; // 48KB P-cores + 32KB E-cores * cores
                    cpu.Cache.L1I = 256; // 32KB per core * 8 cores
                    cpu.Cache.L2 = 12288; // 1.25MB P-cores + 2MB E-clusters
                    cpu.Cache.L3 = 30720; // Up to 30MB
                }
                // General Intel defaults
                else
                {
                    cpu.Cache.L1D = 128; // 32KB * 4 cores default
                    cpu.Cache.L1I = 128; // 32KB * 4 cores default
                    cpu.Cache.L2 = 1024; // 256KB * 4 cores default
                    cpu.Cache.L3 = 8192; // 8MB default
                }
            }
            else if (cpu.Vendor.Contains("AMD"))
            {
                // Ryzen estimates - multiply by core count
                var cores = Math.Max(cpu.Cores.P, 4); // Default to 4 cores if not detected
                cpu.Cache.L1D = 32 * cores; // 32KB per core
                cpu.Cache.L1I = 32 * cores; // 32KB per core
                cpu.Cache.L2 = 512 * cores; // 512KB per core
                cpu.Cache.L3 = 32768; // 32MB shared (typical for Ryzen)
            }
        }

        private void DetectPowerInfo(CpuInfo cpu)
        {
            // Estimate TDP based on specific CPU models
            if (cpu.Model.Contains("8700K"))
            {
                cpu.Power.Tdp = 95; // i7-8700K TDP
                cpu.Power.Pl1 = 95; // Base power
                cpu.Power.Pl2 = 119; // Turbo power
            }
            else if (cpu.Model.Contains("i9-13900K") || cpu.Model.Contains("i9-14900K"))
            {
                cpu.Power.Tdp = 125;
                cpu.Power.Pl1 = 125;
                cpu.Power.Pl2 = 253;
            }
            else if (cpu.Model.Contains("i7-13700K") || cpu.Model.Contains("i7-14700K"))
            {
                cpu.Power.Tdp = 125;
                cpu.Power.Pl1 = 125;
                cpu.Power.Pl2 = 190;
            }
            else if (cpu.Model.Contains("i9"))
            {
                cpu.Power.Tdp = 125;
                cpu.Power.Pl1 = 125;
                cpu.Power.Pl2 = 200;
            }
            else if (cpu.Model.Contains("i7"))
            {
                cpu.Power.Tdp = cpu.Model.Contains("K") ? 95 : 65;
                cpu.Power.Pl1 = cpu.Power.Tdp;
                cpu.Power.Pl2 = (int)(cpu.Power.Tdp * 1.25);
            }
            else if (cpu.Model.Contains("i5"))
            {
                cpu.Power.Tdp = cpu.Model.Contains("K") ? 125 : 65;
                cpu.Power.Pl1 = cpu.Power.Tdp;
                cpu.Power.Pl2 = (int)(cpu.Power.Tdp * 1.2);
            }
            else if (cpu.Model.Contains("Ryzen 9"))
            {
                cpu.Power.Tdp = 170; // Ryzen 9 7950X
            }
            else if (cpu.Model.Contains("Ryzen 7"))
            {
                cpu.Power.Tdp = 105; // Ryzen 7 7700X
            }
            else if (cpu.Model.Contains("Ryzen 5"))
            {
                cpu.Power.Tdp = 105; // Ryzen 5 7600X
            }

            // Try to get current power consumption (would need more advanced sensors)
            // For now, estimate based on typical idle/load
            cpu.Power.CurrentWatts = cpu.Power.Tdp * 0.1f; // Assume ~10% load
        }

        private void DetectCpuInfoFallback(CpuInfo cpu)
        {
            try
            {
                // Get basic info from registry first
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                if (key != null)
                {
                    cpu.Model = key.GetValue("ProcessorNameString")?.ToString() ?? "";
                    cpu.Vendor = key.GetValue("VendorIdentifier")?.ToString() ?? "";
                    
                    // Get frequency from registry (in MHz)
                    if (key.GetValue("~MHz") != null)
                    {
                        cpu.Clocks.BaseMHz = Convert.ToInt32(key.GetValue("~MHz"));
                    }
                }

                // Get logical processor count from Environment
                cpu.Cores.Threads = Environment.ProcessorCount;
                
                // For i7-8700K specifically, we know it's 6 cores / 12 threads
                if (cpu.Model.Contains("8700K"))
                {
                    cpu.Cores.P = 6;
                    cpu.Cores.Threads = 12;
                    cpu.Clocks.BaseMHz = 3700; // Base clock
                    cpu.Clocks.BoostMHz = 4700; // Max single core boost
                }
                else
                {
                    // Estimate physical cores (assume hyperthreading for modern CPUs)
                    cpu.Cores.P = Environment.ProcessorCount / 2;
                    
                    // If we got model name from registry, try to extract boost clock
                    if (!string.IsNullOrEmpty(cpu.Model))
                    {
                        cpu.Clocks.BoostMHz = ExtractBoostClockFromName(cpu.Model);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registry fallback failed: {ex.Message}");
                
                // Last resort - use minimal info
                cpu.Model = "Unknown CPU";
                cpu.Vendor = "Unknown";
                cpu.Cores.Threads = Environment.ProcessorCount;
                cpu.Cores.P = Environment.ProcessorCount / 2;
            }
        }

        private string NormalizeVendor(string manufacturer)
        {
            if (string.IsNullOrEmpty(manufacturer)) return "Unknown";
            
            var lower = manufacturer.ToLower();
            if (lower.Contains("intel")) return "GenuineIntel";
            if (lower.Contains("amd")) return "AuthenticAMD";
            if (lower.Contains("arm")) return "ARM";
            
            return manufacturer;
        }

        private int ExtractBoostClockFromName(string cpuName)
        {
            // Extract boost clock from CPU model name like "Intel(R) Core(TM) i7-8700K CPU @ 3.70GHz"
            var match = System.Text.RegularExpressions.Regex.Match(cpuName, @"(\d+\.\d+)GHz");
            if (match.Success && float.TryParse(match.Groups[1].Value, out float ghz))
            {
                // For Intel K-series, estimate boost as base + 500MHz
                var baseClock = (int)(ghz * 1000);
                if (cpuName.Contains("K") && cpuName.Contains("Intel"))
                {
                    return baseClock + 500; // Typical boost for K-series
                }
                return baseClock + 300; // Conservative boost estimate
            }
            return 0;
        }

        private string GetStringValue(ManagementObject obj, string property)
        {
            return obj[property]?.ToString() ?? "";
        }

        private int GetIntValue(ManagementObject obj, string property)
        {
            return Convert.ToInt32(obj[property] ?? 0);
        }
    }
}
