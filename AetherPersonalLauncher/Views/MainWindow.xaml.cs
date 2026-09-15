using System;
using System.Windows;
using System.Windows.Controls;
using AetherPersonalLauncher.Core;

namespace AetherPersonalLauncher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                ClientArea.Content = new HomePage();
            };
            MineCraft.MineCraftStarted += (s, e) =>
            {
                Dispatcher.Invoke(Hide);
            };
            MineCraft.MineCraftExited += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Show();
                    Activate();
                });
            };
        }

        private void CloseAction(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeAction(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void HomePageOpen(object sender, RoutedEventArgs e)
        {
            if (ClientArea == null) return;
            ClientArea.Content = new HomePage();
        }

        private void DownloadPageOpen(object sender, RoutedEventArgs e)
        {
            if (ClientArea == null) return;
            ClientArea.Content = new DownloadPage();
        }
    }
}