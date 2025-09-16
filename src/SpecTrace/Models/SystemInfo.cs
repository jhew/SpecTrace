using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpecTrace.Models
{
    public class SystemInfo
    {
        [JsonProperty("machine")]
        public MachineInfo Machine { get; set; } = new();

        [JsonProperty("cpu")]
        public CpuInfo Cpu { get; set; } = new();

        [JsonProperty("memory")]
        public MemoryInfo Memory { get; set; } = new();

        [JsonProperty("graphics")]
        public GraphicsInfo Graphics { get; set; } = new();

        [JsonProperty("storage")]
        public StorageInfo Storage { get; set; } = new();

        [JsonProperty("network")]
        public NetworkInfo Network { get; set; } = new();

        [JsonProperty("usb")]
        public UsbInfo Usb { get; set; } = new();

        [JsonProperty("audio")]
        public AudioInfo Audio { get; set; } = new();

        [JsonProperty("sensors")]
        public SensorsInfo Sensors { get; set; } = new();

        [JsonProperty("security")]
        public SecurityInfo Security { get; set; } = new();

        [JsonProperty("processes")]
        public ProcessInfo Processes { get; set; } = new();

        [JsonProperty("generatedAt")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("redacted")]
        public bool Redacted { get; set; }
    }

    public class MachineInfo
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; } = "";

        [JsonProperty("os")]
        public string Os { get; set; } = "";

        [JsonProperty("build")]
        public int Build { get; set; }

        [JsonProperty("secureBoot")]
        public bool SecureBoot { get; set; }

        [JsonProperty("tpm")]
        public string Tpm { get; set; } = "";

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; } = "";
    }

    public class CpuInfo
    {
        [JsonProperty("vendor")]
        public string Vendor { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("stepping")]
        public string Stepping { get; set; } = "";

        [JsonProperty("microcode")]
        public string Microcode { get; set; } = "";

        [JsonProperty("family")]
        public int Family { get; set; }

        [JsonProperty("modelNum")]
        public int ModelNum { get; set; }

        [JsonProperty("cores")]
        public CpuCores Cores { get; set; } = new();

        [JsonProperty("flags")]
        public List<string> Flags { get; set; } = new();

        [JsonProperty("npu")]
        public NpuInfo Npu { get; set; } = new();

        [JsonProperty("cache")]
        public CpuCache Cache { get; set; } = new();

        [JsonProperty("clocks")]
        public CpuClocks Clocks { get; set; } = new();

        [JsonProperty("power")]
        public CpuPower Power { get; set; } = new();
    }

    public class CpuCores
    {
        [JsonProperty("p")]
        public int P { get; set; }

        [JsonProperty("e")]
        public int E { get; set; }

        [JsonProperty("threads")]
        public int Threads { get; set; }

        [JsonProperty("numa")]
        public int Numa { get; set; }

        [JsonProperty("threadDirector")]
        public bool ThreadDirector { get; set; }
    }

    public class NpuInfo
    {
        [JsonProperty("present")]
        public bool Present { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("tops")]
        public float Tops { get; set; }

        [JsonProperty("driverVersion")]
        public string DriverVersion { get; set; } = "";
    }

    public class CpuCache
    {
        [JsonProperty("l1d")]
        public int L1D { get; set; }

        [JsonProperty("l1i")]
        public int L1I { get; set; }

        [JsonProperty("l2")]
        public int L2 { get; set; }

        [JsonProperty("l3")]
        public int L3 { get; set; }
    }

    public class CpuClocks
    {
        [JsonProperty("baseMHz")]
        public int BaseMHz { get; set; }

        [JsonProperty("boostMHz")]
        public int BoostMHz { get; set; }

        [JsonProperty("currentMHz")]
        public int CurrentMHz { get; set; }

        [JsonProperty("utilization")]
        public float Utilization { get; set; }
    }

    public class CpuPower
    {
        [JsonProperty("tdp")]
        public int Tdp { get; set; }

        [JsonProperty("pl1")]
        public int Pl1 { get; set; }

        [JsonProperty("pl2")]
        public int Pl2 { get; set; }

        [JsonProperty("currentWatts")]
        public float CurrentWatts { get; set; }

        [JsonProperty("thermalThrottling")]
        public bool ThermalThrottling { get; set; }
    }
}
