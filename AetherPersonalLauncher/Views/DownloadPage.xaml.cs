using System.Windows.Controls;

namespace AetherPersonalLauncher.Views
{
    public partial class DownloadPage : UserControl
    {
        public DownloadPage()
        {
            InitializeComponent();
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Current" && ViewModel.Current.Count > 0)
                {
                    VersionInfos.ScrollIntoView(ViewModel.Current[0]);
                }
            };
        }
    }
}