using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AetherPersonalLauncher.Models
{
    public class VersionManifest
    {
        [JsonProperty("latest")]
        public LatestVersion Latest { get; set; }
        [JsonProperty("versions")]
        public List<VersionInfo>  Versions { get; set; }
    }

    public class LatestVersion
    {
        [JsonProperty("release")]
        public string Release { get; set; }
        [JsonProperty("snapshot")]
        public string Snapshot { get; set; }
    }

    public class VersionInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("time")]
        public DateTime Time { get; set; }
        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }
    }
}