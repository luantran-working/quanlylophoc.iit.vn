using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Cửa sổ xem màn hình học sinh toàn màn hình
    /// </summary>
    public partial class StudentScreenWindow : Window
    {
        private readonly Student _student;
        private readonly SessionManager _session;
        private readonly LogService _log = LogService.Instance;
        private readonly DispatcherTimer _refreshTimer;
        private byte[]? _highQualityScreen; // Ảnh chất lượng cao từ học sinh
        private DateTime _lastHighQualityRequest = DateTime.MinValue;

        public StudentScreenWindow(Student student)
        {
            InitializeComponent();
            
            _student = student;
            _session = SessionManager.Instance;
            
            // Setup UI
            StudentNameText.Text = student.DisplayName;
            StudentInfoText.Text = $"IP: {student.IpAddress} • {student.ComputerName}";
            
            // Setup refresh timer - faster for high quality viewing
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200) // 5 FPS for detailed view
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            
            // Subscribe to high quality screen data
            _session.NetworkServer.ScreenDataReceived += OnHighQualityScreenReceived;
            
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Start();
            await RequestHighQualityScreen(); // Yêu cầu ảnh Full HD ngay khi mở
            UpdateScreen();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _refreshTimer.Stop();
            _session.NetworkServer.ScreenDataReceived -= OnHighQualityScreenReceived;
        }

        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            // Yêu cầu ảnh Full HD định kỳ
            if ((DateTime.Now - _lastHighQualityRequest).TotalMilliseconds > 300)
            {
                await RequestHighQualityScreen();
            }
            UpdateScreen();
        }

        /// <summary>
        /// Yêu cầu ảnh Full HD từ máy học sinh
        /// </summary>
        private async System.Threading.Tasks.Task RequestHighQualityScreen()
        {
            try
            {
                _lastHighQualityRequest = DateTime.Now;
                
                var request = new ScreenshotRequest
                {
                    TargetStudentId = _student.MachineId,
                    Resolution = "fullhd",
                    Quality = 85,
                    RequestType = "preview",
                    SaveToLocal = false
                };

                var message = new NetworkMessage
                {
                    Type = MessageType.ScreenRequest,
                    SenderId = "server",
                    TargetId = _student.MachineId,
                    Payload = JsonSerializer.Serialize(request)
                };

                await _session.NetworkServer.SendToClientAsync(_student.MachineId, message);
            }
            catch (Exception ex)
            {
                _log.Warning("StudentScreen", $"Failed to request HD screen: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý ảnh chất lượng cao nhận được từ học sinh
        /// </summary>
        private void OnHighQualityScreenReceived(object? sender, ScreenDataReceivedEventArgs e)
        {
            if (e.ClientId != _student.MachineId) return;
            if (e.ScreenData?.ImageData == null || e.ScreenData.ImageData.Length == 0) return;

            _highQualityScreen = e.ScreenData.ImageData;
        }

        private void UpdateScreen()
        {
            try
            {
                // Update lock status
                LockedOverlay.Visibility = _student.IsLocked ? Visibility.Visible : Visibility.Collapsed;
                LockButtonText.Text = _student.IsLocked ? "Mở khóa" : "Khóa máy";
                
                // Ưu tiên ảnh chất lượng cao, fallback về thumbnail
                byte[]? imageData = _highQualityScreen ?? _student.ScreenThumbnail;
                
                if (imageData != null && imageData.Length > 0)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        using (var ms = new MemoryStream(imageData))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            bitmap.Freeze();
                        }
                        ScreenImage.Source = bitmap;
                        LoadingPanel.Visibility = Visibility.Collapsed;
                        
                        // Hiển thị độ phân giải thực tế
                        var resText = _highQualityScreen != null ? $"Full HD ({bitmap.PixelWidth}x{bitmap.PixelHeight})" : $"Thumbnail ({bitmap.PixelWidth}x{bitmap.PixelHeight})";
                        UpdateTimeText.Text = $"Cập nhật: {DateTime.Now:HH:mm:ss} • {resText}";
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("StudentScreen", $"Failed to load image: {ex.Message}");
                    }
                }

                // Update status
                StatusText.Text = _student.IsOnline ? "Đang xem trực tiếp (Full HD)" : "Học sinh offline";
            }
            catch (Exception ex)
            {
                _log.Error("StudentScreen", "Error updating screen", ex);
            }
        }

        private async void LockButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newLockState = !_student.IsLocked;
                await _session.LockStudentAsync(_student.MachineId, newLockState);
                _student.IsLocked = newLockState;
                
                UpdateScreen();
                
                ToastService.Instance.ShowInfo(
                    newLockState ? "Đã khóa máy" : "Đã mở khóa",
                    $"Máy của {_student.DisplayName} đã được {(newLockState ? "khóa" : "mở khóa")}");
            }
            catch (Exception ex)
            {
                _log.Error("StudentScreen", "Error locking student", ex);
            }
        }

        private void RemoteControlButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement remote control
            MessageBox.Show("Tính năng điều khiển từ xa sẽ được thêm trong phiên bản sau.",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Window Chrome Logic
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    Close();
            }
            else if (e.Key == Key.F11)
            {
                // Toggle fullscreen
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }
    }
}
