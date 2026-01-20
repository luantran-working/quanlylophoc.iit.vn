using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class RemoteControlWindow : Window
    {
        private readonly RemoteControlService _remoteService;
        private readonly Student _student;
        private RemoteSession? _session;
        private DispatcherTimer? _refreshTimer;
        private bool _isFullscreen;
        private WindowState _previousWindowState;
        private DateTime _lastFrameTime = DateTime.Now;
        private int _frameCount;

        public RemoteControlWindow(Student student)
        {
            _remoteService = RemoteControlService.Instance;
            _student = student;

            InitializeComponent();

            Loaded += RemoteControlWindow_Loaded;
            Closing += RemoteControlWindow_Closing;
        }

        private async void RemoteControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set window title
            TitleText.Text = $"Điều khiển: {_student.DisplayName}";

            // Start remote control session
            _session = await _remoteService.RequestControlAsync(_student);

            if (_session == null)
            {
                MessageBox.Show(
                    "Không thể kết nối đến máy học sinh. Vui lòng thử lại.",
                    "Lỗi kết nối",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return;
            }

            // Update UI
            ConnectingOverlay.Visibility = Visibility.Collapsed;
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80)); // Green
            StatusText.Text = "Đang điều khiển";
            InfoText.Text = $"Đang điều khiển {_student.DisplayName} ({_student.IpAddress})";

            // Subscribe to screen updates
            _remoteService.ScreenReceived += OnScreenReceived;

            // Start refresh timer for FPS counter
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // Request initial screen
            await RequestScreenUpdate();
        }

        private async void RemoteControlWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _refreshTimer?.Stop();
            _remoteService.ScreenReceived -= OnScreenReceived;

            if (_session != null)
            {
                await _remoteService.EndSessionAsync(_student.MachineId);
            }
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            // Update FPS display
            FpsText.Text = $"FPS: {_frameCount}";
            _frameCount = 0;

            // Update latency (simulated for now)
            var latency = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
            LatencyText.Text = $"Latency: {latency:F0} ms";
        }

        private void OnScreenReceived(object? sender, byte[] screenData)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    using var ms = new MemoryStream(screenData);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    RemoteScreen.Source = bitmap;
                    ResolutionText.Text = $"Resolution: {bitmap.PixelWidth}x{bitmap.PixelHeight}";

                    _lastFrameTime = DateTime.Now;
                    _frameCount++;
                }
                catch (Exception ex)
                {
                    LogService.Instance.Error("RemoteControl", "Failed to display screen", ex);
                }
            });
        }

        private async System.Threading.Tasks.Task RequestScreenUpdate()
        {
            // Request screen update from student
            if (_session != null)
            {
                await _remoteService.TakeScreenshotAsync(_student.MachineId);
            }
        }

        // Window Controls
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && !_isFullscreen)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = _previousWindowState;
                ResizeMode = ResizeMode.CanResize;
                FullscreenIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Fullscreen;
                _isFullscreen = false;
            }
            else
            {
                _previousWindowState = WindowState;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                FullscreenIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FullscreenExit;
                _isFullscreen = true;
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            await ConfirmAndClose();
        }

        private async System.Threading.Tasks.Task ConfirmAndClose()
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn kết thúc phiên điều khiển?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        // Toolbar Actions
        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            await ConfirmAndClose();
        }

        private void ControlToggle_Changed(object sender, RoutedEventArgs e)
        {
            var isControlEnabled = ControlToggle.IsChecked == true;
            _remoteService.SetViewOnlyMode(_student.MachineId, !isControlEnabled);
            ViewOnlyBanner.Visibility = isControlEnabled ? Visibility.Collapsed : Visibility.Visible;
            InfoText.Text = isControlEnabled ? "Điều khiển đang bật" : "Chế độ chỉ xem";
        }

        private async void LockInput_Changed(object sender, RoutedEventArgs e)
        {
            var isLocked = LockInputBtn.IsChecked == true;
            await _remoteService.SetInputLockAsync(_student.MachineId, isLocked);
            InfoText.Text = isLocked ? "Input học sinh đã bị khóa" : "Input học sinh đã được mở khóa";
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RequestScreenUpdate();
            ToastService.Instance.ShowInfo("Làm mới", "Đang làm mới kết nối...");
        }

        private void VirtualKeyboard_Click(object sender, RoutedEventArgs e)
        {
            // Show virtual keyboard menu
            var menu = new ContextMenu();

            var keys = new[]
            {
                ("Ctrl+Alt+Del", "Ctrl+Alt+Delete"),
                ("Alt+Tab", "Alt+Tab"),
                ("Alt+F4", "Alt+F4"),
                ("Win", "Windows Key"),
                ("Ctrl+Esc", "Start Menu"),
                ("Print Screen", "Screenshot")
            };

            foreach (var (combo, name) in keys)
            {
                var menuItem = new MenuItem { Header = $"{name} ({combo})" };
                var keyComboCopy = combo;
                menuItem.Click += async (s, args) =>
                {
                    await SendSpecialKey(keyComboCopy);
                };
                menu.Items.Add(menuItem);
            }

            menu.PlacementTarget = sender as Button;
            menu.IsOpen = true;
        }

        private async System.Threading.Tasks.Task SendSpecialKey(string keyCombo)
        {
            // TODO: Implement special key sending
            ToastService.Instance.ShowInfo("Phím đặc biệt", $"Đã gửi: {keyCombo}");
            await System.Threading.Tasks.Task.Delay(100);
        }

        private async void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            await TakeScreenshot();
        }

        private async System.Threading.Tasks.Task TakeScreenshot()
        {
            var screenshotData = await _remoteService.TakeScreenshotAsync(_student.MachineId);

            if (screenshotData != null || RemoteScreen.Source != null)
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    FileName = $"{_student.DisplayName}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (RemoteScreen.Source is BitmapSource bitmap)
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));

                            using var stream = File.Create(saveDialog.FileName);
                            encoder.Save(stream);

                            ToastService.Instance.ShowSuccess("Đã lưu", $"Ảnh chụp màn hình đã được lưu");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file để gửi"
            };

            if (openDialog.ShowDialog() == true)
            {
                ToastService.Instance.ShowInfo("Gửi file", $"Đang gửi {Path.GetFileName(openDialog.FileName)}...");
                // TODO: Implement file sending
            }
        }

        private void Quality_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_session != null && QualityCombo.SelectedItem is ComboBoxItem item && item.Tag is string quality)
            {
                _session.Quality = int.Parse(quality);
                InfoText.Text = $"Đã đổi chất lượng: {item.Content}";
            }
        }

        // Mouse Events
        private async void RemoteScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (_session == null || !_session.IsControlEnabled) return;

            var pos = e.GetPosition(RemoteScreen);
            var input = new MouseInputData
            {
                X = pos.X / RemoteScreen.ActualWidth,
                Y = pos.Y / RemoteScreen.ActualHeight,
                Button = RemoteMouseButton.None,
                Action = RemoteMouseAction.Move
            };

            await _remoteService.SendMouseInputAsync(_student.MachineId, input);
        }

        private async void RemoteScreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_session == null || !_session.IsControlEnabled) return;

            var pos = e.GetPosition(RemoteScreen);
            var button = e.ChangedButton == System.Windows.Input.MouseButton.Left ? RemoteMouseButton.Left :
                         e.ChangedButton == System.Windows.Input.MouseButton.Right ? RemoteMouseButton.Right :
                         RemoteMouseButton.Middle;

            var input = new MouseInputData
            {
                X = pos.X / RemoteScreen.ActualWidth,
                Y = pos.Y / RemoteScreen.ActualHeight,
                Button = button,
                Action = e.ClickCount == 2 ? RemoteMouseAction.DoubleClick : RemoteMouseAction.Down
            };

            await _remoteService.SendMouseInputAsync(_student.MachineId, input);
        }

        private async void RemoteScreen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_session == null || !_session.IsControlEnabled) return;

            var pos = e.GetPosition(RemoteScreen);
            var button = e.ChangedButton == System.Windows.Input.MouseButton.Left ? RemoteMouseButton.Left :
                         e.ChangedButton == System.Windows.Input.MouseButton.Right ? RemoteMouseButton.Right :
                         RemoteMouseButton.Middle;

            var input = new MouseInputData
            {
                X = pos.X / RemoteScreen.ActualWidth,
                Y = pos.Y / RemoteScreen.ActualHeight,
                Button = button,
                Action = RemoteMouseAction.Up
            };

            await _remoteService.SendMouseInputAsync(_student.MachineId, input);
        }

        private async void RemoteScreen_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_session == null || !_session.IsControlEnabled) return;

            var pos = e.GetPosition(RemoteScreen);
            var input = new MouseInputData
            {
                X = pos.X / RemoteScreen.ActualWidth,
                Y = pos.Y / RemoteScreen.ActualHeight,
                Button = RemoteMouseButton.None,
                Action = RemoteMouseAction.Wheel,
                WheelDelta = e.Delta
            };

            await _remoteService.SendMouseInputAsync(_student.MachineId, input);
        }

        // Keyboard Events
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle special keys
            if (e.Key == Key.Escape)
            {
                if (_isFullscreen)
                    ToggleFullscreen();
                else
                    await ConfirmAndClose();
                return;
            }

            if (e.Key == Key.F11)
            {
                ToggleFullscreen();
                return;
            }

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await TakeScreenshot();
                return;
            }

            // Forward other keys to remote machine
            if (_session != null && _session.IsControlEnabled)
            {
                var input = new KeyboardInputData
                {
                    Key = KeyInterop.VirtualKeyFromKey(e.Key),
                    IsKeyDown = true,
                    Modifiers = GetModifiers()
                };

                await _remoteService.SendKeyboardInputAsync(_student.MachineId, input);
            }
        }

        private string GetModifiers()
        {
            var mods = new System.Collections.Generic.List<string>();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                mods.Add("Ctrl");
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                mods.Add("Alt");
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                mods.Add("Shift");
            return string.Join("+", mods);
        }
    }
}
