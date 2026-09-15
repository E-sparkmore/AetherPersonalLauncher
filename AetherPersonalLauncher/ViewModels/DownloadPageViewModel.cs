using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AetherPersonalLauncher.Core;
using AetherPersonalLauncher.Models;
using AetherPersonalLauncher.Utils;

namespace AetherPersonalLauncher.ViewModels
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private ObservableCollection<VersionInfo> Releases { get; } = new ObservableCollection<VersionInfo>();
        private ObservableCollection<VersionInfo> Snapshot { get; } = new ObservableCollection<VersionInfo>();
        private ObservableCollection<VersionInfo> OldBeta { get; } = new ObservableCollection<VersionInfo>();
        private ObservableCollection<VersionInfo> OldAlpha { get; } = new ObservableCollection<VersionInfo>();

        private ObservableCollection<VersionInfo> _current;

        public ObservableCollection<VersionInfo> Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        private Visibility _progressBarVisibility = Visibility.Visible;

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _versionList = Visibility.Visible;

        public Visibility VersionListVisibility
        {
            get => _versionList;
            set
            {
                _versionList = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                ProgressBarVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                VersionListVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged();
            }
        }
        
        private bool _isDownloadEnable = true;

        public bool IsDownloadEnable
        {
            get => _isDownloadEnable;
            set
            {
                _isDownloadEnable = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndeterminate = true;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public DownloadPageViewModel()
        {
            Current = Releases;
            ReleaseBtnClicked = new RelayCommand(ReleaseBtnClickedExecute);
            SnapshotBtnClicked = new RelayCommand(SnapshotBtnClickedExecute);
            OldBetaBtnClicked = new RelayCommand(OldBetaBtnClickedExecute);
            OldAlphaBtnClicked = new RelayCommand(OldAlphaBtnClickedExecute);
            DownloadClicked = new RelayCommand<VersionInfo>(DownloadClickedExecute);
            if (Downloader.Manifest == null)
            {
                IsLoading = true;
                Downloader.ManifestDownloadCompleted += GetVersionManifestCallback;
            }
            else
            {
                InitializeProperties();
            }

            Downloader.DownloadProgressChanged += DownloaderOnDownloadProgressChanged;
            MineCraft.MineCraftStarted += (s, e) => IsDownloadEnable = false;
            MineCraft.MineCraftExited += (s, e) => IsDownloadEnable = true;
        }

        private void DownloaderOnDownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Progress = e.Progress;
            IsIndeterminate = e.IsIndeterminate;
        }

        private void GetVersionManifestCallback(object s, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(InitializeProperties);
            Downloader.ManifestDownloadCompleted -= GetVersionManifestCallback;
        }

        private void InitializeProperties()
        {
            IsLoading = Downloader.IsDownloading;
            foreach (var version in Downloader.Manifest.Versions)
            {
                switch (version.Type)
                {
                    case "release":
                        Releases.Add(version);
                        break;
                    case "snapshot":
                        Snapshot.Add(version);
                        break;
                    case "old_beta":
                        OldBeta.Add(version);
                        break;
                    case "old_alpha":
                        OldAlpha.Add(version);
                        break;
                }
            }
        }

        public ICommand ReleaseBtnClicked { get; set; }
        public ICommand SnapshotBtnClicked { get; set; }
        public ICommand OldBetaBtnClicked { get; set; }
        public ICommand OldAlphaBtnClicked { get; set; }
        public ICommand DownloadClicked { get; set; }

        private double _progress;

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        private void ReleaseBtnClickedExecute() => Current = Releases;
        private void SnapshotBtnClickedExecute() => Current = Snapshot;
        private void OldBetaBtnClickedExecute() => Current = OldBeta;
        private void OldAlphaBtnClickedExecute() => Current = OldAlpha;

        private void DownloadClickedExecute(VersionInfo version)
        {
            Config.CurrentVersion = version.Id;
            IsLoading = true;
            Task.Run(async () =>
            {
                try
                {
                    await Downloader.DownloadMineCraft(version, new CancellationToken(false));
                    Application.Current.Dispatcher.Invoke(() => IsLoading = false);
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"下载失败{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsLoading = false;
                    });
                }
            });
        }
    }
}