using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpecTrace.Models
{
    public class UsbInfo
    {
        [JsonProperty("controllers")]
        public List<UsbController> Controllers { get; set; } = new();
    }

    public class UsbController
    {
        [JsonProperty("standard")]
        public string Standard { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("devices")]
        public List<UsbDevice> Devices { get; set; } = new();

        [JsonProperty("usb4Domain")]
        public bool Usb4Domain { get; set; }

        [JsonProperty("thunderbolt")]
        public ThunderboltInfo Thunderbolt { get; set; } = new();
    }

    public class UsbDevice
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("vendorId")]
        public string VendorId { get; set; } = "";

        [JsonProperty("productId")]
        public string ProductId { get; set; } = "";

        [JsonProperty("speed")]
        public string Speed { get; set; } = "";

        [JsonProperty("tunneledPCIe")]
        public bool TunneledPCIe { get; set; }

        [JsonProperty("authorized")]
        public bool Authorized { get; set; }

        [JsonProperty("securityLevel")]
        public string SecurityLevel { get; set; } = "";
    }

    public class ThunderboltInfo
    {
        [JsonProperty("present")]
        public bool Present { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("securityLevel")]
        public string SecurityLevel { get; set; } = "";

        [JsonProperty("authorizedDevices")]
        public int AuthorizedDevices { get; set; }
    }

    public class AudioInfo
    {
        [JsonProperty("defaultRender")]
        public string DefaultRender { get; set; } = "";

        [JsonProperty("defaultCapture")]
        public string DefaultCapture { get; set; } = "";

        [JsonProperty("devices")]
        public List<AudioDevice> Devices { get; set; } = new();

        [JsonProperty("spatial")]
        public List<string> Spatial { get; set; } = new();
    }

    public class AudioDevice
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("driver")]
        public string Driver { get; set; } = "";

        [JsonProperty("provider")]
        public string Provider { get; set; } = "";

        [JsonProperty("supportedFormats")]
        public List<string> SupportedFormats { get; set; } = new();

        [JsonProperty("maxSampleRate")]
        public int MaxSampleRate { get; set; }

        [JsonProperty("channels")]
        public int Channels { get; set; }
    }

    public class SensorsInfo
    {
        [JsonProperty("temps")]
        public Dictionary<string, int> Temps { get; set; } = new();

        [JsonProperty("fans")]
        public Dictionary<string, int> Fans { get; set; } = new();

        [JsonProperty("voltages")]
        public Dictionary<string, float> Voltages { get; set; } = new();

        [JsonProperty("powerSource")]
        public PowerSource PowerSource { get; set; } = new();
    }

    public class PowerSource
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("battery")]
        public BatteryInfo Battery { get; set; } = new();

        [JsonProperty("powerPlan")]
        public string PowerPlan { get; set; } = "";

        [JsonProperty("modernStandby")]
        public bool ModernStandby { get; set; }

        [JsonProperty("processorStates")]
        public ProcessorStates ProcessorStates { get; set; } = new();
    }

    public class BatteryInfo
    {
        [JsonProperty("present")]
        public bool Present { get; set; }

        [JsonProperty("designCapacityMWh")]
        public int DesignCapacityMWh { get; set; }

        [JsonProperty("fullChargeCapacityMWh")]
        public int FullChargeCapacityMWh { get; set; }

        [JsonProperty("currentCapacityMWh")]
        public int CurrentCapacityMWh { get; set; }

        [JsonProperty("cycleCount")]
        public int CycleCount { get; set; }

        [JsonProperty("wearPercent")]
        public int WearPercent { get; set; }

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("chemistry")]
        public string Chemistry { get; set; } = "";

        [JsonProperty("charging")]
        public bool Charging { get; set; }
    }

    public class ProcessorStates
    {
        [JsonProperty("minPercent")]
        public int MinPercent { get; set; }

        [JsonProperty("maxPercent")]
        public int MaxPercent { get; set; }

        [JsonProperty("throttlePercent")]
        public int ThrottlePercent { get; set; }
    }

    public class SecurityInfo
    {
        [JsonProperty("vbs")]
        public bool Vbs { get; set; }

        [JsonProperty("hvci")]
        public bool Hvci { get; set; }

        [JsonProperty("credentialGuard")]
        public bool CredentialGuard { get; set; }

        [JsonProperty("coreIsolation")]
        public bool CoreIsolation { get; set; }

        [JsonProperty("hypervisor")]
        public string Hypervisor { get; set; } = "";

        [JsonProperty("wsl2")]
        public bool Wsl2 { get; set; }

        [JsonProperty("virtualBox")]
        public bool VirtualBox { get; set; }

        [JsonProperty("vmware")]
        public bool Vmware { get; set; }

        [JsonProperty("pcHealthCheck")]
        public PcHealthCheck PcHealthCheck { get; set; } = new();
    }

    public class PcHealthCheck
    {
        [JsonProperty("tpm2")]
        public bool Tpm2 { get; set; }

        [JsonProperty("secureBoot")]
        public bool SecureBoot { get; set; }

        [JsonProperty("uefi")]
        public bool Uefi { get; set; }

        [JsonProperty("cpu")]
        public bool Cpu { get; set; }

        [JsonProperty("memory")]
        public bool Memory { get; set; }

        [JsonProperty("storage")]
        public bool Storage { get; set; }
    }

    public class ProcessInfo
    {
        [JsonProperty("top")]
        public List<ProcessSnapshot> Top { get; set; } = new();

        [JsonProperty("captureTime")]
        public System.DateTime CaptureTime { get; set; }
    }

    public class ProcessSnapshot
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("pid")]
        public int Pid { get; set; }

        [JsonProperty("cpuPct")]
        public float CpuPct { get; set; }

        [JsonProperty("memMB")]
        public long MemMB { get; set; }

        [JsonProperty("startupImpact")]
        public string StartupImpact { get; set; } = "";
    }
}
