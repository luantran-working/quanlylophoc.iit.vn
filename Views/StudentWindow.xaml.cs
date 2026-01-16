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
            
            // Wire up network events
            _networkClient.Connected += OnConnected;
            _networkClient.Disconnected += OnDisconnected;
            _networkClient.MessageReceived += OnMessageReceived;
            _networkClient.ScreenShareReceived += OnScreenShareReceived;
            _networkClient.ScreenLocked += OnScreenLocked;
            _networkClient.ScreenUnlocked += OnScreenUnlocked;
            
            Loaded += StudentWindow_Loaded;
            Closing += StudentWindow_Closing;
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
            var serverInfo = await _networkClient.DiscoverServerAsync(30);
            
            if (serverInfo != null)
            {
                UpdateConnectionStatus($"Đang kết nối đến {serverInfo.ClassName}...");
                
                var connected = await _networkClient.ConnectAsync(serverInfo.ServerIp, serverInfo.ServerPort);
                if (connected)
                {
                    _isConnected = true;
                    UpdateConnectionStatus($"Đã kết nối - {serverInfo.ClassName}");
                    
                    // Start sending screen thumbnails periodically
                    _ = SendScreenDataAsync();
                }
                else
                {
                    ShowConnectionError("Không thể kết nối đến phòng học.");
                }
            }
            else
            {
                ShowConnectionError("Không tìm thấy phòng học trong mạng LAN.\nĐảm bảo giáo viên đã khởi động phiên học.");
            }
        }

        private async Task SendScreenDataAsync()
        {
            while (_isConnected)
            {
                try
                {
                    var thumbnail = _screenCapture.CaptureScreenThumbnail(320, 180, 50);
                    var (width, height) = ScreenCaptureService.GetScreenSize();
                    await _networkClient.SendScreenDataAsync(thumbnail, width, height);
                    
                    await Task.Delay(2000); // Every 2 seconds
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
                if (imageData.Length > 0)
                {
                    // Show screen share
                    var bitmap = ScreenCaptureService.BytesToBitmapImage(imageData);
                    // ScreenShareImage.Source = bitmap;
                    // ScreenSharePlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Hide screen share
                    // ScreenShareImage.Source = null;
                    // ScreenSharePlaceholder.Visibility = Visibility.Visible;
                }
            });
        }

        private void OnScreenLocked(object? sender, EventArgs e)
        {
            _isLocked = true;
            Dispatcher.Invoke(() =>
            {
                // Show lock screen overlay
                ShowLockScreen();
            });
        }

        private void OnScreenUnlocked(object? sender, EventArgs e)
        {
            _isLocked = false;
            Dispatcher.Invoke(() =>
            {
                // Hide lock screen overlay
                HideLockScreen();
            });
        }

        private void ShowLockScreen()
        {
            // TODO: Show full-screen lock overlay
            MessageBox.Show("Máy đang bị khóa bởi giáo viên.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void HideLockScreen()
        {
            // TODO: Hide lock overlay
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

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi cho giáo viên",
                Filter = "Tất cả tệp (*.*)|*.*|Tài liệu Word (*.docx)|*.docx|PDF (*.pdf)|*.pdf|Hình ảnh (*.jpg;*.png)|*.jpg;*.png",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // TODO: Actually send files through network
                string files = string.Join("\n", openFileDialog.FileNames);
                MessageBox.Show(
                    $"Đã chọn {openFileDialog.FileNames.Length} file để gửi:\n{files}",
                    "Gửi file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
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
