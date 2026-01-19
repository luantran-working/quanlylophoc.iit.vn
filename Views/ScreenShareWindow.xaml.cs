using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
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
        private enum ShareMode { FullScreen, Window }
        private ShareMode _currentMode = ShareMode.FullScreen;
        private IntPtr _selectedWindowHandle = IntPtr.Zero;

        // Annotation
        private Color _currentColor = Colors.Red;

        public ScreenShareWindow()
        {
            InitializeComponent();

            _session = SessionManager.Instance;
            _screenCapture = new ScreenCaptureService();

            Loaded += OnLoaded;
            Closing += OnClosing;
            KeyDown += OnKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateViewerCount();
            _session.OnlineStudents.CollectionChanged += (s, args) => UpdateViewerCount();

            // Setup annotation canvas
            AnnotationCanvas.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = _currentColor,
                Width = 3,
                Height = 3,
                FitToCurve = true
            };
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    if (_isSharing) StopSharing();
                    else ShareScreen_Click(sender, e);
                    break;
                case Key.F6:
                    if (_isSharing) PauseButton_Click(sender, e);
                    break;
                case Key.F11:
                    FullscreenButton_Click(sender, e);
                    break;
                case Key.P:
                    PenButton.IsChecked = !PenButton.IsChecked;
                    PenButton_Click(sender, e);
                    break;
                case Key.H:
                    HighlightButton.IsChecked = !HighlightButton.IsChecked;
                    HighlightButton_Click(sender, e);
                    break;
                case Key.C:
                    ClearAnnotations_Click(sender, e);
                    break;
                case Key.Escape:
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    break;
            }
        }

        private void UpdateViewerCount()
        {
            Dispatcher.Invoke(() =>
            {
                var count = _session.OnlineStudents.Count;
                ViewerCountText.Text = $" • {count} học sinh đang xem";
            });
        }

        #region Window Controls

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

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        #endregion

        #region Screen Share Modes

        private void ShareScreen_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = ShareMode.FullScreen;
            _selectedWindowHandle = IntPtr.Zero;
            StartSharing();
        }

        private void ShareWindow_Click(object sender, RoutedEventArgs e)
        {
            // Show window selection dialog
            var windowList = GetOpenWindows();
            if (windowList.Count == 0)
            {
                MessageBox.Show("Không tìm thấy cửa sổ nào để chia sẻ.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create simple selection dialog
            var dialog = new Window
            {
                Title = "Chọn cửa sổ để chia sẻ",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x3D)),
                WindowStyle = WindowStyle.ToolWindow
            };

            var listBox = new ListBox
            {
                Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10)
            };

            foreach (var window in windowList)
            {
                listBox.Items.Add(new ListBoxItem
                {
                    Content = window.Value,
                    Tag = window.Key,
                    Foreground = Brushes.White,
                    Padding = new Thickness(8),
                    FontSize = 14
                });
            }

            var okButton = new Button
            {
                Content = "Chia sẻ",
                Width = 100,
                Height = 35,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),
                Foreground = Brushes.White
            };

            okButton.Click += (s, args) =>
            {
                if (listBox.SelectedItem is ListBoxItem selectedItem)
                {
                    _selectedWindowHandle = (IntPtr)selectedItem.Tag;
                    _currentMode = ShareMode.Window;
                    dialog.DialogResult = true;
                    dialog.Close();
                }
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "Chọn cửa sổ để chia sẻ:",
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 5)
            });
            panel.Children.Add(listBox);
            panel.Children.Add(okButton);

            dialog.Content = panel;

            if (dialog.ShowDialog() == true)
            {
                StartSharing();
            }
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private Dictionary<IntPtr, string> GetOpenWindows()
        {
            var windows = new Dictionary<IntPtr, string>();
            var shellWindow = GetShellWindow();

            EnumWindows((hWnd, lParam) =>
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                var title = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, title, 256);
                var titleStr = title.ToString();

                if (!string.IsNullOrWhiteSpace(titleStr) && titleStr != "Trình chiếu màn hình")
                {
                    windows[hWnd] = titleStr;
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        #endregion

        #region Screen Sharing Logic

        private void StartSharing()
        {
            if (_isSharing) return;

            try
            {
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
                AnnotationCanvas.Visibility = Visibility.Visible;
                LiveIndicator.Visibility = Visibility.Visible;
                TitleText.Text = "ĐANG TRÌNH CHIẾU";
                StatusText.Text = _currentMode == ShareMode.Window ? "Đang chia sẻ cửa sổ" : "Đang chia sẻ màn hình";

                // Start preview timer
                _previewTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(150)
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

                    // Capture screen at 720p quality for streaming
                    byte[] screenData = _screenCapture.CaptureScreenThumbnail(1280, 720, 65);

                    // Send to all students
                    await networkServer.SendScreenShareAsync(screenData);

                    // Control frame rate (~12 FPS)
                    await Task.Delay(80, ct);
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
                if (!_isSharing) return;

                // Capture for preview
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
            AnnotationCanvas.Visibility = Visibility.Collapsed;
            AnnotationCanvas.Strokes.Clear();
            LiveIndicator.Visibility = Visibility.Collapsed;
            TitleText.Text = "TRÌNH CHIẾU MÀN HÌNH";
            StatusText.Text = "Đã dừng";

            // Reset buttons
            PenButton.IsChecked = false;
            HighlightButton.IsChecked = false;
            PauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            PauseButton.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));

            _log.Info("ScreenShare", "Stopped screen sharing");
            ToastService.Instance.ShowInfo("Dừng trình chiếu", "Đã ngừng chia sẻ màn hình");
        }

        #endregion

        #region Control Buttons

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSharing) return;

            _isPaused = !_isPaused;

            PauseIcon.Kind = _isPaused
                ? MaterialDesignThemes.Wpf.PackIconKind.Play
                : MaterialDesignThemes.Wpf.PackIconKind.Pause;

            PauseButton.ToolTip = _isPaused ? "Tiếp tục (F6)" : "Tạm dừng (F6)";
            PauseButton.Background = _isPaused
                ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))
                : new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));

            StatusText.Text = _isPaused ? "Đã tạm dừng" : "Đang chia sẻ";

            if (_isPaused)
                ToastService.Instance.ShowInfo("Tạm dừng", "Đã tạm dừng trình chiếu");
            else
                ToastService.Instance.ShowInfo("Tiếp tục", "Đã tiếp tục trình chiếu");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSharing)
            {
                Close();
                return;
            }

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

        #endregion

        #region Annotation Tools

        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            if (PenButton.IsChecked == true)
            {
                HighlightButton.IsChecked = false;
                AnnotationCanvas.EditingMode = InkCanvasEditingMode.Ink;
                AnnotationCanvas.DefaultDrawingAttributes = new DrawingAttributes
                {
                    Color = _currentColor,
                    Width = 3,
                    Height = 3,
                    FitToCurve = true
                };
            }
            else
            {
                AnnotationCanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            if (HighlightButton.IsChecked == true)
            {
                PenButton.IsChecked = false;
                AnnotationCanvas.EditingMode = InkCanvasEditingMode.Ink;
                AnnotationCanvas.DefaultDrawingAttributes = new DrawingAttributes
                {
                    Color = Color.FromArgb(128, _currentColor.R, _currentColor.G, _currentColor.B),
                    Width = 20,
                    Height = 10,
                    FitToCurve = true,
                    IsHighlighter = true
                };
            }
            else
            {
                AnnotationCanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void ClearAnnotations_Click(object sender, RoutedEventArgs e)
        {
            AnnotationCanvas.Strokes.Clear();
        }

        private void ColorPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Ellipse ellipse && ellipse.Tag is string colorStr)
            {
                _currentColor = (Color)ColorConverter.ConvertFromString(colorStr);

                // Update current tool if active
                if (PenButton.IsChecked == true)
                {
                    AnnotationCanvas.DefaultDrawingAttributes.Color = _currentColor;
                }
                else if (HighlightButton.IsChecked == true)
                {
                    AnnotationCanvas.DefaultDrawingAttributes.Color =
                        Color.FromArgb(128, _currentColor.R, _currentColor.G, _currentColor.B);
                }
            }
        }

        #endregion
    }
}
