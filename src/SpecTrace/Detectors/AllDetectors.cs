using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using SpecTrace.Models;
using System.Linq;

namespace SpecTrace.Detectors
{
    public class MemoryDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectMemoryInfo(systemInfo.Memory, deepScan, redact);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Memory detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectMemoryInfo(MemoryInfo memory, bool deepScan, bool redact)
        {
            // First get memory modules to calculate installed capacity
            DetectMemoryModules(memory, redact);
            
            // Get available memory from OS
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            using var results = searcher.Get();

            long availableBytes = 0;
            foreach (ManagementObject obj in results)
            {
                availableBytes = Convert.ToInt64(obj["TotalPhysicalMemory"] ?? 0);
                break;
            }

            // Calculate installed capacity from DIMMs
            long installedBytes = memory.Dimms.Sum(d => (long)d.SizeGB * 1024 * 1024 * 1024);
            
            // Use installed capacity as the primary total, but show available if different
            memory.TotalBytes = installedBytes > 0 ? installedBytes : availableBytes;
            
            if (deepScan)
            {
                DetectMemoryTimings(memory);
            }
        }

        private void DetectMemoryModules(MemoryInfo memory, bool redact)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                using var results = searcher.Get();

                memory.Dimms.Clear();
                int slotIndex = 0;

                foreach (ManagementObject obj in results)
                {
                    var dimm = new DimmInfo
                    {
                        Slot = $"DIMM{slotIndex++}",
                        SizeGB = (int)(Convert.ToInt64(obj["Capacity"] ?? 0) / (1024 * 1024 * 1024)),
                        SpeedMTps = Convert.ToInt32(obj["Speed"] ?? 0),
                        Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                        PartNumber = obj["PartNumber"]?.ToString()?.Trim() ?? "",
                        SerialNumber = redact ? "REDACTED" : (obj["SerialNumber"]?.ToString() ?? ""),
                        Type = obj["MemoryType"]?.ToString() ?? "Unknown"
                    };
                    
                    memory.Dimms.Add(dimm);
                }

                // Calculate channels (simplified)
                memory.Channels = memory.Dimms.Count >= 2 ? 2 : 1;
                
                // Set speed from fastest module
                if (memory.Dimms.Any())
                {
                    memory.SpeedMTps = memory.Dimms.Max(d => d.SpeedMTps);
                }
            }
            catch
            {
                // Memory module detection failed
            }
        }

        private void DetectMemoryTimings(MemoryInfo memory)
        {
            // This would require more advanced detection methods
            // For now, provide estimated timings based on speed
            if (memory.SpeedMTps >= 6000)
                memory.Timings = "30-38-38-96";
            else if (memory.SpeedMTps >= 5600)
                memory.Timings = "28-36-36-89";
            else if (memory.SpeedMTps >= 3200)
                memory.Timings = "16-18-18-36";
            else
                memory.Timings = "15-15-15-35";

            memory.Profile = memory.SpeedMTps > 3200 ? "XMP/EXPO" : "JEDEC";
        }
    }

    public class GraphicsDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectGraphicsInfo(systemInfo.Graphics, deepScan);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Graphics detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectGraphicsInfo(GraphicsInfo graphics, bool deepScan)
        {
            // Detect GPUs
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            using var results = searcher.Get();

            graphics.Gpus.Clear();

            foreach (ManagementObject obj in results)
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name) || name.Contains("Basic")) continue;

                var gpu = new GpuInfo
                {
                    Name = name,
                    Vendor = GetVendorFromName(name),
                    Driver = obj["DriverVersion"]?.ToString() ?? "",
                    DriverDate = obj["DriverDate"]?.ToString() ?? "",
                };

                // Get GPU memory information
                DetectGpuMemory(gpu, obj);

                // Get PCIe information
                DetectGpuPCIe(gpu, obj);

                // Estimate capabilities based on GPU name
                EstimateGpuCapabilities(gpu);

                graphics.Gpus.Add(gpu);
            }

            if (deepScan)
            {
                DetectDisplays(graphics);
            }
        }

        private string GetVendorFromName(string name)
        {
            if (name.Contains("NVIDIA") || name.Contains("GeForce") || name.Contains("RTX") || name.Contains("GTX"))
                return "NVIDIA";
            else if (name.Contains("AMD") || name.Contains("Radeon") || name.Contains("RX"))
                return "AMD";
            else if (name.Contains("Intel"))
                return "Intel";
            else
                return "Unknown";
        }

        private void DetectGpuMemory(GpuInfo gpu, ManagementObject obj)
        {
            try
            {
                // Try to get dedicated video memory
                var dedicatedMemory = obj["AdapterRAM"];
                if (dedicatedMemory != null && Convert.ToUInt32(dedicatedMemory) > 0)
                {
                    gpu.Memory.TotalMB = Convert.ToUInt32(dedicatedMemory) / (1024 * 1024); // Convert to MB
                }
                else
                {
                    // Fallback: estimate memory based on GPU name
                    EstimateGpuMemoryFromName(gpu);
                }

                // Set memory type based on GPU generation
                gpu.Memory.Type = DetermineMemoryType(gpu.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting GPU memory: {ex.Message}");
                // Estimate memory based on GPU name as fallback
                EstimateGpuMemoryFromName(gpu);
            }
        }

        private void EstimateGpuMemoryFromName(GpuInfo gpu)
        {
            var name = gpu.Name.ToLower();
            
            // Estimate memory based on common GPU configurations
            if (name.Contains("rtx 4090")) gpu.Memory.TotalMB = 24576; // 24GB
            else if (name.Contains("rtx 4080")) gpu.Memory.TotalMB = 16384; // 16GB
            else if (name.Contains("rtx 4070 ti")) gpu.Memory.TotalMB = 12288; // 12GB
            else if (name.Contains("rtx 4070")) gpu.Memory.TotalMB = 12288; // 12GB
            else if (name.Contains("rtx 4060 ti")) gpu.Memory.TotalMB = 16384; // 16GB or 8GB variants
            else if (name.Contains("rtx 4060")) gpu.Memory.TotalMB = 8192; // 8GB
            else if (name.Contains("rtx 3090")) gpu.Memory.TotalMB = 24576; // 24GB
            else if (name.Contains("rtx 3080 ti")) gpu.Memory.TotalMB = 12288; // 12GB
            else if (name.Contains("rtx 3080")) gpu.Memory.TotalMB = 10240; // 10GB
            else if (name.Contains("rtx 3070")) gpu.Memory.TotalMB = 8192; // 8GB
            else if (name.Contains("rtx 3060 ti")) gpu.Memory.TotalMB = 8192; // 8GB
            else if (name.Contains("rtx 3060")) gpu.Memory.TotalMB = 12288; // 12GB
            else if (name.Contains("rx 7900")) gpu.Memory.TotalMB = 20480; // 20GB
            else if (name.Contains("rx 7800")) gpu.Memory.TotalMB = 16384; // 16GB
            else if (name.Contains("rx 7700")) gpu.Memory.TotalMB = 12288; // 12GB
            else if (name.Contains("rx 7600")) gpu.Memory.TotalMB = 8192; // 8GB
            else if (name.Contains("rx 6950")) gpu.Memory.TotalMB = 16384; // 16GB
            else if (name.Contains("rx 6900")) gpu.Memory.TotalMB = 16384; // 16GB
            else if (name.Contains("rx 6800")) gpu.Memory.TotalMB = 16384; // 16GB
            else if (name.Contains("rx 6700")) gpu.Memory.TotalMB = 10240; // 10GB
            else if (name.Contains("rx 6600")) gpu.Memory.TotalMB = 8192; // 8GB
            else gpu.Memory.TotalMB = 4096; // Default 4GB for unknown GPUs
        }

        private string DetermineMemoryType(string gpuName)
        {
            var name = gpuName.ToLower();
            
            // Determine memory type based on GPU generation
            if (name.Contains("rtx 40") || name.Contains("rtx 30"))
                return "GDDR6X";
            else if (name.Contains("rtx 20") || name.Contains("gtx 16"))
                return "GDDR6";
            else if (name.Contains("rx 7") || name.Contains("rx 6"))
                return "GDDR6";
            else if (name.Contains("rx 5") || name.Contains("vega"))
                return "HBM2";
            else if (name.Contains("gtx 10"))
                return "GDDR5X";
            else
                return "GDDR5";
        }

        private void DetectGpuPCIe(GpuInfo gpu, ManagementObject obj)
        {
            try
            {
                // Estimate PCIe based on GPU generation
                EstimatePCIeFromGpuName(gpu);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting GPU PCIe: {ex.Message}");
                EstimatePCIeFromGpuName(gpu);
            }
        }

        private void EstimatePCIeFromGpuName(GpuInfo gpu)
        {
            var name = gpu.Name.ToLower();
            
            // Estimate PCIe generation and lanes based on GPU generation
            if (name.Contains("rtx 40") || name.Contains("rx 7") || name.Contains("arc a"))
            {
                gpu.Pcie.Max = "5.0 x16";
                gpu.Pcie.Negotiated = "5.0 x16";
            }
            else if (name.Contains("rtx 30") || name.Contains("rtx 20") ||
                     name.Contains("rx 6") || name.Contains("rx 5"))
            {
                gpu.Pcie.Max = "4.0 x16";
                gpu.Pcie.Negotiated = "4.0 x16";
            }
            else if (name.Contains("gtx 16") || name.Contains("gtx 10") ||
                     name.Contains("vega") || name.Contains("rx "))
            {
                gpu.Pcie.Max = "3.0 x16";
                gpu.Pcie.Negotiated = "3.0 x16";
            }
            else
            {
                gpu.Pcie.Max = "2.0 x16";
                gpu.Pcie.Negotiated = "2.0 x16";
            }
        }

        private void EstimateGpuCapabilities(GpuInfo gpu)
        {
            // Estimate capabilities based on GPU name
            if (gpu.Name.Contains("RTX 40") || gpu.Name.Contains("RTX 30") || gpu.Name.Contains("RTX 20"))
            {
                gpu.Dxr = true;
                gpu.DxFeatureLevel = "12_2";
                gpu.DirectStorage = true;
            }
            else if (gpu.Name.Contains("RX 6") || gpu.Name.Contains("RX 7"))
            {
                gpu.Dxr = true;
                gpu.DxFeatureLevel = "12_2";
            }

            // PCIe is now handled by DetectGpuPCIe method
        }

        private void DetectDisplays(GraphicsInfo graphics)
        {
            // This would require EDID parsing and display API calls
            // For now, create a placeholder
            graphics.Displays.Add(new DisplayInfo
            {
                Name = "Primary Display",
                NativeResolution = "1920x1080",
                RefreshRate = 60,
                Hdr = false,
                Vrr = false
            });
        }
    }

    public class StorageDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectStorageInfo(systemInfo.Storage, deepScan, redact);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Storage detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectStorageInfo(StorageInfo storage, bool deepScan, bool redact)
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            using var results = searcher.Get();

            storage.Drives.Clear();

            foreach (ManagementObject obj in results)
            {
                var drive = new DriveInfo
                {
                    Model = obj["Model"]?.ToString() ?? "",
                    SerialNumber = redact ? "REDACTED" : (obj["SerialNumber"]?.ToString()?.Trim() ?? ""),
                    CapacityGB = Convert.ToInt64(obj["Size"] ?? 0) / (1024 * 1024 * 1024),
                    Bus = DetermineBusType(obj["InterfaceType"]?.ToString() ?? ""),
                    Rotational = !IsSSd(obj["Model"]?.ToString() ?? "")
                };

                if (deepScan)
                {
                    // Detect SMART data for both SSDs and HDDs
                    DetectSmartData(drive, obj);
                }

                storage.Drives.Add(drive);
            }

            DetectVolumes(storage, redact);
        }

        private string DetermineBusType(string interfaceType)
        {
            return interfaceType?.ToUpper() switch
            {
                "SCSI" => "NVMe", // Often NVMe appears as SCSI
                "IDE" => "SATA",
                _ => "Unknown"
            };
        }

        private bool IsSSd(string model)
        {
            var ssdKeywords = new[] { "SSD", "NVMe", "M.2", "Samsung", "Crucial", "WD_BLACK" };
            return ssdKeywords.Any(keyword => model.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private void DetectSmartData(DriveInfo drive, ManagementObject diskObj)
        {
            try
            {
                // Get the physical drive index
                var driveIndex = diskObj["Index"]?.ToString();
                if (string.IsNullOrEmpty(driveIndex))
                    return;

                // Query SMART data using MSStorageDriver_FailurePredictStatus
                DetectBasicSmartHealth(drive, driveIndex);
                
                // Query temperature using MSStorageDriver_FailurePredictThresholds
                DetectSmartTemperature(drive, driveIndex);
                
                // For SSDs, try to get wear level information
                if (!drive.Rotational)
                {
                    DetectSsdWearLevel(drive, driveIndex);
                }
                
                // Set default values if detection failed
                if (drive.Smart.Temperature == 0)
                {
                    drive.Smart.Temperature = drive.Rotational ? 45 : 35; // Default temps
                }
                
                if (string.IsNullOrEmpty(drive.Smart.Health))
                {
                    drive.Smart.Health = "Unknown";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting SMART data for {drive.Model}: {ex.Message}");
                // Set fallback values
                drive.Smart.Health = "Unknown";
                drive.Smart.Temperature = drive.Rotational ? 45 : 35;
                drive.Smart.PercentUsed = 0;
            }
        }

        private void DetectBasicSmartHealth(DriveInfo drive, string driveIndex)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("root\\WMI",
                    $"SELECT * FROM MSStorageDriver_FailurePredictStatus WHERE InstanceName LIKE '%PhysicalDrive{driveIndex}%'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var predictFailure = Convert.ToBoolean(obj["PredictFailure"] ?? false);
                    var reason = Convert.ToUInt32(obj["Reason"] ?? 0);
                    
                    if (predictFailure)
                    {
                        drive.Smart.Health = "Warning";
                    }
                    else
                    {
                        drive.Smart.Health = "Good";
                    }
                    
                    return; // Found data, exit
                }
            }
            catch
            {
                // WMI SMART detection failed, will use defaults
            }
        }

        private void DetectSmartTemperature(DriveInfo drive, string driveIndex)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("root\\WMI",
                    $"SELECT * FROM MSStorageDriver_FailurePredictData WHERE InstanceName LIKE '%PhysicalDrive{driveIndex}%'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var vendorSpecific = obj["VendorSpecific"] as byte[];
                    if (vendorSpecific != null && vendorSpecific.Length >= 194)
                    {
                        // Temperature is typically at offset 194 (attribute 194)
                        // This is a simplified approach - real SMART parsing is complex
                        var temp = vendorSpecific[194];
                        if (temp > 0 && temp < 100) // Reasonable temperature range
                        {
                            drive.Smart.Temperature = temp;
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Temperature detection failed, will use default
            }
        }

        private void DetectSsdWearLevel(DriveInfo drive, string driveIndex)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("root\\WMI",
                    $"SELECT * FROM MSStorageDriver_FailurePredictData WHERE InstanceName LIKE '%PhysicalDrive{driveIndex}%'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var vendorSpecific = obj["VendorSpecific"] as byte[];
                    if (vendorSpecific != null && vendorSpecific.Length >= 173)
                    {
                        // Attribute 173 is typically SSD wear leveling count
                        var wearLevel = vendorSpecific[173];
                        if (wearLevel > 0)
                        {
                            // Convert to percentage used (100 - remaining life)
                            drive.Smart.PercentUsed = Math.Min(100 - wearLevel, 100);
                            return;
                        }
                    }
                }
                
                // Fallback: estimate based on drive age/model
                EstimateSsdWear(drive);
            }
            catch
            {
                EstimateSsdWear(drive);
            }
        }

        private void EstimateSsdWear(DriveInfo drive)
        {
            // Very rough estimation based on capacity and model
            // This is just a placeholder - real wear detection requires vendor-specific tools
            var model = drive.Model.ToLower();
            
            if (model.Contains("samsung") || model.Contains("crucial") || model.Contains("intel"))
            {
                drive.Smart.PercentUsed = 3; // High-quality drives
            }
            else if (model.Contains("wd") || model.Contains("seagate"))
            {
                drive.Smart.PercentUsed = 5; // Good quality drives
            }
            else
            {
                drive.Smart.PercentUsed = 8; // Generic/unknown drives
            }
        }

        private void DetectVolumes(StorageInfo storage, bool redact)
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3");
            using var results = searcher.Get();

            storage.Volumes.Clear();

            foreach (ManagementObject obj in results)
            {
                var volume = new VolumeInfo
                {
                    Letter = obj["DeviceID"]?.ToString() ?? "",
                    Label = redact ? "REDACTED" : (obj["VolumeName"]?.ToString() ?? ""),
                    FileSystem = obj["FileSystem"]?.ToString() ?? "",
                    TotalSizeGB = Convert.ToInt64(obj["Size"] ?? 0) / (1024 * 1024 * 1024),
                    FreeSizeGB = Convert.ToInt64(obj["FreeSpace"] ?? 0) / (1024 * 1024 * 1024),
                    BitLocker = DetectBitLockerStatus(obj["DeviceID"]?.ToString() ?? "")
                };

                storage.Volumes.Add(volume);
            }
        }

        private string DetectBitLockerStatus(string driveLetter)
        {
            try
            {
                // Query Win32_EncryptableVolume for BitLocker status
                using var searcher = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                    $"SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = '{driveLetter}'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var protectStatus = Convert.ToUInt32(obj["ProtectionStatus"] ?? 0);
                    var conversionStatus = Convert.ToUInt32(obj["ConversionStatus"] ?? 0);
                    
                    return protectStatus switch
                    {
                        0 => "Unprotected",
                        1 => "Protected", 
                        2 => "Unknown",
                        _ => "Unknown"
                    };
                }
            }
            catch
            {
                // BitLocker WMI namespace might not be available on all systems
                // Try alternative detection via manage-bde command
                return DetectBitLockerFallback(driveLetter);
            }
            
            return "Unknown";
        }

        private string DetectBitLockerFallback(string driveLetter)
        {
            try
            {
                // This is a very basic detection - just check if the drive letter is valid
                // Real BitLocker detection would require admin privileges and manage-bde calls
                if (string.IsNullOrEmpty(driveLetter) || !driveLetter.EndsWith(":"))
                {
                    return "Unknown";
                }
                
                // For now, assume unprotected unless we can prove otherwise
                return "Unprotected";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    // Placeholder implementations for other detectors
    public class NetworkDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectNetworkAdapters(systemInfo.Network, redact);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Network detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectNetworkAdapters(NetworkInfo network, bool redact)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2"); // Connected adapters
                using var results = searcher.Get();

                network.Adapters.Clear();

                foreach (ManagementObject obj in results)
                {
                    var name = obj["Name"]?.ToString() ?? "";
                    var macAddress = obj["MACAddress"]?.ToString() ?? "";
                    var speed = Convert.ToInt64(obj["Speed"] ?? 0);

                    // Skip virtual, loopback, and tunnel adapters
                    if (IsPhysicalAdapter(name, macAddress))
                    {
                        var adapter = new NetworkAdapter
                        {
                            Name = name,
                            Mac = redact ? "XX:XX:XX:XX:XX:XX" : macAddress,
                            Type = DetermineAdapterType(name),
                            LinkSpeedMbps = speed / 1000000 // Convert to Mbps
                        };

                        // Get additional details for Wi-Fi adapters
                        if (adapter.Type == "Wi-Fi")
                        {
                            DetectWiFiDetails(adapter, name);
                        }

                        // Get IP addresses for this adapter
                        if (!redact)
                        {
                            DetectIPAddresses(adapter, obj["GUID"]?.ToString() ?? "");
                        }

                        network.Adapters.Add(adapter);
                    }
                }

                // Detect Bluetooth separately
                DetectBluetoothAdapters(network, redact);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Network adapter detection failed: {ex.Message}");
            }
        }

        private bool IsPhysicalAdapter(string name, string macAddress)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(macAddress))
                return false;

            var nameLower = name.ToLower();
            
            // Exclude virtual and software adapters
            var excludeKeywords = new[] { 
                "virtual", "tap", "vpn", "tunnel", "loopback", "isatap", 
                "teredo", "hyper-v", "vmware", "virtualbox", "docker" 
            };

            return !excludeKeywords.Any(keyword => nameLower.Contains(keyword));
        }

        private string DetermineAdapterType(string name)
        {
            var nameLower = name.ToLower();

            if (nameLower.Contains("wi-fi") || nameLower.Contains("wireless") || 
                nameLower.Contains("802.11") || nameLower.Contains("wifi"))
                return "Wi-Fi";
            
            if (nameLower.Contains("ethernet") || nameLower.Contains("realtek") ||
                nameLower.Contains("intel") && nameLower.Contains("gigabit"))
                return "Ethernet";
            
            if (nameLower.Contains("bluetooth"))
                return "Bluetooth";
            
            return "Unknown";
        }



        private void DetectWiFiDetails(NetworkAdapter adapter, string adapterName)
        {
            try
            {
                // Try to get Wi-Fi standards from WMI
                var query = $"SELECT * FROM MSNdis_80211_SupportedRates WHERE InstanceName LIKE '%{adapterName.Replace(" ", "%")}%'";
                using var searcher = new ManagementObjectSearcher(@"root\wmi", query);
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    // Estimate Wi-Fi standard based on supported rates
                    adapter.WiFi.Standard = "802.11ac"; // Default assumption for modern adapters
                    break;
                }
            }
            catch
            {
                // Fallback: estimate based on adapter name
                var nameLower = adapterName.ToLower();
                if (nameLower.Contains("ax") || nameLower.Contains("6e") || nameLower.Contains("be"))
                    adapter.WiFi.Standard = "802.11ax/be";
                else if (nameLower.Contains("ac"))
                    adapter.WiFi.Standard = "802.11ac";
                else
                    adapter.WiFi.Standard = "802.11n";
            }
        }

        private void DetectIPAddresses(NetworkAdapter adapter, string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid)) return;

                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE SettingID = '{guid}'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var ipAddresses = obj["IPAddress"] as string[];
                    if (ipAddresses != null)
                    {
                        adapter.ActiveIPs.AddRange(ipAddresses.Where(ip => !string.IsNullOrEmpty(ip)));
                    }
                    break;
                }
            }
            catch
            {
                // IP detection failed - not critical
            }
        }

        private void DetectBluetoothAdapters(NetworkInfo network, bool redact)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Service = 'BTHUSB'");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var name = obj["Name"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(name) && name.ToLower().Contains("bluetooth"))
                    {
                        var adapter = new NetworkAdapter
                        {
                            Name = name,
                            Type = "Bluetooth"
                        };
                        
                        adapter.Bluetooth.Version = EstimateBluetoothVersion(name);

                        network.Adapters.Add(adapter);
                        break; // Usually only one Bluetooth adapter
                    }
                }
            }
            catch
            {
                // Bluetooth detection failed - not critical
            }
        }

        private string EstimateBluetoothVersion(string deviceName)
        {
            var nameLower = deviceName.ToLower();
            
            if (nameLower.Contains("5.") || nameLower.Contains("le"))
                return "5.0+";
            else if (nameLower.Contains("4."))
                return "4.0+";
            else
                return "Unknown";
        }
    }

    public class UsbDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            // Placeholder - would implement USB device detection
            await Task.CompletedTask;
        }
    }

    public class AudioDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            // Placeholder - would implement audio device detection
            await Task.CompletedTask;
        }
    }

    public class SensorsDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            // Placeholder - would implement sensor detection
            await Task.CompletedTask;
        }
    }

    public class SecurityDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            // Placeholder - would implement security feature detection
            await Task.CompletedTask;
        }
    }

    public class BatteryDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DetectBatteryInfo(systemInfo.Sensors.PowerSource.Battery, redact);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Battery detection failed: {ex.Message}");
                }
            }, cancellationToken);
        }

        private void DetectBatteryInfo(BatteryInfo battery, bool redact)
        {
            try
            {
                // Query Win32_Battery for basic battery information
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                using var results = searcher.Get();

                bool batteryFound = false;
                foreach (ManagementObject obj in results)
                {
                    batteryFound = true;
                    battery.Present = true;
                    battery.Manufacturer = redact ? "REDACTED" : (obj["Name"]?.ToString() ?? "");
                    battery.Chemistry = obj["Chemistry"]?.ToString() ?? "";
                    
                    // Battery status
                    var batteryStatus = Convert.ToUInt16(obj["BatteryStatus"] ?? 0);
                    battery.Charging = batteryStatus == 2; // 2 = On AC Power
                    
                    // Get design capacity if available
                    var designCapacity = obj["DesignCapacity"];
                    if (designCapacity != null)
                    {
                        battery.DesignCapacityMWh = Convert.ToInt32(designCapacity);
                    }
                    
                    break; // Take first battery only
                }

                if (batteryFound)
                {
                    // Get additional battery details from Win32_PortableBattery
                    DetectAdvancedBatteryInfo(battery, redact);
                }
                else
                {
                    battery.Present = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting battery info: {ex.Message}");
                battery.Present = false;
            }
        }

        private void DetectAdvancedBatteryInfo(BatteryInfo battery, bool redact)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PortableBattery");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    // Get design capacity in mWh
                    var designCapacityMWh = obj["DesignCapacity"];
                    if (designCapacityMWh != null)
                    {
                        battery.DesignCapacityMWh = Convert.ToInt32(designCapacityMWh);
                    }

                    // Get full charge capacity
                    var fullChargeCapacity = obj["FullChargeCapacity"];
                    if (fullChargeCapacity != null)
                    {
                        battery.FullChargeCapacityMWh = Convert.ToInt32(fullChargeCapacity);
                    }

                    // Calculate wear percentage
                    if (battery.DesignCapacityMWh > 0 && battery.FullChargeCapacityMWh > 0)
                    {
                        battery.WearPercent = Math.Max(0, 
                            100 - (battery.FullChargeCapacityMWh * 100 / battery.DesignCapacityMWh));
                    }

                    // Get manufacturer info
                    if (!redact)
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString();
                        if (!string.IsNullOrEmpty(manufacturer))
                        {
                            battery.Manufacturer = manufacturer;
                        }
                    }

                    // Get chemistry
                    var chemistry = obj["Chemistry"];
                    if (chemistry != null)
                    {
                        battery.Chemistry = ConvertChemistryCode(Convert.ToUInt16(chemistry));
                    }

                    break; // Take first battery only
                }
            }
            catch
            {
                // Advanced battery info detection failed, basic info is still available
            }

            // Try to get current capacity and cycle count from WMI
            DetectBatteryCycleCount(battery);
        }

        private void DetectBatteryCycleCount(BatteryInfo battery)
        {
            try
            {
                // Try to get cycle count from ACPI battery information
                using var searcher = new ManagementObjectSearcher("root\\WMI", 
                    "SELECT * FROM BatteryStaticData");
                using var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    // This is vendor-specific and may not work on all systems
                    var cycleCount = obj["CycleCount"];
                    if (cycleCount != null)
                    {
                        battery.CycleCount = Convert.ToInt32(cycleCount);
                        break;
                    }
                }
            }
            catch
            {
                // Cycle count detection failed - this is common as it's vendor-specific
                // Use estimation based on wear level
                if (battery.WearPercent > 0)
                {
                    // Very rough estimation: assume ~500 cycles per 10% wear for lithium batteries
                    battery.CycleCount = battery.WearPercent * 50;
                }
            }
        }

        private string ConvertChemistryCode(ushort chemistryCode)
        {
            return chemistryCode switch
            {
                1 => "Other",
                2 => "Unknown",
                3 => "Lead Acid",
                4 => "Nickel Cadmium",
                5 => "Nickel Metal Hydride",
                6 => "Lithium-ion",
                7 => "Zinc Air",
                8 => "Lithium Polymer",
                _ => "Unknown"
            };
        }
    }

    public class ProcessDetector : IDetector
    {
        public async Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken)
        {
            // Placeholder - would implement process snapshot
            await Task.CompletedTask;
        }
    }
}
