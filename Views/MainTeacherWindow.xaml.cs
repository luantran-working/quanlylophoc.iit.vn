using System;
using System.Linq;
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
        private int _selectedCount = 0;

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

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

                // Generate and display connection password
                var connectionPassword = ConnectionPasswordService.Instance.GeneratePasswordFromIP(_session.NetworkServer.ServerIp);

                // Show connection password to teacher (Updated: Show in Header only)
                ConnectionCodeText.Text = connectionPassword;

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
            student.PropertyChanged += OnStudentPropertyChanged;
            UpdateStatusBar();
            // Show notification
            // TODO: Show toast notification
        }

        private void OnStudentDisconnected(object? sender, Student student)
        {
            student.PropertyChanged -= OnStudentPropertyChanged;
            UpdateStatusBar();
        }

        private void OnChatMessageReceived(object? sender, ChatMessage message)
        {
            // Show notification if chat window is not open
            // TODO: Implement notification system
        }

        private void StartPresentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenShareWindow = new ScreenShareWindow();
                screenShareWindow.Owner = this;
                screenShareWindow.Show();
            }
            catch (Exception ex)
            {
                Services.LogService.Instance.Error("MainTeacher", "Failed to open ScreenShareWindow", ex);
                MessageBox.Show($"Lỗi khi mở cửa sổ trình chiếu: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OpenFileCollection_Click(object sender, RoutedEventArgs e)
        {
            var win = new FileCollectionWindow();
            win.Owner = this;
            win.Show();
        }

        private void OpenBulkFileSend_Click(object sender, RoutedEventArgs e)
        {
            var win = new BulkFileSendWindow();
            win.Owner = this;
            win.Show();
        }

        private void OpenCreatePoll_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreatePollWindow();
            win.Owner = this;
            win.Show();
        }

        private void OpenAssignmentList_Click(object sender, RoutedEventArgs e)
        {
            var assignmentListWindow = new AssignmentListWindow();
            assignmentListWindow.Owner = this;
            assignmentListWindow.Show();
        }

        private void OpenConfigTable_Click(object sender, RoutedEventArgs e)
        {
            var configTableWindow = new SystemConfigTableWindow();
            configTableWindow.Owner = this;
            configTableWindow.Show();
        }

        private void OpenWhiteboard_Click(object sender, RoutedEventArgs e)
        {
            var whiteboardWindow = new WhiteboardWindow();
            whiteboardWindow.Owner = this;
            whiteboardWindow.Show();
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

        // ============================================
        // BATCH OPERATIONS
        // ============================================

        private void BatchActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open context menu
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void SelectAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            SetAllStudentsSelection(true);
        }

        private void SelectAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Only update if triggered by user interaction, not by code
            if (_selectedCount == _session.OnlineStudents.Count)
                SetAllStudentsSelection(false);
        }

        private void SetAllStudentsSelection(bool selected)
        {
            foreach (var student in _session.OnlineStudents)
            {
                student.IsSelected = selected;
            }
            UpdateSelectionUI();
        }

        private void ToggleSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCount > 0)
            {
                // Bỏ chọn tất cả
                SetAllStudentsSelection(false);
                SelectAllCheckbox.IsChecked = false;
            }
            else
            {
                // Chọn tất cả
                SetAllStudentsSelection(true);
                SelectAllCheckbox.IsChecked = true;
            }
        }

        private void UpdateSelectionUI()
        {
            _selectedCount = _session.OnlineStudents.Count(s => s.IsSelected);

            BatchActionsBtn.IsEnabled = _selectedCount > 0;

            if (_selectedCount > 0)
            {
                SelectionCountText.Text = $"({_selectedCount} đã chọn)";
                ToggleSelectionBtn.Content = "Bỏ chọn";

                // Update checkbox header state without triggering event loop if possible
                // (Simple IsChecked assignment is fine here due to check in handler)
                if (_selectedCount == _session.OnlineStudents.Count)
                    SelectAllCheckbox.IsChecked = true;
                else
                    SelectAllCheckbox.IsChecked = null; // Indeterminate if partially selected
            }
            else
            {
                SelectionCountText.Text = "";
                ToggleSelectionBtn.Content = "Chọn tất cả";
                SelectAllCheckbox.IsChecked = false;
            }
        }

        private void OnStudentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Student.IsSelected))
            {
                UpdateSelectionUI();
            }
        }

        private async void BatchLock_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn khóa {selectedStudents.Count} học sinh đã chọn?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            foreach (var student in selectedStudents)
            {
                await _session.LockStudentAsync(student.MachineId, true);
            }

            ToastService.Instance.ShowSuccess("Đã khóa", $"Đã khóa {selectedStudents.Count} học sinh");
        }

        private async void BatchUnlock_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            foreach (var student in selectedStudents)
            {
                await _session.LockStudentAsync(student.MachineId, false);
            }

            ToastService.Instance.ShowSuccess("Đã mở khóa", $"Đã mở khóa {selectedStudents.Count} học sinh");
        }

        private void BatchMessage_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            // TODO: Open message dialog
            var studentNames = string.Join(", ", selectedStudents.Select(s => s.DisplayName));
            MessageBox.Show(
                $"Tính năng gửi tin nhắn cho nhóm sẽ được thêm trong phiên bản sau.\n\nHọc sinh đã chọn: {studentNames}",
                "Gửi tin nhắn",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BatchSendFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Title = "Chọn file để gửi";

            if (fileDialog.ShowDialog() == true)
            {
                // TODO: Implement batch file sending
                ToastService.Instance.ShowInfo(
                    "Gửi file",
                    $"Đang gửi file đến {selectedStudents.Count} học sinh...");
            }
        }

        private void BatchCameraOff_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            // TODO: Implement camera control
            ToastService.Instance.ShowInfo(
                "Tắt camera",
                $"Đã tắt camera của {selectedStudents.Count} học sinh");
        }

        private void BatchMicOff_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = _session.OnlineStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any()) return;

            // TODO: Implement microphone control
            ToastService.Instance.ShowInfo(
                "Tắt mic",
                $"Đã tắt mic của {selectedStudents.Count} học sinh");
        }
        private void CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ConnectionCodeText.Text) && ConnectionCodeText.Text != "------")
            {
                Clipboard.SetText(ConnectionCodeText.Text);
                ToastService.Instance.ShowSuccess("Đã sao chép", "Mã kết nối đã được lưu vào clipboard");
            }
        }
    }
}
