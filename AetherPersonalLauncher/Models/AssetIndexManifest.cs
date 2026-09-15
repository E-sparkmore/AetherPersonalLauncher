using System.Collections.Generic;
using Newtonsoft.Json;

namespace AetherPersonalLauncher.Models
{
    public class AssetIndexManifest
    {
        [JsonProperty("objects")] public Dictionary<string, AssetIndexObj> Objects { get; set; }
    }

    public class AssetIndexObj
    {
        [JsonProperty("hash")] public string Hash { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
    }
}