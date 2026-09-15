using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace AetherPersonalLauncher.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OsEnum
    {
        [EnumMember(Value = "windows")] Windows,
        [EnumMember(Value = "linux")] Linux,
        [EnumMember(Value = "osx")] MacOs
    }

    public class VersionComponents
    {
        [JsonProperty("arguments")] public ArgumentsClass Arguments { get; set; }
        [JsonProperty("assetIndex")] public DownloadInfoClass AssetIndex { get; set; }
        [JsonProperty("assets")] public string Assets { get; set; }
        [JsonProperty("complianceLevel")] public int ComplianceLevel { get; set; }
        [JsonProperty("downloads")] public ServerClientDownloadInfoClass Downloads { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("javaVersion")] public JavaVersionClass JavaVersion { get; set; }
        [JsonProperty("libraries")] public List<LibraryClass> Libraries { get; set; }
        [JsonProperty("logging")] public LoggingClass Logging { get; set; }
        [JsonProperty("mainClass")] public string MainClass { get; set; }
        [JsonProperty("minecraftArguments")] public string MinecraftArguments { get; set; }
        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion { get; set; }
        [JsonProperty("releaseTime")] public DateTime ReleaseTime { get; set; }
        [JsonProperty("time")] public DateTime Time { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }

    public class ArgumentsClass
    {
        [JsonProperty("default-user-jvm")] public List<ArgumentClass> DefaultUserJvmList { get; set; }
        [JsonProperty("game")] public List<JToken> Game { get; set; }
        [JsonProperty("jvm")] public List<JToken> Jvm { get; set; }
    }

    public class ArgumentClass
    {
        [JsonProperty("rules")] public List<RuleClass> Rules { get; set; }
        [JsonProperty("value")] public List<string> Value { get; set; }
    }

    public class RuleClass
    {
        [JsonProperty("action")] public string Action { get; set; }
        [JsonProperty("os")] public OsClass Os { get; set; }
        [JsonProperty("arch")] public string Arch { get; set; }
    }

    public class OsClass
    {
        [JsonProperty("name")] public OsEnum Name { get; set; }
        [JsonProperty("versionRange")] public VersionRangeClass VersionRange { get; set; }
    }

    public class VersionRangeClass
    {
        [JsonProperty("min")] public string MinVersion { get; set; }
        [JsonProperty("max")] public string MaxVersion { get; set; }
    }

    public class DownloadInfoClass
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("sha1")] public string Sha1 { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
        [JsonProperty("totalSize")] public long TotalSize { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("path")] public string PathInfo { get; set; }
    }

    public class ServerClientDownloadInfoClass
    {
        [JsonProperty("client")] public DownloadInfoClass Client { get; set; }
        [JsonProperty("server")] public DownloadInfoClass Server { get; set; }
    }

    public class JavaVersionClass
    {
        [JsonProperty("component")] public string Component { get; set; }
        [JsonProperty("majorVersion")] public int MajorVersion { get; set; }
    }

    public class LibraryDownloadInfo
    {
        [JsonProperty("artifact")] public DownloadInfoClass Artifact { get; set; }
    }

    public class LibraryClass
    {
        [JsonProperty("downloads")] public LibraryDownloadInfo Downloads { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("rules")] public List<RuleClass> Rules { get; set; }
    }

    public class LoggingClass
    {
        [JsonProperty("client")] public LoggingClientClass Client { get; set; }
    }

    public class LoggingClientClass
    {
        [JsonProperty("argument")] public string Argument { get; set; }
        [JsonProperty("file")] public DownloadInfoClass FileInfo { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }
}