using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class FileCollectionWindow : Window
    {
        private readonly SessionManager _session;

        public FileCollectionWindow()
        {
            InitializeComponent();
            _session = SessionManager.Instance;
            StudentGrid.ItemsSource = _session.OnlineStudents;
        }

        private async void StartCollection_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourcePathBox.Text))
            {
                MessageBox.Show("Vui lòng nhập đường dẫn thư mục nguồn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var request = new FileCollectionRequest
            {
                RemotePath = SourcePathBox.Text.Trim(),
                Recursive = RecursiveCheck.IsChecked ?? true,
                Extensions = ExtensionsBox.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToList()
            };

            // Reset status
            foreach (var student in _session.OnlineStudents)
            {
                student.CollectionStatus = "Đang gửi yêu cầu...";
            }

            try
            {
                await _session.NetworkServer.BroadcastToAllAsync(new NetworkMessage
                {
                    Type = MessageType.FileCollectionRequest,
                    SenderId = "server",
                    Payload = System.Text.Json.JsonSerializer.Serialize(request)
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi yêu cầu: {ex.Message}");
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CollectedFiles");
                if (_session.CurrentSession != null)
                {
                    baseDir = Path.Combine(baseDir, _session.CurrentSession.Id.ToString());
                }

                Directory.CreateDirectory(baseDir);
                Process.Start("explorer.exe", baseDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở thư mục: {ex.Message}");
            }
        }
    }
}
