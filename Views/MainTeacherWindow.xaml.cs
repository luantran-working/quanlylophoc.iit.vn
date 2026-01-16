using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for MainTeacherWindow.xaml
    /// </summary>
    public partial class MainTeacherWindow : Window
    {
        private readonly SessionManager _session;
        private bool _isSessionStarted;

        public MainTeacherWindow()
        {
            InitializeComponent();
            
            _session = SessionManager.Instance;
            DataContext = _session;
            
            // Wire up events
            _session.StudentConnected += OnStudentConnected;
            _session.StudentDisconnected += OnStudentDisconnected;
            _session.ChatMessageReceived += OnChatMessageReceived;
            
            // Start session automatically
            Loaded += MainTeacherWindow_Loaded;
            Closing += MainTeacherWindow_Closing;
        }

        private async void MainTeacherWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await StartSessionAsync();
        }

        private void MainTeacherWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isSessionStarted)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn kết thúc phiên học?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                _session.EndSession();
            }
        }

        private async Task StartSessionAsync()
        {
            // Show session config dialog
            var className = $"Lớp học - {DateTime.Now:dd/MM/yyyy}";
            var subject = "Môn học";

            // TODO: Show dialog to get class name and subject
            // For now, use defaults

            var success = await _session.StartSessionAsync(className, subject);
            if (success)
            {
                _isSessionStarted = true;
                
                // Update UI with class name and server IP
                ClassNameText.Text = className;
                ServerIpText.Text = _session.NetworkServer.ServerIp;
                
                UpdateStatusBar();
            }
            else
            {
                MessageBox.Show("Không thể khởi động phiên học. Vui lòng thử lại.",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void UpdateStatusBar()
        {
            // Update status bar with online count
            var count = _session.OnlineStudents.Count;
            // StatusText would be bound in XAML
        }

        private void OnStudentConnected(object? sender, Student student)
        {
            UpdateStatusBar();
            // Show notification
            // TODO: Show toast notification
        }

        private void OnStudentDisconnected(object? sender, Student student)
        {
            UpdateStatusBar();
        }

        private void OnChatMessageReceived(object? sender, ChatMessage message)
        {
            // Show notification if chat window is not open
            // TODO: Implement notification system
        }

        private async void StartPresentation_Click(object sender, RoutedEventArgs e)
        {
            if (_session.IsScreenSharing)
            {
                await _session.StopScreenShareAsync();
                // Update button text
            }
            else
            {
                var screenShareWindow = new ScreenShareWindow();
                screenShareWindow.Owner = this;
                screenShareWindow.Show();
                
                // Start screen sharing in background
                _ = _session.StartScreenShareAsync();
            }
        }

        private void OpenGroupChat_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatWindow();
            chatWindow.Title = $"Chat nhóm - {_session.CurrentSession?.ClassName ?? "Lớp học"}";
            chatWindow.Owner = this;
            chatWindow.Show();
        }

        private void OpenFileTransfer_Click(object sender, RoutedEventArgs e)
        {
            var fileTransferWindow = new FileTransferWindow();
            fileTransferWindow.Owner = this;
            fileTransferWindow.Show();
        }

        private void OpenTestCreation_Click(object sender, RoutedEventArgs e)
        {
            var testCreationWindow = new TestCreationWindow();
            testCreationWindow.Owner = this;
            testCreationWindow.Show();
        }

        private async void LockAllStudents_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var isLocking = button?.Tag?.ToString() != "locked";
            
            await _session.LockAllStudentsAsync(isLocking);
            
            if (button != null)
            {
                button.Tag = isLocking ? "locked" : "unlocked";
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open settings window
            MessageBox.Show("Cài đặt sẽ được thêm trong phiên bản sau.",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenLogWindow_Click(object sender, RoutedEventArgs e)
        {
            var logWindow = new LogWindow();
            logWindow.Owner = this;
            logWindow.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // This will trigger the Closing event handler
        }

        private void RefreshScreens_Click(object sender, RoutedEventArgs e)
        {
            // Force refresh screen thumbnails
            RefreshAllScreenThumbnails();
            ToastService.Instance.ShowInfo("Làm mới", "Đang cập nhật màn hình học sinh...");
        }

        private void RefreshAllScreenThumbnails()
        {
            // Refresh each ScreenThumbnailControl
            var itemsControl = ScreenGrid;
            if (itemsControl == null) return;

            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null)
                {
                    var control = FindChild<Controls.ScreenThumbnailControl>(container);
                    control?.UpdateUI();
                }
            }
        }

        private void GridSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagStr && int.TryParse(tagStr, out int columns))
            {
                // Find the UniformGrid and update columns
                var itemsControl = ScreenGrid;
                if (itemsControl?.ItemsPanel?.Template != null)
                {
                    // We need to access the actual panel
                    var panel = FindVisualChild<System.Windows.Controls.Primitives.UniformGrid>(itemsControl);
                    if (panel != null)
                    {
                        panel.Columns = columns;
                    }
                }

                // Update button styles
                ResetGridButtonStyles();
                button.FontWeight = FontWeights.Bold;
                button.Foreground = FindResource("PrimaryHueMidBrush") as System.Windows.Media.Brush;
            }
        }

        private void ResetGridButtonStyles()
        {
            var normalBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            Grid2x2Btn.FontWeight = FontWeights.Normal;
            Grid2x2Btn.Foreground = normalBrush;
            Grid4x4Btn.FontWeight = FontWeights.Normal;
            Grid4x4Btn.Foreground = normalBrush;
            Grid6x6Btn.FontWeight = FontWeights.Normal;
            Grid6x6Btn.Foreground = normalBrush;
        }

        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var foundChild = FindChild<T>(child);
                if (foundChild != null)
                    return foundChild;
            }
            return null;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            return FindChild<T>(parent);
        }
    }
}

