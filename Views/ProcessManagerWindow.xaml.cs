using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class ProcessManagerWindow : Window
    {
        private readonly Student _student;
        private readonly SessionManager _session;

        public ProcessManagerWindow(Student student)
        {
            InitializeComponent();
            _student = student;
            _session = SessionManager.Instance;

            TitleText.Text = $"QUẢN LÝ ỨNG DỤNG - {_student.DisplayName}";

            _session.ProcessListReceived += OnProcessListReceived;

            Loaded += (s, e) => RefreshList();
            Unloaded += (s, e) => _session.ProcessListReceived -= OnProcessListReceived;
        }

        private void RefreshList()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            _ = _session.NetworkServer.SendToClientAsync(_student.MachineId, new NetworkMessage
            {
                Type = MessageType.ProcessListRequest,
                SenderId = "server"
            });
        }

        private void OnProcessListReceived(object? sender, ProcessListReceivedEventArgs e)
        {
            if (e.ClientId != _student.MachineId) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                ProcessGrid.ItemsSource = e.Processes;
            });
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private async void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int pid)
            {
                // Find process info for confirmation (optional)
                var process = (ProcessGrid.ItemsSource as List<ProcessInfo>)?.FirstOrDefault(p => p.Id == pid);
                string procName = process?.Name ?? "Unknown";

                var result = MessageBox.Show($"Bạn có chắc chắn muốn tắt ứng dụng {procName} (PID: {pid})?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var action = new ProcessAction { ProcessId = pid, Action = "Kill" };
                    await _session.NetworkServer.SendToClientAsync(_student.MachineId, new NetworkMessage
                    {
                        Type = MessageType.ProcessKillCommand,
                        SenderId = "server",
                        Payload = JsonSerializer.Serialize(action)
                    });

                    ToastService.Instance.ShowInfo("Đã gửi lệnh", $"Yêu cầu tắt {procName}...");

                    // Auto refresh after a short delay
                    await System.Threading.Tasks.Task.Delay(2000);
                    RefreshList();
                }
            }
        }
    }
}
