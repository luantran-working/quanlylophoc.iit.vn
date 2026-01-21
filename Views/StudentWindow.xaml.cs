using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        private readonly NetworkClientService _networkClient;
        private readonly ScreenCaptureService _screenCapture;
        private readonly string _studentName;
        private bool _isHandRaised = false;
        private bool _isConnected = false;
        private bool _isLocked = false;
        private bool _isRemoteControlled = false;

        public StudentWindow() : this("Học sinh")
        {
        }

        public StudentWindow(string studentName)
        {
            InitializeComponent();

            _studentName = studentName;
            _networkClient = new NetworkClientService();
            _networkClient.DisplayName = studentName;
            _screenCapture = new ScreenCaptureService();
            PollService.Instance.InitializeClient(_networkClient);

            // Wire up network events
            _networkClient.Connected += OnConnected;
            _networkClient.Disconnected += OnDisconnected;
            _networkClient.MessageReceived += OnMessageReceived;
            _networkClient.ScreenShareReceived += OnScreenShareReceived;
            _networkClient.ScreenLocked += OnScreenLocked;
            _networkClient.ScreenUnlocked += OnScreenUnlocked;
            _networkClient.RemoteControlStarted += OnRemoteControlStarted;
            _networkClient.RemoteControlStopped += OnRemoteControlStopped;

            FileReceiverService.Instance.FileRequestReceived += OnFileRequestReceived;

            // PollService.Instance.PollStarted += OnPollStarted;

            Loaded += StudentWindow_Loaded;
            Closing += StudentWindow_Closing;
        }

        // Window Control Events
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
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

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void StudentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ConnectToServerAsync();
        }

        private void StudentWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _networkClient.Disconnect();
            _networkClient.Dispose();
        }

        private async Task ConnectToServerAsync()
        {
            UpdateConnectionStatus("Đang tìm phòng học...");

            // Try to discover server
            var serverInfo = await _networkClient.DiscoverServerAsync(10);

            if (serverInfo != null)
            {
                await TryConnectToServer(serverInfo.ServerIp, serverInfo.ServerPort, serverInfo.ClassName);
            }
            else
            {
                // Show manual connect dialog
                await ShowManualConnectDialog();
            }
        }

        private async Task ShowManualConnectDialog()
        {
            var dialog = new ManualConnectDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                if (dialog.ContinueSearching)
                {
                    // User wants to continue auto-discovery
                    await ConnectToServerAsync();
                }
                else if (!string.IsNullOrEmpty(dialog.ServerIp))
                {
                    string targetIp = dialog.ServerIp;

                    // Check if input is a connection code
                    if (targetIp.StartsWith("CONNECTION_CODE:"))
                    {
                        var code = targetIp.Substring("CONNECTION_CODE:".Length);
                        UpdateConnectionStatus($"Đang tìm server từ mã kết nối {code}...");

                        // Try to convert code to IP (scan common ranges)
                        var resolvedIp = await Task.Run(() => ConnectionPasswordService.Instance.TryGetIPFromPassword(code));

                        if (resolvedIp != null)
                        {
                            targetIp = resolvedIp;
                        }
                        else
                        {
                            MessageBox.Show(
                                "Không tìm thấy server với mã kết nối này trong mạng 192.168.x.x.\nVui lòng kiểm tra lại hoặc nhập IP trực tiếp.",
                                "Không tìm thấy", MessageBoxButton.OK, MessageBoxImage.Warning);
                            await ShowManualConnectDialog(); // Retry
                            return;
                        }
                    }

                    // User entered IP manually or resolved from code
                    await TryConnectToServer(targetIp, 5000, "Phòng học");
                }
            }
            else
            {
                // User cancelled, go back to role selection
                var roleWindow = new RoleSelectionWindow();
                roleWindow.Show();
                Close();
            }
        }

        private async Task TryConnectToServer(string serverIp, int port, string className)
        {
            UpdateConnectionStatus($"Đang kết nối đến {serverIp}...");

            var connected = await _networkClient.ConnectAsync(serverIp, port);
            if (connected)
            {
                _isConnected = true;
                UpdateConnectionStatus($"Đã kết nối - {className}");

                // Show toast notification
                ToastService.Instance.ShowSuccess(
                    "Kết nối thành công",
                    $"Đã kết nối đến {className}\nServer: {serverIp}:{port}");

                // Start sending screen thumbnails periodically
                _ = SendScreenDataAsync();
            }
            else
            {
                Dispatcher.Invoke(async () =>
                {
                    MessageBox.Show(
                        $"Không thể kết nối đến {serverIp}:{port}\n\nKiểm tra:\n- Firewall trên máy giáo viên\n- Cùng mạng WiFi/LAN\n- Giáo viên đã khởi động phiên học",
                        "Lỗi kết nối",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    await ShowManualConnectDialog();
                });
            }
        }

        private async Task SendScreenDataAsync()
        {
            while (_isConnected)
            {
                try
                {
                    // When remote controlled, use higher quality and faster rate
                    int width, height, quality, delay;
                    if (_isRemoteControlled)
                    {
                        // Lower quality to ensure packet fits in buffer (packet fragmentation workaround)
                        width = 1024;
                        height = 576;
                        quality = 55;
                        delay = 100;
                    }
                    else
                    {
                        // Normal quality for thumbnail view (360p, 75% quality, 1.5s delay)
                        width = 640;
                        height = 360;
                        quality = 70;
                        delay = 1500;
                    }

                    var thumbnail = _screenCapture.CaptureScreenThumbnail(width, height, quality);
                    var (screenWidth, screenHeight) = ScreenCaptureService.GetScreenSize();
                    await _networkClient.SendScreenDataAsync(thumbnail, screenWidth, screenHeight);

                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Screen capture error: {ex.Message}");
                }
            }
        }

        private void UpdateConnectionStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {
                // Update status in UI
                // ConnectionStatusText.Text = status;
            });
        }

        private void ShowConnectionError(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(
                    $"{message}\n\nBạn có muốn thử lại không?",
                    "Lỗi kết nối",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    _ = ConnectToServerAsync();
                }
                else
                {
                    Close();
                }
            });
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionStatus("Đã kết nối");
            });
        }

        private void OnDisconnected(object? sender, string reason)
        {
            _isConnected = false;
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionStatus("Mất kết nối");

                if (!_isLocked)
                {
                    ShowConnectionError($"Mất kết nối với giáo viên: {reason}");
                }
            });
        }

        private void OnMessageReceived(object? sender, NetworkMessage message)
        {
            Dispatcher.Invoke(() =>
            {
                switch (message.Type)
                {
                    case MessageType.ChatMessage:
                    case MessageType.ChatPrivate:
                        ShowChatNotification(message.SenderName, message.Payload ?? "");
                        break;

                    case MessageType.TestStart:
                        ShowTestNotification(message.Payload ?? "");
                        break;

                    case MessageType.Notification:
                        ShowNotification(message.Payload ?? "");
                        break;
                }
            });
        }

        private void OnScreenShareReceived(object? sender, byte[] imageData)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (imageData.Length > 0)
                    {
                        // Show screen share
                        var bitmap = ScreenCaptureService.BytesToBitmapImage(imageData);
                        PresentationImage.Source = bitmap;
                        PresentationImage.Visibility = Visibility.Visible;
                        NoPresentationPlaceholder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        // Hide screen share (stream stopped)
                        PresentationImage.Source = null;
                        PresentationImage.Visibility = Visibility.Collapsed;
                        NoPresentationPlaceholder.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    Services.LogService.Instance.Warning("Student", $"Error displaying screen share: {ex.Message}");
                }
            });
        }

        private void OnScreenLocked(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _isLocked = true;
                _isRemoteControlled = false;
                ShowLockScreen();
            });
        }

        private void OnScreenUnlocked(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _isLocked = false;
                HideLockScreen();
            });
        }

        private void OnFileRequestReceived(object? sender, BulkFileTransferRequest req)
        {
            Dispatcher.Invoke(() =>
            {
                 var popup = new FileNotificationPopup(req);
                 popup.Owner = this;
                 popup.Show();
            });
        }

        private void OnPollStarted(object? sender, Poll poll)
        {
            Dispatcher.Invoke(() =>
            {
                var win = new VotePollWindow(poll);
                // win.Owner = this; // Should be top but maybe not child if fullscreen lock?
                win.Show();
            });
        }

        private void OnRemoteControlStarted(object? sender, EventArgs e)
        {
            _isRemoteControlled = true;
            Services.LogService.Instance.Info("Student", "Remote control session started by teacher");
        }

        private void OnRemoteControlStopped(object? sender, EventArgs e)
        {
            _isRemoteControlled = false;
            Services.LogService.Instance.Info("Student", "Remote control session ended");
        }

        private LockScreenWindow? _lockScreenWindow;

        private void ShowLockScreen()
        {
            if (_lockScreenWindow != null) return;

            _lockScreenWindow = new LockScreenWindow();
            _lockScreenWindow.Show();

            // Log the lock event
            Services.LogService.Instance.Info("Student", "Screen locked by teacher");
        }

        private void HideLockScreen()
        {
            if (_lockScreenWindow != null)
            {
                _lockScreenWindow.Unlock();
                _lockScreenWindow = null;

                // Log the unlock event
                Services.LogService.Instance.Info("Student", "Screen unlocked by teacher");

                // Show toast
                ToastService.Instance.ShowInfo("Đã mở khóa", "Giáo viên đã mở khóa màn hình của bạn");
            }
        }

        private void ShowChatNotification(string sender, string content)
        {
            MessageBox.Show($"{sender}: {content}", "Tin nhắn mới",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowTestNotification(string testInfo)
        {
            var result = MessageBox.Show(
                $"Giáo viên gửi bài kiểm tra!\n\n{testInfo}\n\nBạn có muốn bắt đầu làm bài?",
                "Bài kiểm tra mới",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Open test window
            }
        }

        private void ShowNotification(string message)
        {
            MessageBox.Show(message, "Thông báo từ Giáo viên",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SubmitAssignment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SubmitAssignmentDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var submission = new AssignmentSubmission
                    {
                        StudentId = _networkClient.MachineId,
                        StudentName = _studentName,
                        SessionId = "0", // Server will handle SessionId
                        SubmittedAt = DateTime.Now,
                        Note = dialog.Note
                    };

                    foreach (var fileView in dialog.SelectedFiles)
                    {
                        var data = await System.IO.File.ReadAllBytesAsync(fileView.LocalPath);
                        submission.Files.Add(new SubmittedFile
                        {
                            FileName = fileView.FileName,
                            FileSize = fileView.FileSize,
                            LocalPath = "",
                            Data = data
                        });
                    }

                    ToastService.Instance.ShowInfo("Đang gửi...", "Đang nộp bài tập...");

                    // Send to server
                    await _networkClient.SubmitAssignmentAsync(submission);

                    MessageBox.Show("Đã nộp bài thành công!", "Nộp bài", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi nộp bài: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RaiseHand_Click(object sender, MouseButtonEventArgs e)
        {
            _isHandRaised = !_isHandRaised;
            RaiseHandToggle.IsChecked = _isHandRaised;

            if (_isHandRaised)
            {
                RaiseHandStatus.Text = "Đang giơ tay...";
                RaiseHandBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x4D, 0x4D, 0x5D));

                // Send raise hand to server
                await _networkClient.RaiseHandAsync(true);
            }
            else
            {
                RaiseHandStatus.Text = "Nhấn để giơ tay";
                RaiseHandBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3D, 0x3D, 0x4D));

                await _networkClient.RaiseHandAsync(false);
            }
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatWindow();
            chatWindow.Title = "Chat với Giáo viên";
            chatWindow.Owner = this;
            chatWindow.Show();
        }

        private async void RequestHelp_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có muốn gửi yêu cầu trợ giúp đến giáo viên không?\n\nGiáo viên sẽ nhận được thông báo và có thể hỗ trợ bạn.",
                "Yêu cầu trợ giúp",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Send help request through network
                await _networkClient.SendMessageAsync(new NetworkMessage
                {
                    Type = MessageType.Notification,
                    SenderId = _networkClient.MachineId,
                    SenderName = _studentName,
                    Payload = $"{_studentName} yêu cầu trợ giúp!"
                });

                MessageBox.Show(
                    "Yêu cầu trợ giúp đã được gửi!\nGiáo viên sẽ phản hồi sớm nhất có thể.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void OpenLogWindow_Click(object sender, RoutedEventArgs e)
        {
            var logWindow = new LogWindow();
            logWindow.Owner = this;
            logWindow.Show();
        }
    }
}
