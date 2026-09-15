using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AetherPersonalLauncher.Models;
using Newtonsoft.Json;

namespace AetherPersonalLauncher.Core
{
    public static class Downloader
    {
        private static readonly HttpClient Client = new HttpClient();

        private const string VersionManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest.json";
        public static event EventHandler<ProgressEventArgs> DownloadProgressChanged;
        public static event EventHandler ManifestDownloadCompleted;
        public static VersionManifest Manifest { get; private set; }
        public static bool IsDownloading { get; private set; }

        private static List<DownloaderManifestItem> _manifest;

        public static readonly string RootPath = ".minecraft";
        public static readonly string AssetsPath = Path.Combine(RootPath, "assets");
        private static readonly string IndexesPath = Path.Combine(AssetsPath, "indexes");
        private static readonly string ObjectsPath = Path.Combine(AssetsPath, "objects");
        private static readonly string LibrariesPath = Path.Combine(RootPath, "libraries");
        public static readonly string VersionsPath = Path.Combine(RootPath, "versions");
        private static readonly string LoggingPath = Path.Combine(AssetsPath, "log_configs");
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(10);

        static Downloader()
        {
            ServicePointManager.DefaultConnectionLimit = 16;
            Client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36");
            Client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            Task.Run(GetVersionManifest);
        }

        private static Task<string> GetStringAsync(string url)
        {
            return Client.GetStringAsync(url);
        }

        private static async Task GetFileAsync(string url, string filePath, CancellationToken token)
        {
            using (var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                    }
                }
            }
        }

        private static async Task GetVersionManifest()
        {
            try
            {
                var result = await GetStringAsync(Downloader.VersionManifestUrl);
                Manifest = JsonConvert.DeserializeObject<VersionManifest>(result);
                ManifestDownloadCompleted?.Invoke(null, EventArgs.Empty);
            }
            catch (HttpRequestException)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("获取版本列表失败", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                });
            }
        }

        private static void InitialLocalDirectory(string ver)
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
                Config.CurrentVersion = "无";
            }

            Directory.CreateDirectory(RootPath);
            Directory.CreateDirectory(AssetsPath);
            Directory.CreateDirectory(IndexesPath);
            Directory.CreateDirectory(ObjectsPath);
            Directory.CreateDirectory(LibrariesPath);
            Directory.CreateDirectory(VersionsPath);
            Directory.CreateDirectory(LoggingPath);
            Directory.CreateDirectory(Path.Combine(VersionsPath, ver));
            Directory.CreateDirectory(Path.Combine(VersionsPath, ver, $"{ver}-natives"));
        }

        private static async Task<List<DownloaderManifestItem>> GetMineCraftFileManifest(VersionInfo ver,
            string manifestPath)
        {
            var manifest = new List<DownloaderManifestItem>();
            var version = await GetStringAsync(ver.Url);
            var components = JsonConvert.DeserializeObject<VersionComponents>(version);
            var index = await GetStringAsync(components.AssetIndex.Url);
            var assetIndex = JsonConvert.DeserializeObject<AssetIndexManifest>(index);

            InitialLocalDirectory(components.Id);
            File.WriteAllText(Path.Combine(IndexesPath, $"{components.Id}.json"), index);
            File.WriteAllText(Path.Combine(RootPath, "version.json"), version);

            foreach (var item in assetIndex.Objects)
            {
                var relativePath = Path.Combine(item.Value.Hash.Substring(0, 2), item.Value.Hash);
                manifest.Add(new DownloaderManifestItem()
                {
                    Url =
                        $"https://resources.download.minecraft.net/{item.Value.Hash.Substring(0, 2)}/{item.Value.Hash}",
                    Path = Path.Combine(ObjectsPath, relativePath),
                    State = FileState.Waiting,
                    Type = FileType.Obj
                });
            }

            manifest.Add(new DownloaderManifestItem()
            {
                Url = components.Downloads.Client.Url,
                Path = Path.Combine(VersionsPath, components.Id, $"{components.Id}.jar"),
                State = FileState.Waiting,
                Type = FileType.Client
            });

            foreach (var item in components.Libraries)
            {
                if (item.Rules == null || item.Rules.Any(rule =>
                        (rule.Action == null || (rule.Action == "allow" &&
                                                 (rule.Os == null || (rule.Os.Name == OsEnum.Windows &&
                                                                      (rule.Os.VersionRange == null ||
                                                                       (rule.Os.VersionRange.MinVersion == null ||
                                                                        Convert.ToInt32(rule.Os.VersionRange.MinVersion
                                                                            .Split('.')[0]) <= 10) &&
                                                                       (rule.Os.VersionRange.MaxVersion == null ||
                                                                        Convert.ToInt32(rule.Os.VersionRange.MaxVersion
                                                                            .Split('.')[0]) >= 11)) && (
                                                                          !item.Downloads.Artifact.Url.Contains(
                                                                              "native-windows") ||
                                                                          item.Downloads.Artifact.Url.EndsWith(
                                                                              "native-windows.jar"))
                                                     )))) &&
                        string.IsNullOrEmpty(rule.Arch)))
                {
                    string filePath = Path.Combine(LibrariesPath, item.Downloads.Artifact.PathInfo).Replace("\\", "/");

                    manifest.Add(new DownloaderManifestItem()
                    {
                        Url = item.Downloads.Artifact.Url,
                        Path = filePath,
                        State = FileState.Waiting,
                        Type = item.Rules != null &&
                               item.Rules.Any(rule =>
                                   rule.Os != null && rule.Os != null && rule.Os.Name == OsEnum.Windows &&
                                   item.Downloads.Artifact.Url.EndsWith("native-windows.jar"))
                            ? FileType.NativeLibrary
                            : FileType.Lib
                    });
                }
            }

            if (components.Logging != null && components.Logging.Client != null)
            {
                manifest.Add(new DownloaderManifestItem()
                {
                    Url = components.Logging.Client.FileInfo.Url,
                    Path = Path.Combine(LoggingPath, components.Logging.Client.FileInfo.Id),
                    State = FileState.Waiting,
                    Type = FileType.Log
                });
            }

            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));

            return manifest;
        }

        public static async Task DownloadMineCraft(VersionInfo ver, CancellationToken token)
        {
            IsDownloading = true;
            DownloadProgressChanged?.Invoke(null, new ProgressEventArgs(0, true));
            string manifestPath = Path.Combine(Environment.CurrentDirectory, RootPath, $"FileManiFest_{ver.Id}.json");
            if (File.Exists(manifestPath))
            {
                _manifest = JsonConvert.DeserializeObject<List<DownloaderManifestItem>>(File.ReadAllText(manifestPath));
            }
            else
            {
                _manifest = await GetMineCraftFileManifest(ver, manifestPath);
            }

            var tasks = new List<Task>();
            int done = 0;
            int total = _manifest.Count;
            foreach (var item in _manifest)
            {
                var localItem = item;
                tasks.Add(Task.Run(async () =>
                {
                    await Semaphore.WaitAsync(token);
                    try
                    {
                        if (localItem.State == FileState.Waiting || localItem.State == FileState.Downloading ||
                            localItem.State == FileState.Failed ||
                            (localItem.State == FileState.Downloaded &&
                             !File.Exists(Path.Combine(Environment.CurrentDirectory, localItem.Path))))
                        {
                            if (Path.GetDirectoryName(localItem.Path) is string path)
                            {
                                localItem.State = FileState.Downloading;
                                Directory.CreateDirectory(path);
                                await GetFileAsync(localItem.Url,
                                    Path.Combine(Environment.CurrentDirectory, localItem.Path),
                                    token);
                                localItem.State = FileState.Downloaded;
                            }
                            else
                            {
                                throw new DirectoryNotFoundException("找不到文件夹");
                            }
                        }

                        int current = Interlocked.Increment(ref done);
                        DownloadProgressChanged?.Invoke(null, new ProgressEventArgs((double)current / total));
                    }
                    catch (Exception)
                    {
                        localItem.State = FileState.Failed;
                    }
                    finally
                    {
                        Semaphore.Release();
                    }
                }, token));
            }

            await Task.WhenAll(tasks);

            foreach (var item in _manifest.Where(item => item.Type == FileType.NativeLibrary))
            {
                var localItem = item;
                if (localItem.State == FileState.Downloaded)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        ZipArchive archive =
                            ZipFile.OpenRead(Path.Combine(Environment.CurrentDirectory, localItem.Path));
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.StartsWith("windows/x64") && entry.FullName.EndsWith(".dll"))
                            {
                                string destPath = Path.GetFullPath(Path.Combine(VersionsPath, ver.Id,
                                    $"{ver.Id}-natives", "java", entry.Name));
                                if (!File.Exists(destPath) && Path.GetDirectoryName(destPath) is string destDir)
                                {
                                    Directory.CreateDirectory(destDir);
                                    entry.ExtractToFile(destPath, true);
                                }
                            }
                        }

                        archive.Dispose();
                    }, token));
                }
            }

            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(_manifest));
            _manifest = null;
            IsDownloading = false;
        }

        public static void SaveManifest()
        {
            if (Config.CurrentVersion == "无" || !IsDownloading || _manifest == null) return;
            string manifestPath = Path.Combine(Environment.CurrentDirectory, RootPath,
                $"FileManiFest_{Config.CurrentVersion}.json");
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(_manifest));
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; }
        public bool IsIndeterminate { get; }

        public ProgressEventArgs(double progress, bool isIndeterminate = false)
        {
            Progress = progress;
            IsIndeterminate = isIndeterminate;
        }
    }
}