using System;
using System.IO;
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

        public StudentScreenWindow(Student student)
        {
            InitializeComponent();
            
            _student = student;
            _session = SessionManager.Instance;
            
            // Setup UI
            StudentNameText.Text = student.DisplayName;
            StudentInfoText.Text = $"IP: {student.IpAddress} • {student.ComputerName}";
            
            // Setup refresh timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Start();
            UpdateScreen();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _refreshTimer.Stop();
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            try
            {
                // Update lock status
                LockedOverlay.Visibility = _student.IsLocked ? Visibility.Visible : Visibility.Collapsed;
                LockButtonText.Text = _student.IsLocked ? "Mở khóa" : "Khóa máy";
                
                // Update screen image
                if (_student.ScreenThumbnail != null && _student.ScreenThumbnail.Length > 0)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        using (var ms = new MemoryStream(_student.ScreenThumbnail))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            bitmap.Freeze();
                        }
                        ScreenImage.Source = bitmap;
                        LoadingPanel.Visibility = Visibility.Collapsed;
                        UpdateTimeText.Text = $"Cập nhật: {DateTime.Now:HH:mm:ss}";
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("StudentScreen", $"Failed to load image: {ex.Message}");
                    }
                }

                // Update status
                StatusText.Text = _student.IsOnline ? "Đang xem trực tiếp" : "Học sinh offline";
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.F11)
            {
                // Toggle fullscreen
                if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
            }
        }
    }
}
