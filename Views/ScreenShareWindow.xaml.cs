using System.Windows;
using System.Windows.Input;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for ScreenShareWindow.xaml
    /// </summary>
    public partial class ScreenShareWindow : Window
    {
        private bool _isPaused = false;
        private bool _isSharing = false;

        public ScreenShareWindow()
        {
            InitializeComponent();
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSharing)
            {
                var result = MessageBox.Show(
                    "Đang trình chiếu, bạn có chắc muốn dừng không?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            
            this.Close();
        }

        private void ShareScreen_Click(object sender, RoutedEventArgs e)
        {
            StartSharing("Toàn màn hình");
        }

        private void ShareWindow_Click(object sender, RoutedEventArgs e)
        {
            StartSharing("Cửa sổ");
        }

        private void ShareRegion_Click(object sender, RoutedEventArgs e)
        {
            StartSharing("Vùng chọn");
        }

        private void StartSharing(string mode)
        {
            _isSharing = true;
            PreviewPlaceholder.Visibility = Visibility.Collapsed;
            ViewerPanel.Visibility = Visibility.Visible;
            
            MessageBox.Show(
                $"Bắt đầu chia sẻ: {mode}\nHọc sinh sẽ thấy nội dung của bạn.",
                "Trình chiếu",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused;
            // TODO: Update button icon and pause/resume sharing
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn dừng trình chiếu?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _isSharing = false;
                PreviewPlaceholder.Visibility = Visibility.Visible;
                ViewerPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
    }
}
