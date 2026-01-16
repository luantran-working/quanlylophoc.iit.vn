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
    }
}
