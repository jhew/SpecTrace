using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpecTrace.Models
{
    public class MemoryInfo
    {
        [JsonProperty("totalBytes")]
        public long TotalBytes { get; set; }

        [JsonProperty("channels")]
        public int Channels { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; } = "";

        [JsonProperty("speedMTps")]
        public int SpeedMTps { get; set; }

        [JsonProperty("timings")]
        public string Timings { get; set; } = "";

        [JsonProperty("dimms")]
        public List<DimmInfo> Dimms { get; set; } = new();

        [JsonProperty("interleave")]
        public string Interleave { get; set; } = "";

        [JsonProperty("commandRate")]
        public string CommandRate { get; set; } = "";

        [JsonProperty("gearMode")]
        public string GearMode { get; set; } = "";
    }

    public class DimmInfo
    {
        [JsonProperty("slot")]
        public string Slot { get; set; } = "";

        [JsonProperty("sizeGB")]
        public int SizeGB { get; set; }

        [JsonProperty("speedMTps")]
        public int SpeedMTps { get; set; }

        [JsonProperty("timings")]
        public string Timings { get; set; } = "";

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("partNumber")]
        public string PartNumber { get; set; } = "";

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; } = "";

        [JsonProperty("ranks")]
        public int Ranks { get; set; }

        [JsonProperty("ecc")]
        public bool Ecc { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "";
    }

    public class GraphicsInfo
    {
        [JsonProperty("gpus")]
        public List<GpuInfo> Gpus { get; set; } = new();

        [JsonProperty("displays")]
        public List<DisplayInfo> Displays { get; set; } = new();
    }

    public class GpuInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("vendor")]
        public string Vendor { get; set; } = "";

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } = "";

        [JsonProperty("driver")]
        public string Driver { get; set; } = "";

        [JsonProperty("driverDate")]
        public string DriverDate { get; set; } = "";

        [JsonProperty("whql")]
        public bool Whql { get; set; }

        [JsonProperty("pcie")]
        public PcieInfo Pcie { get; set; } = new();

        [JsonProperty("memory")]
        public GpuMemory Memory { get; set; } = new();

        [JsonProperty("dxr")]
        public bool Dxr { get; set; }

        [JsonProperty("directStorage")]
        public bool DirectStorage { get; set; }

        [JsonProperty("dxFeatureLevel")]
        public string DxFeatureLevel { get; set; } = "";

        [JsonProperty("vulkan")]
        public string Vulkan { get; set; } = "";

        [JsonProperty("openCL")]
        public string OpenCL { get; set; } = "";

        [JsonProperty("resizableBar")]
        public bool ResizableBar { get; set; }
    }

    public class PcieInfo
    {
        [JsonProperty("max")]
        public string Max { get; set; } = "";

        [JsonProperty("negotiated")]
        public string Negotiated { get; set; } = "";

        [JsonProperty("generation")]
        public int Generation { get; set; }

        [JsonProperty("lanes")]
        public int Lanes { get; set; }
    }

    public class GpuMemory
    {
        [JsonProperty("totalMB")]
        public long TotalMB { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; } = "";

        [JsonProperty("busWidth")]
        public int BusWidth { get; set; }
    }

    public class DisplayInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("edidSerial")]
        public string EdidSerial { get; set; } = "";

        [JsonProperty("nativeResolution")]
        public string NativeResolution { get; set; } = "";

        [JsonProperty("refreshRate")]
        public int RefreshRate { get; set; }

        [JsonProperty("hdr")]
        public bool Hdr { get; set; }

        [JsonProperty("vrr")]
        public bool Vrr { get; set; }

        [JsonProperty("colorSpace")]
        public string ColorSpace { get; set; } = "";

        [JsonProperty("currentMode")]
        public string CurrentMode { get; set; } = "";

        [JsonProperty("connection")]
        public string Connection { get; set; } = "";
    }
}
