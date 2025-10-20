using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32;
using SpecTrace.Core;
using SpecTrace.Models;
using System.IO;

namespace SpecTrace.Views
{
    public partial class MainWindow : Window
    {
        private SystemScanner _scanner;
        private DataExporter _exporter;
        private SystemInfo? _currentSystemInfo;
        private bool _isDarkTheme = false;
        
        // Original values for redaction toggle
        private string? _originalHostname;
        private string? _originalSerialNumber;

        public MainWindow()
        {
            InitializeComponent();
            _scanner = new SystemScanner();
            _exporter = new DataExporter();
            
            // Initialize with light theme
            ApplyTheme(false);
            
            // Perform initial quick scan
            _ = PerformScanAsync(false);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformScanAsync(ScanModeText.Text.Contains("Deep"));
        }

        private async void QuickScanButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformScanAsync(false);
        }

        private async void DeepScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if running as admin for deep scan
            if (!IsRunAsAdmin())
            {
                var result = MessageBox.Show(
                    "Deep scan requires administrator privileges for full hardware access. Continue with limited deep scan?",
                    "Administrator Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes)
                    return;
            }
            
            await PerformScanAsync(true);
        }

        private async Task PerformScanAsync(bool deepScan)
        {
            try
            {
                StatusText.Text = deepScan ? "Performing deep scan..." : "Performing quick scan...";
                ScanModeText.Text = deepScan ? "Deep Scan" : "Quick Scan";
                
                // Disable buttons during scan
                SetButtonsEnabled(false);

                _currentSystemInfo = await _scanner.ScanAsync(deepScan, RedactCheckBox.IsChecked ?? false);
                
                UpdateUI(_currentSystemInfo);
                
                StatusText.Text = "Scan completed";
                LastUpdateText.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Scan failed: {ex.Message}";
                MessageBox.Show($"Scan failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void UpdateUI(SystemInfo systemInfo)
        {
            // Update CPU tab
            CpuVendorText.Text = systemInfo.Cpu.Vendor;
            CpuModelText.Text = systemInfo.Cpu.Model;
            
            if (systemInfo.Cpu.Cores.E > 0)
            {
                CpuCoresText.Text = $"{systemInfo.Cpu.Cores.P}P + {systemInfo.Cpu.Cores.E}E cores, {systemInfo.Cpu.Cores.Threads} threads";
            }
            else
            {
                CpuCoresText.Text = $"{systemInfo.Cpu.Cores.P} cores, {systemInfo.Cpu.Cores.Threads} threads";
            }
            
            CpuBaseClockText.Text = $"{systemInfo.Cpu.Clocks.BaseMHz} MHz";
            CpuBoostClockText.Text = $"{systemInfo.Cpu.Clocks.BoostMHz} MHz";
            CpuCacheText.Text = systemInfo.Cpu.Cache.L3 > 0 ? $"{systemInfo.Cpu.Cache.L3} KB" : "Unknown";
            CpuNpuText.Text = systemInfo.Cpu.Npu.Present ? 
                $"Yes ({systemInfo.Cpu.Npu.Vendor}, {systemInfo.Cpu.Npu.Tops} TOPS)" : "No";
            CpuFeaturesText.Text = systemInfo.Cpu.Flags.Any() ? 
                string.Join(", ", systemInfo.Cpu.Flags) : "None detected";
            CpuTdpText.Text = systemInfo.Cpu.Power.Tdp > 0 ? $"{systemInfo.Cpu.Power.Tdp}W" : "Unknown";

            // Update Motherboard tab
            MoboManufacturerText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.Manufacturer) ? 
                systemInfo.Machine.Motherboard.Manufacturer : "Unknown";
            MoboModelText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.Model) ? 
                systemInfo.Machine.Motherboard.Model : "Unknown";
            MoboVersionText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.Version) ? 
                systemInfo.Machine.Motherboard.Version : "Not available";
            MoboSerialText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.SerialNumber) ? 
                systemInfo.Machine.Motherboard.SerialNumber : "Not available";
            BiosVendorText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.BiosVendor) ? 
                systemInfo.Machine.Motherboard.BiosVendor : "Unknown";
            BiosVersionText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.BiosVersion) ? 
                systemInfo.Machine.Motherboard.BiosVersion : "Unknown";
            BiosDateText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Motherboard.BiosDate) ? 
                systemInfo.Machine.Motherboard.BiosDate : "Unknown";
            MoboFormFactorText.Text = EstimateFormFactor(systemInfo.Machine.Model);

            // Update Memory tab
            MemoryTotalText.Text = $"{systemInfo.Memory.TotalBytes / (1024 * 1024 * 1024)} GB";
            MemoryChannelsText.Text = systemInfo.Memory.Channels.ToString();
            MemorySpeedText.Text = $"{systemInfo.Memory.SpeedMTps} MT/s";
            MemoryTimingsText.Text = systemInfo.Memory.Timings;
            MemoryProfileText.Text = systemInfo.Memory.Profile;
            MemoryModulesGrid.ItemsSource = systemInfo.Memory.Dimms;

            // Update Graphics tab
            GraphicsGrid.ItemsSource = systemInfo.Graphics.Gpus;

            // Update Storage tab
            StorageGrid.ItemsSource = systemInfo.Storage.Drives;

            // Update Security tab
            SecureBootText.Text = systemInfo.Machine.SecureBoot ? "✅ Enabled" : "❌ Disabled";
            TpmText.Text = !string.IsNullOrEmpty(systemInfo.Machine.Tpm) ? 
                $"✅ {systemInfo.Machine.Tpm}" : "❌ Not available";
            VbsText.Text = systemInfo.Security.Vbs ? "✅ Enabled" : "❌ Disabled";
            HvciText.Text = systemInfo.Security.Hvci ? "✅ Enabled" : "❌ Disabled";
            CredentialGuardText.Text = systemInfo.Security.CredentialGuard ? "✅ Enabled" : "❌ Disabled";

            // Update Summary tab
            SummaryTextBox.Text = _exporter.ToForumSummary(systemInfo);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            RefreshButton.IsEnabled = enabled;
            QuickScanButton.IsEnabled = enabled;
            DeepScanButton.IsEnabled = enabled;
            ExportButton.IsEnabled = enabled;
        }

        private bool IsRunAsAdmin()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo == null)
            {
                MessageBox.Show("No system information available. Please run a scan first.", 
                    "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export System Information",
                Filter = "JSON Files (*.json)|*.json|HTML Files (*.html)|*.html|Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string content = Path.GetExtension(dialog.FileName).ToLower() switch
                    {
                        ".json" => _exporter.ToJson(_currentSystemInfo),
                        ".html" => _exporter.ToHtml(_currentSystemInfo),
                        ".md" => _exporter.ToMarkdown(_currentSystemInfo),
                        ".txt" => _exporter.ToDxDiagText(_currentSystemInfo),
                        _ => _exporter.ToJson(_currentSystemInfo)
                    };

                    File.WriteAllText(dialog.FileName, content);
                    StatusText.Text = $"Exported to {dialog.FileName}";
                    
                    MessageBox.Show($"System information exported successfully to:\n{dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Export Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme(_isDarkTheme);
            ThemeToggleButton.Content = _isDarkTheme ? "☀️ Light" : "🌙 Dark";
            StatusText.Text = $"Switched to {(_isDarkTheme ? "dark" : "light")} theme";
        }

        private void ApplyTheme(bool isDarkTheme)
        {
            try
            {
                // Clear existing theme resources from both Application and Window
                var existingAppTheme = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme.xaml") == true);
                
                if (existingAppTheme != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(existingAppTheme);
                }

                var existingWindowTheme = this.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme.xaml") == true);
                
                if (existingWindowTheme != null)
                {
                    this.Resources.MergedDictionaries.Remove(existingWindowTheme);
                }

                // Load the new theme
                var themeUri = new Uri($"pack://application:,,,/Themes/{(isDarkTheme ? "Dark" : "Light")}Theme.xaml");
                var newTheme = new ResourceDictionary { Source = themeUri };
                
                // Apply to both Application and Window level
                Application.Current.Resources.MergedDictionaries.Add(newTheme);
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });

                // Force UI refresh by invalidating visual
                this.InvalidateVisual();
                this.UpdateLayout();
            }
            catch (Exception ex)
            {
                // Use a fallback status update method in case StatusText is not available
                try
                {
                    StatusText.Text = $"Error applying theme: {ex.Message}";
                }
                catch
                {
                    // Fallback - set title bar to show error
                    this.Title = $"SpecTrace - Theme Error: {ex.Message}";
                }
            }
        }







        // Copy section methods
        private void CopyCpuSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var cpuInfo = $"""
                    CPU Information:
                    Vendor: {_currentSystemInfo.Cpu.Vendor}
                    Model: {_currentSystemInfo.Cpu.Model}
                    Cores: {CpuCoresText.Text}
                    Base Clock: {CpuBaseClockText.Text}
                    Boost Clock: {CpuBoostClockText.Text}
                    Cache L3: {CpuCacheText.Text}
                    NPU: {CpuNpuText.Text}
                    Features: {CpuFeaturesText.Text}
                    """;
                Clipboard.SetText(cpuInfo);
                StatusText.Text = "CPU information copied to clipboard";
            }
        }

        private void CopyMemorySection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var memoryInfo = $"""
                    Memory Information:
                    Total: {MemoryTotalText.Text}
                    Channels: {MemoryChannelsText.Text}
                    Speed: {MemorySpeedText.Text}
                    Timings: {MemoryTimingsText.Text}
                    Profile: {MemoryProfileText.Text}
                    """;
                Clipboard.SetText(memoryInfo);
                StatusText.Text = "Memory information copied to clipboard";
            }
        }

        private void CopyMotherboardSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var moboInfo = $"""
                    Motherboard Information:
                    Manufacturer: {MoboManufacturerText.Text}
                    Model: {MoboModelText.Text}
                    Version: {MoboVersionText.Text}
                    Serial Number: {MoboSerialText.Text}
                    BIOS Vendor: {BiosVendorText.Text}
                    BIOS Version: {BiosVersionText.Text}
                    BIOS Date: {BiosDateText.Text}
                    Form Factor: {MoboFormFactorText.Text}
                    """;
                Clipboard.SetText(moboInfo);
                StatusText.Text = "Motherboard information copied to clipboard";
            }
        }

        private void CopyGraphicsSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var gpuInfo = string.Join("\n", _currentSystemInfo.Graphics.Gpus.Select(gpu => 
                    $"GPU: {gpu.Name} ({gpu.Vendor}) - Driver: {gpu.Driver}"));
                Clipboard.SetText($"Graphics Information:\n{gpuInfo}");
                StatusText.Text = "Graphics information copied to clipboard";
            }
        }

        private void CopyStorageSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var storageInfo = string.Join("\n", _currentSystemInfo.Storage.Drives.Select(drive => 
                    $"Drive: {drive.Model} ({drive.CapacityGB}GB {drive.Bus})"));
                Clipboard.SetText($"Storage Information:\n{storageInfo}");
                StatusText.Text = "Storage information copied to clipboard";
            }
        }

        private void CopySecuritySection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo != null)
            {
                var securityInfo = $"""
                    Security Information:
                    Secure Boot: {SecureBootText.Text}
                    TPM: {TpmText.Text}
                    VBS: {VbsText.Text}
                    HVCI: {HvciText.Text}
                    Credential Guard: {CredentialGuardText.Text}
                    """;
                Clipboard.SetText(securityInfo);
                StatusText.Text = "Security information copied to clipboard";
            }
        }

        private void CopySummary_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SummaryTextBox.Text))
            {
                Clipboard.SetText(SummaryTextBox.Text);
                StatusText.Text = "System summary copied to clipboard";
            }
        }

        private void RedactCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_currentSystemInfo == null) return;

            bool shouldRedact = RedactCheckBox.IsChecked ?? false;
            ApplyRedactionToCurrentData(shouldRedact);
            UpdateUI(_currentSystemInfo);
            
            StatusText.Text = shouldRedact ? "PII redaction enabled" : "PII redaction disabled";
        }

        private void ApplyRedactionToCurrentData(bool redact)
        {
            if (_currentSystemInfo == null) return;

            // Store redaction state
            _currentSystemInfo.Redacted = redact;

            // Apply redaction to machine info
            if (redact)
            {
                // Store original values if not already stored
                if (!_currentSystemInfo.Redacted)
                {
                    _originalHostname = _currentSystemInfo.Machine.Hostname;
                    _originalSerialNumber = _currentSystemInfo.Machine.SerialNumber;
                }
                
                _currentSystemInfo.Machine.Hostname = "REDACTED";
                _currentSystemInfo.Machine.SerialNumber = "REDACTED";
            }
            else
            {
                // Restore original values
                _currentSystemInfo.Machine.Hostname = _originalHostname ?? Environment.MachineName;
                _currentSystemInfo.Machine.SerialNumber = _originalSerialNumber ?? "";
            }

            // Apply redaction to memory modules
            if (_currentSystemInfo.Memory?.Dimms != null)
            {
                foreach (var dimm in _currentSystemInfo.Memory.Dimms)
                {
                    if (redact)
                    {
                        dimm.SerialNumber = "REDACTED";
                    }
                    // Note: We can't easily restore original serial numbers without re-scanning
                    // This is a limitation of the current architecture
                }
            }

            // Apply redaction to storage drives
            if (_currentSystemInfo.Storage?.Drives != null)
            {
                foreach (var drive in _currentSystemInfo.Storage.Drives)
                {
                    if (redact)
                    {
                        drive.SerialNumber = "REDACTED";
                    }
                }
            }

            // Apply redaction to storage volumes
            if (_currentSystemInfo.Storage?.Volumes != null)
            {
                foreach (var volume in _currentSystemInfo.Storage.Volumes)
                {
                    if (redact)
                    {
                        volume.Label = "REDACTED";
                    }
                }
            }

            // Apply redaction to network adapters
            if (_currentSystemInfo.Network?.Adapters != null)
            {
                foreach (var adapter in _currentSystemInfo.Network.Adapters)
                {
                    if (redact)
                    {
                        adapter.Mac = "XX:XX:XX:XX:XX:XX";
                    }
                }
            }
        }

        private string EstimateFormFactor(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return "Unknown";

            var model = modelName.ToLower();

            // Desktop form factors
            if (model.Contains("mini-itx") || model.Contains("mini itx"))
                return "Mini-ITX";
            if (model.Contains("micro-atx") || model.Contains("micro atx") || model.Contains("matx"))
                return "Micro-ATX";
            if (model.Contains("e-atx") || model.Contains("eatx") || model.Contains("extended atx"))
                return "E-ATX";
            if (model.Contains("atx"))
                return "ATX";

            // Laptop/Mobile form factors
            if (model.Contains("laptop") || model.Contains("notebook") || model.Contains("ultrabook"))
                return "Laptop";
            if (model.Contains("all-in-one") || model.Contains("aio"))
                return "All-in-One";

            // Server form factors
            if (model.Contains("server") || model.Contains("rack"))
                return "Server/Rack";

            // Check for common laptop/desktop indicators
            if (model.Contains("latitude") || model.Contains("thinkpad") || model.Contains("elitebook") ||
                model.Contains("pavilion") || model.Contains("inspiron") || model.Contains("vivobook"))
                return "Laptop";

            // Default based on generic system names
            if (model.Contains("system product"))
                return "Desktop (ATX/Micro-ATX)";

            return "Unknown";
        }
    }

    // Helper converters
    public static class Converters
    {
        public static readonly System.Windows.Data.IValueConverter InverseBooleanConverter = 
            new InverseBooleanValueConverter();
    }

    public class InverseBooleanValueConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, 
            System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, 
            System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
