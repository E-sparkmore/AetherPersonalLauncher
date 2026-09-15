using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AetherPersonalLauncher.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileState
    {
        [EnumMember(Value = "Waiting")] Waiting,
        [EnumMember(Value = "Downloading")] Downloading,
        [EnumMember(Value = "Downloaded")] Downloaded,
        [EnumMember(Value = "Failed")] Failed
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileType
    {
        [EnumMember(Value = "object")] Obj,
        [EnumMember(Value = "library")] Lib,
        [EnumMember(Value = "binary")] NativeLibrary,
        [EnumMember(Value = "client")] Client,
        [EnumMember(Value = "log")] Log
    }

    public class DownloaderManifestItem
    {
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("state")] public FileState State { get; set; }
        [JsonProperty("type")] public FileType Type { get; set; }
    }
}