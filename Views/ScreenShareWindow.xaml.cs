using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for ScreenShareWindow.xaml
    /// </summary>
    public partial class ScreenShareWindow : Window
    {
        private readonly SessionManager _session;
        private readonly ScreenCaptureService _screenCapture;
        private readonly LogService _log = LogService.Instance;
        
        private bool _isPaused = false;
        private bool _isSharing = false;
        private CancellationTokenSource? _shareCts;
        private DispatcherTimer? _previewTimer;
        
        // Screen share mode
        private enum ShareMode { FullScreen, Window, Region }
        private ShareMode _currentMode = ShareMode.FullScreen;
        private System.Drawing.Rectangle? _selectedRegion;

        public ScreenShareWindow()
        {
            InitializeComponent();
            
            _session = SessionManager.Instance;
            _screenCapture = new ScreenCaptureService();
            
            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Update viewer count
            UpdateViewerCount();
            
            // Subscribe to student changes
            _session.OnlineStudents.CollectionChanged += (s, args) => UpdateViewerCount();
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isSharing)
            {
                var result = MessageBox.Show(
                    "Đang trình chiếu, bạn có chắc muốn dừng không?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                
                StopSharing();
            }
        }

        private void UpdateViewerCount()
        {
            Dispatcher.Invoke(() =>
            {
                var count = _session.OnlineStudents.Count;
                // Update the viewer count text in toolbar
                // Find and update the viewer count display
            });
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
            Close();
        }

        #region Screen Share Modes

        private void ShareScreen_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = ShareMode.FullScreen;
            StartSharing();
        }

        private void ShareWindow_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = ShareMode.Window;
            // TODO: Show window selection dialog
            MessageBox.Show(
                "Tính năng chọn cửa sổ sẽ được thêm trong phiên bản sau.\nĐang chia sẻ toàn màn hình...",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            _currentMode = ShareMode.FullScreen;
            StartSharing();
        }

        private void ShareRegion_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = ShareMode.Region;
            // TODO: Show region selection overlay
            MessageBox.Show(
                "Tính năng chọn vùng sẽ được thêm trong phiên bản sau.\nĐang chia sẻ toàn màn hình...",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            _currentMode = ShareMode.FullScreen;
            StartSharing();
        }

        #endregion

        #region Screen Sharing Logic

        private void StartSharing()
        {
            if (_isSharing) return;

            try
            {
                // Check if network server is running
                if (_session.NetworkServer == null || !_session.IsRunning)
                {
                    MessageBox.Show(
                        "Chưa khởi động phiên học.\nVui lòng bắt đầu phiên học trước khi trình chiếu.",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _isSharing = true;
                _isPaused = false;
                _shareCts = new CancellationTokenSource();

                // Update UI
                PreviewPlaceholder.Visibility = Visibility.Collapsed;
                ScreenContent.Visibility = Visibility.Visible;
                ViewerPanel.Visibility = Visibility.Visible;

                // Start preview timer
                _previewTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(200) // 5 FPS preview for performance
                };
                _previewTimer.Tick += PreviewTimer_Tick;
                _previewTimer.Start();

                // Start screen sharing task
                _ = ShareScreenAsync(_shareCts.Token);

                _log.Info("ScreenShare", $"Started screen sharing (Mode: {_currentMode})");
                ToastService.Instance.ShowSuccess("Bắt đầu trình chiếu", "Học sinh đang xem màn hình của bạn");
            }
            catch (Exception ex)
            {
                _log.Error("ScreenShare", "Failed to start sharing", ex);
                MessageBox.Show($"Lỗi khi bắt đầu trình chiếu: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                _isSharing = false;
            }
        }

        private async Task ShareScreenAsync(CancellationToken ct)
        {
            var networkServer = _session.NetworkServer;
            
            while (!ct.IsCancellationRequested && _isSharing)
            {
                try
                {
                    if (_isPaused)
                    {
                        await Task.Delay(100, ct);
                        continue;
                    }

                    // Capture screen with good quality for streaming
                    byte[] screenData;
                    switch (_currentMode)
                    {
                        case ShareMode.Region when _selectedRegion.HasValue:
                            var r = _selectedRegion.Value;
                            screenData = _screenCapture.CaptureRegion(r.X, r.Y, r.Width, r.Height, 65);
                            break;
                        default:
                            // Capture at 720p quality for streaming
                            screenData = _screenCapture.CaptureScreenThumbnail(1280, 720, 65);
                            break;
                    }

                    // Send to all students
                    await networkServer.SendScreenShareAsync(screenData);

                    // Control frame rate (~15 FPS)
                    await Task.Delay(66, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.Warning("ScreenShare", $"Error during screen share: {ex.Message}");
                    await Task.Delay(100, ct);
                }
            }
        }

        private void PreviewTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!_isSharing || _isPaused) return;

                // Capture for preview (lower quality for performance)
                var previewData = _screenCapture.CaptureScreenThumbnail(640, 360, 50);
                
                var bitmap = new BitmapImage();
                using (var ms = new MemoryStream(previewData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                ScreenContent.Source = bitmap;
            }
            catch (Exception ex)
            {
                _log.Warning("ScreenShare", $"Preview error: {ex.Message}");
            }
        }

        private void StopSharing()
        {
            if (!_isSharing) return;

            _isSharing = false;
            _shareCts?.Cancel();
            _previewTimer?.Stop();

            // Notify students
            _ = _session.StopScreenShareAsync();

            // Update UI
            PreviewPlaceholder.Visibility = Visibility.Visible;
            ScreenContent.Visibility = Visibility.Collapsed;
            ScreenContent.Source = null;
            ViewerPanel.Visibility = Visibility.Collapsed;

            _log.Info("ScreenShare", "Stopped screen sharing");
            ToastService.Instance.ShowInfo("Dừng trình chiếu", "Đã ngừng chia sẻ màn hình");
        }

        #endregion

        #region Control Buttons

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = !_isPaused;

            // Update button icon
            if (PauseButton.Content is MaterialDesignThemes.Wpf.PackIcon icon)
            {
                icon.Kind = _isPaused 
                    ? MaterialDesignThemes.Wpf.PackIconKind.Play 
                    : MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }

            PauseButton.ToolTip = _isPaused ? "Tiếp tục" : "Tạm dừng";
            PauseButton.Background = _isPaused 
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00));

            if (_isPaused)
            {
                ToastService.Instance.ShowInfo("Tạm dừng", "Đã tạm dừng trình chiếu");
            }
            else
            {
                ToastService.Instance.ShowInfo("Tiếp tục", "Đã tiếp tục trình chiếu");
            }
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
                StopSharing();
            }
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
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

        #endregion
    }
}
