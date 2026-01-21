using System.Windows;
using ClassroomManagement.Services;
using ClassroomManagement.Models;
using System.Linq;

namespace ClassroomManagement.Views
{
    public partial class SystemConfigTableWindow : Window
    {
        public SystemConfigTableWindow()
        {
            InitializeComponent();
            SpecsGrid.ItemsSource = SessionManager.Instance.OnlineStudentsSystemInfo;

            // Auto request specs when opening
            _ = SessionManager.Instance.RequestAllSystemSpecsAsync();
        }

        private async void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            await SessionManager.Instance.RequestAllSystemSpecsAsync();
        }

        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SystemInfoPackage info)
            {
                var detailWindow = new ComputerSpecsWindow(info);
                detailWindow.Owner = this;
                detailWindow.ShowDialog();
            }
        }
    }
}
