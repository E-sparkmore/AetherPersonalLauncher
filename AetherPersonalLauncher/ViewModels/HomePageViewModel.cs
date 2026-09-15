using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AetherPersonalLauncher.Core;
using AetherPersonalLauncher.Utils;

namespace AetherPersonalLauncher.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private string _currentVersion = Config.CurrentVersion;
        private readonly DispatcherTimer _tipTimer = new DispatcherTimer();
        private const string ButtonTip = "开  始  游  戏";

        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                _currentVersion = value;
                OnPropertyChanged();
            }
        }

        private string _currentUserName = Config.CurrentUser;

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                Config.CurrentUser = value;
                OnPropertyChanged();
            }
        }

        private bool _isStartEnabled = true;

        public bool IsStartEnabled
        {
            get => _isStartEnabled;
            set
            {
                _isStartEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _tip = ButtonTip;

        public string Tip
        {
            get => _tip;
            set
            {
                _tip = value;
                OnPropertyChanged();
            }
        }

        private void ShowTip(string message)
        {
            Tip = message;
            IsStartEnabled = false;
            _tipTimer.Start();
        }

        void TipRecover(object s, EventArgs e)
        {
            _tipTimer.Stop();
            IsStartEnabled = true;
            Tip = ButtonTip;
        }

        public HomePageViewModel()
        {
            _tipTimer.Interval = TimeSpan.FromSeconds(3);
            _tipTimer.Tick += TipRecover;
            Config.CurrentVersionChanged += (sender, arg) => { CurrentVersion = arg; };
            StartGame = new RelayCommand(StartGame_Execute);
        }

        public ICommand StartGame { get; set; }

        private void StartGame_Execute()
        {
            IsStartEnabled = false;

            if (CurrentVersion == "无")
            {
                ShowTip("无版本");
                return;
            }
            
            if (string.IsNullOrEmpty(CurrentUserName))
            {
                ShowTip("用户名为空");
                return;
            }

            if (Downloader.IsDownloading)
            {
                ShowTip("下载中...");
                return;
            }
            try
            {
                MineCraft.InitMineCraftProcess(CurrentUserName);
                Task.Run(() =>
                {
                    var exitCode = MineCraft.StartMineCraftWaitForExit();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (exitCode)
                        {
                            case 0:
                                break;
                            case -1:
                                ShowTip("正在运行");
                                break;
                            default:
                                ShowTip("非正常退出");
                                break;
                        }
                    });
                });
            }
            catch (FileNotFoundException)
            {
                ShowTip("文件不完整");
            }
            catch (DirectoryNotFoundException)
            {
                ShowTip("路径缺失");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}