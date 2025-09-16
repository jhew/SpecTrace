using System;
using System.Text;
using SpecTrace.Models;
using Newtonsoft.Json;

namespace SpecTrace.Core
{
    public class DataExporter
    {
        public string ToJson(SystemInfo systemInfo)
        {
            return JsonConvert.SerializeObject(systemInfo, Formatting.Indented);
        }

        public string ToMarkdown(SystemInfo systemInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# System Information Report");
            sb.AppendLine();
            sb.AppendLine($"**Generated:** {systemInfo.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"**Redacted:** {systemInfo.Redacted}");
            sb.AppendLine();

            // Machine Info
            sb.AppendLine("## Machine");
            sb.AppendLine($"- **Hostname:** {systemInfo.Machine.Hostname}");
            sb.AppendLine($"- **OS:** {systemInfo.Machine.Os}");
            sb.AppendLine($"- **Build:** {systemInfo.Machine.Build}");
            sb.AppendLine($"- **Manufacturer:** {systemInfo.Machine.Manufacturer}");
            sb.AppendLine($"- **Model:** {systemInfo.Machine.Model}");
            sb.AppendLine($"- **Secure Boot:** {(systemInfo.Machine.SecureBoot ? "✅" : "❌")}");
            sb.AppendLine($"- **TPM:** {systemInfo.Machine.Tpm}");
            sb.AppendLine();

            // CPU Info
            sb.AppendLine("## CPU");
            sb.AppendLine($"- **Vendor:** {systemInfo.Cpu.Vendor}");
            sb.AppendLine($"- **Model:** {systemInfo.Cpu.Model}");
            sb.AppendLine($"- **Cores:** {systemInfo.Cpu.Cores.P}P + {systemInfo.Cpu.Cores.E}E / {systemInfo.Cpu.Cores.Threads}T");
            sb.AppendLine($"- **Base Clock:** {systemInfo.Cpu.Clocks.BaseMHz} MHz");
            sb.AppendLine($"- **Boost Clock:** {systemInfo.Cpu.Clocks.BoostMHz} MHz");
            sb.AppendLine($"- **Cache L3:** {systemInfo.Cpu.Cache.L3} KB");
            sb.AppendLine($"- **NPU:** {(systemInfo.Cpu.Npu.Present ? $"Yes ({systemInfo.Cpu.Npu.Tops} TOPS)" : "No")}");
            if (systemInfo.Cpu.Flags.Count > 0)
            {
                sb.AppendLine($"- **Features:** {string.Join(", ", systemInfo.Cpu.Flags)}");
            }
            sb.AppendLine();

            // Memory Info
            sb.AppendLine("## Memory");
            sb.AppendLine($"- **Total:** {systemInfo.Memory.TotalBytes / (1024 * 1024 * 1024)} GB");
            sb.AppendLine($"- **Channels:** {systemInfo.Memory.Channels}");
            sb.AppendLine($"- **Speed:** {systemInfo.Memory.SpeedMTps} MT/s");
            sb.AppendLine($"- **Timings:** {systemInfo.Memory.Timings}");
            sb.AppendLine($"- **Profile:** {systemInfo.Memory.Profile}");
            
            if (systemInfo.Memory.Dimms.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("### Memory Modules");
                foreach (var dimm in systemInfo.Memory.Dimms)
                {
                    sb.AppendLine($"- **{dimm.Slot}:** {dimm.SizeGB}GB {dimm.Type} @ {dimm.SpeedMTps} MT/s ({dimm.Manufacturer} {dimm.PartNumber})");
                }
            }
            sb.AppendLine();

            // Graphics Info
            if (systemInfo.Graphics.Gpus.Count > 0)
            {
                sb.AppendLine("## Graphics");
                foreach (var gpu in systemInfo.Graphics.Gpus)
                {
                    sb.AppendLine($"- **GPU:** {gpu.Name}");
                    sb.AppendLine($"  - **Driver:** {gpu.Driver}");
                    sb.AppendLine($"  - **PCIe:** {gpu.Pcie.Negotiated}");
                    sb.AppendLine($"  - **DXR:** {(gpu.Dxr ? "✅" : "❌")}");
                    sb.AppendLine($"  - **DirectStorage:** {(gpu.DirectStorage ? "✅" : "❌")}");
                }
                sb.AppendLine();
            }

            // Storage Info
            if (systemInfo.Storage.Drives.Count > 0)
            {
                sb.AppendLine("## Storage");
                foreach (var drive in systemInfo.Storage.Drives)
                {
                    sb.AppendLine($"- **{drive.Model}** ({drive.CapacityGB}GB {drive.Bus})");
                    if (!drive.Rotational && drive.Smart.Health != "")
                    {
                        sb.AppendLine($"  - **Health:** {drive.Smart.Health} ({drive.Smart.PercentUsed}% used)");
                        sb.AppendLine($"  - **Temperature:** {drive.Smart.Temperature}°C");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ToForumSummary(SystemInfo systemInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("**System Specifications**");
            sb.AppendLine();
            
            // CPU
            var cpuCores = systemInfo.Cpu.Cores.E > 0 ? 
                $"{systemInfo.Cpu.Cores.P}P+{systemInfo.Cpu.Cores.E}E / {systemInfo.Cpu.Cores.Threads}T" :
                $"{systemInfo.Cpu.Cores.P}C / {systemInfo.Cpu.Cores.Threads}T";
            
            sb.AppendLine($"**CPU**: {systemInfo.Cpu.Model} ({cpuCores})");
            if (systemInfo.Cpu.Npu.Present)
                sb.AppendLine($"**NPU**: Yes ({systemInfo.Cpu.Npu.Tops} TOPS)");

            // Memory
            var memoryGb = systemInfo.Memory.TotalBytes / (1024 * 1024 * 1024);
            sb.AppendLine($"**Memory**: {memoryGb}GB DDR5-{systemInfo.Memory.SpeedMTps} {systemInfo.Memory.Profile}");

            // Graphics
            foreach (var gpu in systemInfo.Graphics.Gpus)
            {
                sb.AppendLine($"**GPU**: {gpu.Name} (Driver {gpu.Driver})");
                if (gpu.Dxr) sb.Append(", DXR: Yes");
                sb.AppendLine();
            }

            // Storage summary
            if (systemInfo.Storage.Drives.Count > 0)
            {
                sb.Append("**Storage**: ");
                var driveList = new System.Collections.Generic.List<string>();
                foreach (var drive in systemInfo.Storage.Drives)
                {
                    driveList.Add($"{drive.CapacityGB}GB {(drive.Rotational ? "HDD" : "SSD")}");
                }
                sb.AppendLine(string.Join(", ", driveList));
            }

            // Security
            var securityFeatures = new System.Collections.Generic.List<string>();
            if (systemInfo.Machine.SecureBoot) securityFeatures.Add("Secure Boot ✅");
            if (systemInfo.Machine.Tpm == "2.0") securityFeatures.Add("TPM 2.0 ✅");
            if (securityFeatures.Count > 0)
            {
                sb.AppendLine($"**Security**: {string.Join(", ", securityFeatures)}");
            }

            // OS
            sb.AppendLine($"**OS**: {systemInfo.Machine.Os} (Build {systemInfo.Machine.Build})");

            return sb.ToString();
        }

        public string ToHtml(SystemInfo systemInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"utf-8\">");
            sb.AppendLine("    <title>SpecTrace System Report</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', sans-serif; margin: 40px; }");
            sb.AppendLine("        h1 { color: #2563eb; }");
            sb.AppendLine("        h2 { color: #1f2937; border-bottom: 2px solid #e5e7eb; padding-bottom: 8px; }");
            sb.AppendLine("        .info-grid { display: grid; grid-template-columns: 200px 1fr; gap: 8px; }");
            sb.AppendLine("        .info-label { font-weight: bold; }");
            sb.AppendLine("        .status-ok { color: #059669; }");
            sb.AppendLine("        .status-warn { color: #d97706; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            sb.AppendLine("<h1>SpecTrace System Report</h1>");
            sb.AppendLine($"<p><strong>Generated:</strong> {systemInfo.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
            
            // Convert markdown content to HTML (simplified)
            var markdownContent = ToMarkdown(systemInfo);
            var htmlContent = markdownContent
                .Replace("## ", "<h2>")
                .Replace("\n**", "\n<strong>")
                .Replace(":**", ":</strong>")
                .Replace("✅", "<span class=\"status-ok\">✅</span>")
                .Replace("❌", "<span class=\"status-warn\">❌</span>")
                .Replace("\n", "<br>\n");
            
            sb.AppendLine(htmlContent);
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        public string ToDxDiagText(SystemInfo systemInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("System Information");
            sb.AppendLine("------------------");
            sb.AppendLine($"Machine Name: {systemInfo.Machine.Hostname}");
            sb.AppendLine($"Operating System: {systemInfo.Machine.Os}");
            sb.AppendLine($"System Manufacturer: {systemInfo.Machine.Manufacturer}");
            sb.AppendLine($"System Model: {systemInfo.Machine.Model}");
            sb.AppendLine($"BIOS: {systemInfo.Machine.Tpm}");
            sb.AppendLine($"Processor: {systemInfo.Cpu.Model}");
            sb.AppendLine($"Memory: {systemInfo.Memory.TotalBytes / (1024 * 1024)} MB RAM");
            sb.AppendLine();

            sb.AppendLine("Display Devices");
            sb.AppendLine("---------------");
            foreach (var gpu in systemInfo.Graphics.Gpus)
            {
                sb.AppendLine($"Card name: {gpu.Name}");
                sb.AppendLine($"Manufacturer: {gpu.Vendor}");
                sb.AppendLine($"Driver Version: {gpu.Driver}");
                sb.AppendLine($"DirectX Version: {gpu.DxFeatureLevel}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
