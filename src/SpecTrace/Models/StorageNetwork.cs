using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpecTrace.Models
{
    public class StorageInfo
    {
        [JsonProperty("drives")]
        public List<DriveInfo> Drives { get; set; } = new();

        [JsonProperty("volumes")]
        public List<VolumeInfo> Volumes { get; set; } = new();
    }

    public class DriveInfo
    {
        [JsonProperty("bus")]
        public string Bus { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; } = "";

        [JsonProperty("firmware")]
        public string Firmware { get; set; } = "";

        [JsonProperty("capacityGB")]
        public long CapacityGB { get; set; }

        [JsonProperty("rotational")]
        public bool Rotational { get; set; }

        [JsonProperty("pcie")]
        public string Pcie { get; set; } = "";

        [JsonProperty("smart")]
        public SmartInfo Smart { get; set; } = new();

        [JsonProperty("nvme")]
        public NvmeInfo Nvme { get; set; } = new();

        [JsonProperty("trim")]
        public bool Trim { get; set; }

        [JsonProperty("writeCache")]
        public string WriteCache { get; set; } = "";

        [JsonProperty("directStorage")]
        public bool DirectStorage { get; set; }
    }

    public class SmartInfo
    {
        [JsonProperty("percentUsed")]
        public int PercentUsed { get; set; }

        [JsonProperty("mediaErrors")]
        public long MediaErrors { get; set; }

        [JsonProperty("temperature")]
        public int Temperature { get; set; }

        [JsonProperty("powerCycles")]
        public long PowerCycles { get; set; }

        [JsonProperty("powerOnHours")]
        public long PowerOnHours { get; set; }

        [JsonProperty("health")]
        public string Health { get; set; } = "";

        [JsonProperty("attributes")]
        public List<SmartAttribute> Attributes { get; set; } = new();
    }

    public class SmartAttribute
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("worst")]
        public int Worst { get; set; }

        [JsonProperty("threshold")]
        public int Threshold { get; set; }

        [JsonProperty("raw")]
        public long Raw { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "";
    }

    public class NvmeInfo
    {
        [JsonProperty("pcieGen")]
        public int PcieGen { get; set; }

        [JsonProperty("pcieLanes")]
        public int PcieLanes { get; set; }

        [JsonProperty("namespaces")]
        public int Namespaces { get; set; }

        [JsonProperty("lbaSize")]
        public int LbaSize { get; set; }

        [JsonProperty("volatileWriteCache")]
        public bool VolatileWriteCache { get; set; }

        [JsonProperty("nonVolatileWriteCache")]
        public bool NonVolatileWriteCache { get; set; }
    }

    public class VolumeInfo
    {
        [JsonProperty("letter")]
        public string Letter { get; set; } = "";

        [JsonProperty("label")]
        public string Label { get; set; } = "";

        [JsonProperty("fs")]
        public string FileSystem { get; set; } = "";

        [JsonProperty("clusterSize")]
        public int ClusterSize { get; set; }

        [JsonProperty("bitLocker")]
        public string BitLocker { get; set; } = "";

        [JsonProperty("storageSpaces")]
        public bool StorageSpaces { get; set; }

        [JsonProperty("raid")]
        public string Raid { get; set; } = "";

        [JsonProperty("totalSizeGB")]
        public long TotalSizeGB { get; set; }

        [JsonProperty("freeSizeGB")]
        public long FreeSizeGB { get; set; }
    }

    public class NetworkInfo
    {
        [JsonProperty("adapters")]
        public List<NetworkAdapter> Adapters { get; set; } = new();
    }

    public class NetworkAdapter
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("mac")]
        public string Mac { get; set; } = "";

        [JsonProperty("linkSpeedMbps")]
        public long LinkSpeedMbps { get; set; }

        [JsonProperty("driver")]
        public string Driver { get; set; } = "";

        [JsonProperty("driverVersion")]
        public string DriverVersion { get; set; } = "";

        [JsonProperty("busLocation")]
        public string BusLocation { get; set; } = "";

        [JsonProperty("wifi")]
        public WiFiInfo WiFi { get; set; } = new();

        [JsonProperty("bluetooth")]
        public BluetoothInfo Bluetooth { get; set; } = new();

        [JsonProperty("activeIPs")]
        public List<string> ActiveIPs { get; set; } = new();

        [JsonProperty("dns")]
        public List<string> Dns { get; set; } = new();

        [JsonProperty("vpn")]
        public bool Vpn { get; set; }
    }

    public class WiFiInfo
    {
        [JsonProperty("standard")]
        public string Standard { get; set; } = "";

        [JsonProperty("band")]
        public string Band { get; set; } = "";

        [JsonProperty("channelWidth")]
        public int ChannelWidth { get; set; }

        [JsonProperty("mimo")]
        public string Mimo { get; set; } = "";

        [JsonProperty("rssi")]
        public int Rssi { get; set; }

        [JsonProperty("wpa3")]
        public bool Wpa3 { get; set; }

        [JsonProperty("regulatory")]
        public string Regulatory { get; set; } = "";

        [JsonProperty("phyRates")]
        public string PhyRates { get; set; } = "";
    }

    public class BluetoothInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("controller")]
        public string Controller { get; set; } = "";

        [JsonProperty("codecs")]
        public List<string> Codecs { get; set; } = new();
    }
}
