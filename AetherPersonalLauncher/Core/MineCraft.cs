using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AetherPersonalLauncher.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AetherPersonalLauncher.Core
{
    public static class MineCraft
    {
        public static event EventHandler MineCraftStarted;
        public static event EventHandler MineCraftExited;
        private static Process MineCraftProcess { get; set; }
        private static ProcessStartInfo MineCraftProcessInfo { get; set; }

        private static bool _isRunning;

        public static int StartMineCraftWaitForExit()
        {
            if (_isRunning) return -1;
            MineCraftProcess = new Process
            {
                StartInfo =  MineCraftProcessInfo
            };
            MineCraftProcess.Start();
            MineCraftStarted?.Invoke(null, null);
            _isRunning = true;
            MineCraftProcess.WaitForExit();
            _isRunning = false;
            MineCraftExited?.Invoke(null, null);
            return MineCraftProcess.ExitCode;
        }

        public static void StopMineCraft()
        {
            if (MineCraftProcess == null || !_isRunning) return;
            MineCraftProcess.Kill();
        }

        public static void InitMineCraftProcess(string userName)
        {
            MineCraftProcessInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = PrepareArguments(userName),
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        private static string PrepareArguments(string userName)
        {
            var manifestPath = Path.Combine(Environment.CurrentDirectory, Downloader.RootPath,
                $"FileManiFest_{Config.CurrentVersion}.json");
            var versionPath = Path.Combine(Environment.CurrentDirectory, Downloader.RootPath, "version.json");
            string text = File.ReadAllText(manifestPath);
            List<DownloaderManifestItem> manifest =
                JsonConvert.DeserializeObject<List<DownloaderManifestItem>>(text);
            text = File.ReadAllText(versionPath);
            VersionComponents components = JsonConvert.DeserializeObject<VersionComponents>(text);
            foreach (var item in manifest)
            {
                if (item.State != FileState.Downloaded)
                {
                    throw new FileNotFoundException("文件不完整");
                }
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Clear();
            if (components.Arguments != null)
            {
                if (components.Arguments.Jvm != null)
                {
                    foreach (var item in components.Arguments.Jvm)
                    {
                        if (item.Type == JTokenType.String)
                        {
                            sb.Append(item);
                            sb.Append(" ");
                        }
                        else if (item.Type == JTokenType.Object)
                        {
                            if (item["value"] == null) continue;
                            if (item["rules"] == null || JsonConvert.DeserializeObject<List<RuleClass>>(
                                    item["rules"]
                                        .ToString()).Any(rule =>
                                    rule == null ||
                                    (rule.Action == "allow" &&
                                     (rule.Os == null || rule.Os.Name == OsEnum.Windows) &&
                                     (rule.Arch == null || rule.Arch != "x86"))))
                            {
                                if (item["value"].Type == JTokenType.String)
                                {
                                    sb.Append(item["value"]);
                                    sb.Append(" ");
                                }
                                else if (item["value"].Type == JTokenType.Array)
                                {
                                    List<string> values =
                                        JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                                    var args = string.Join(" ", values);
                                    sb.Append(args);
                                    sb.Append(" ");
                                }
                            }
                        }
                    }
                }

                if (components.Arguments.DefaultUserJvmList != null)
                {
                    _ = components.Arguments.DefaultUserJvmList.Where(item => item.Rules.Any(rule => rule == null ||
                        (rule.Action == "allow" &&
                         (rule.Os == null || rule.Os.Name == OsEnum.Windows) &&
                         (rule.Arch == null || rule.Arch != "x86")))
                    ).Select<ArgumentClass, object>(arg =>
                    {
                        var args = string.Join(" ", arg.Value);
                        sb.Append(args);
                        sb.Append(" ");
                        return arg;
                    });
                }

                sb.Append(components.MainClass);
                sb.Append(" ");

                if (components.Arguments.Game != null)
                {
                    foreach (var item in components.Arguments.Game.Where(item => item.Type == JTokenType.String))
                    {
                        sb.Append(item);
                        sb.Append(" ");
                    }
                }
            }

            if (components.MinecraftArguments != null)
            {
                sb.Append(components.MinecraftArguments);
                sb.Append(" ");
            }
            var nativesDirectory = Path.Combine(Environment.CurrentDirectory, Downloader.VersionsPath,
                Config.CurrentVersion, $"{Config.CurrentVersion}-natives");
            sb.Replace("${natives_directory}", nativesDirectory.Replace("\\", "/"));
            sb.Replace("${launcher_name}", "AetherMCLauncher");
            sb.Replace("${launcher_version}", "1.0.0");
            sb.Replace("${auth_player_name}", userName);
            sb.Replace("${version_name}", Config.CurrentVersion);
            sb.Replace("${game_directory}",
                Path.Combine(Downloader.VersionsPath, Config.CurrentVersion).Replace("\\", "/"));
            sb.Replace("${assets_root}", Downloader.AssetsPath.Replace("\\", "/"));
            sb.Replace("${assets_index_name}", Config.CurrentVersion);
            sb.Replace("${auth_access_token}", "0");
            sb.Replace("${version_type}", "Terraria");
            sb.Replace("${auth_uuid}", "00000000000000000000000000000000");

            string classPath = "\"" + string.Join(";",
                manifest.Where(item => item.Type == FileType.Lib).Select(item =>
                    Path.Combine(Environment.CurrentDirectory, item.Path)
                )) + ";" + Path.Combine(Environment.CurrentDirectory, Downloader.VersionsPath, Config.CurrentVersion,
                $"{Config.CurrentVersion}.jar") + "\"";

            sb.Replace("${classpath}", classPath.Replace("\\", "/"));
            return sb.ToString();
        }
    }
}