using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Controls
{
    /// <summary>
    /// Control hiển thị thumbnail màn hình học sinh
    /// </summary>
    public partial class ScreenThumbnailControl : UserControl
    {
        private Student? _student;
        private readonly LogService _log = LogService.Instance;
        private readonly DispatcherTimer _refreshTimer;

        public ScreenThumbnailControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            
            // Setup auto-refresh timer (500ms)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Student student)
            {
                _student = student;
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            if (_student == null) return;

            try
            {
                // Update lock overlay
                LockedOverlay.Visibility = _student.IsLocked ? Visibility.Visible : Visibility.Collapsed;
                
                // Update lock button icon
                if (LockButton.Content is MaterialDesignThemes.Wpf.PackIcon lockIcon)
                {
                    lockIcon.Kind = _student.IsLocked 
                        ? MaterialDesignThemes.Wpf.PackIconKind.LockOpen 
                        : MaterialDesignThemes.Wpf.PackIconKind.Lock;
                }
                LockButton.ToolTip = _student.IsLocked ? "Mở khóa màn hình" : "Khóa màn hình";

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
                        PlaceholderPanel.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        // Ignore image load errors during rapid updates
                    }
                }
                else
                {
                    PlaceholderPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _log.Error("ScreenThumbnail", "Error updating UI", ex);
            }
        }

        private async void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_student == null) return;

            try
            {
                var session = SessionManager.Instance;
                var newLockState = !_student.IsLocked;
                
                await session.LockStudentAsync(_student.MachineId, newLockState);
                _student.IsLocked = newLockState;
                
                UpdateUI();
                
                ToastService.Instance.ShowInfo(
                    newLockState ? "Đã khóa máy" : "Đã mở khóa",
                    $"Máy của {_student.DisplayName} đã được {(newLockState ? "khóa" : "mở khóa")}");
            }
            catch (Exception ex)
            {
                _log.Error("ScreenThumbnail", "Error locking student", ex);
                ToastService.Instance.ShowError("Lỗi", "Không thể thực hiện thao tác khóa máy");
            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (_student == null) return;

            try
            {
                // Open private chat window
                var chatWindow = new Views.ChatWindow(_student);
                chatWindow.Title = $"Chat với {_student.DisplayName}";
                chatWindow.Show();
            }
            catch (Exception ex)
            {
                _log.Error("ScreenThumbnail", "Error opening chat", ex);
            }
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_student == null) return;

            try
            {
                // Open fullscreen view
                var fullscreenWindow = new Views.StudentScreenWindow(_student);
                fullscreenWindow.Show();
            }
            catch (Exception ex)
            {
                _log.Error("ScreenThumbnail", "Error opening fullscreen", ex);
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-click to open fullscreen
            FullscreenButton_Click(sender, e);
        }
    }
}
