using System.Windows;
using AetherPersonalLauncher.Core;

namespace AetherPersonalLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            MineCraft.StopMineCraft();
            Config.Save();
            Downloader.SaveManifest();
        }
    }
}